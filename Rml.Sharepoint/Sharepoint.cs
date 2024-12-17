using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Nito.AsyncEx;
using Nito.Disposables;
using File = Microsoft.SharePoint.Client.File;

namespace Rml.Sharepoint
{
    public static class Sharepoint
    {
        private static readonly AsyncLock ClientContextsLock = new();

        private static readonly Dictionary<(string url, string userAgent, string token), ClientContext> ClientContexts =
            new();

        private static readonly AsyncLock RootFoldersLock = new();
        private static readonly Dictionary<ClientContext, Folder> RootFolders = new();

        private static readonly Dictionary<ClientRuntimeContext, AsyncLock> ClientContextLocks = new();

        private static readonly AsyncLock EntityCacheLock = new();
        private static readonly Dictionary<Folder, Dictionary<string, ClientObject>> EntityCache = new();
        private static readonly Dictionary<ClientObject, Folder> EntityFolderCache = new();

        private static async ValueTask<Dictionary<string, ClientObject>> GetEntitiesInternalAsync(Folder folder, bool useLock)
        {
            using (useLock ? await EntityCacheLock.LockAsync() : Disposable.Create(null))
            {
                if (EntityCache.TryGetValue(folder, out var entities) is false)
                {
                    var folders = folder.Folders;
                    var files = folder.Files;
                    
                    folder.Context.Load(folders, o => o.Include(oo => oo.Name));
                    folder.Context.Load(files, o => o.Include(oo => oo.Name));
                    await folder.Context.ExecuteQueryRetryAsync();

                    entities = folders.AsEnumerable()
                        .Select(o => (name:o.Name, value:(ClientObject)o))
                        .Concat(files.AsEnumerable().Select(o => (name:o.Name, value:(ClientObject)o)))
                        .ToDictionary(o => o.name, o => o.value);
                    
                    EntityCache.Add(folder, entities);
                    foreach (var keyValuePair in entities)
                    {
                        EntityFolderCache.Add(keyValuePair.Value, folder);
                    }
                }

                return entities;
            }
        }
        
        private static async ValueTask<ClientObject?> GetEntityAsync(Folder folder, string name)
        {
            using (await EntityCacheLock.LockAsync())
            {
                var entities = await GetEntitiesInternalAsync(folder, false);
                return entities.TryGetValue(name, out var entity) ? entity : null;
            }
        }

        private static async ValueTask ClearEntityCacheAsync(ClientObject clientObject)
        {
            using (await EntityCacheLock.LockAsync())
            {
                switch (clientObject)
                {
                    case Folder folder:
                    {
                        if (EntityCache.Remove(folder, out var entities))
                        {
                            foreach (var keyValuePair in entities)
                            {
                                EntityFolderCache.Remove(keyValuePair.Value);
                            }
                        }
                    }
                        break;
                    case File file:
                    {
                        if (EntityFolderCache.Remove(file, out var parentFolder))
                        {
                            if (EntityCache.TryGetValue(parentFolder, out var entities))
                            {
                                entities[file.Name] = file;
                            }
                            
                            EntityFolderCache.Add(file, parentFolder);
                        }
                    }
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public static async ValueTask ClearEntityCacheAsync()
        {
            var disposables = await ClientContextLocks
                .Select(async o => await o.Value.LockAsync())
                .WhenAll();
            
            using (new CollectionDisposable(disposables))
            {
                using (await EntityCacheLock.LockAsync())
                {
                    EntityCache.Clear();
                    EntityFolderCache.Clear();
                }
            }
        }

        private static async ValueTask<bool> IsLockedAsync(File? file, bool useLock)
        {
            if (file is null)
                return false;

            using (useLock ? await ClientContextLocks[file.Context].LockAsync() : Disposable.Create(null))
            {
                file.Context.Load(file, o => o.CheckOutType);
                await file.Context.ExecuteQueryRetryAsync();
            }

            return file.CheckOutType switch
            {
                CheckOutType.None => false,
                CheckOutType.Offline => true,
                CheckOutType.Online => true,
                _ => throw new InvalidOperationException()
            };
        }

        private static async ValueTask UpdateLockedFileAsync(File file, bool useLock)
        {
            using (useLock ? await ClientContextLocks[file.Context].LockAsync() : Disposable.Create(null))
            {
                file.CheckIn("", CheckinType.MajorCheckIn);
                file.CheckOut();
                await file.Context.ExecuteQueryRetryAsync();
                await ClearEntityCacheAsync(file);
            }
        }
        
        public static async ValueTask<ClientContext?> GetClientContextAsync(string url, string userAgent, string token)
        {
            using (await ClientContextsLock.LockAsync())
            {
                if (ClientContexts.TryGetValue((url, userAgent, token), out var clientContext) is false)
                {
                    clientContext = new ClientContext(url);
                    clientContext.ExecutingWebRequest += (_, args) =>
                    {
                        args.WebRequestExecutor.WebRequest.UserAgent = userAgent;
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + token;
                    };

                    try
                    {
                        clientContext.Load(clientContext.Web);
                        await clientContext.ExecuteQueryRetryAsync();
                        ClientContexts.Add((url, userAgent, token), clientContext);
                        ClientContextLocks.Add(clientContext, new AsyncLock());
                    }
                    catch
                    {
                        return null;
                    }
                }

                return clientContext;
            }
        }

        public static async ValueTask<Folder?> GetRootFolderAsync(ClientContext clientContext, bool forceCheckOut)
        {
            using (await RootFoldersLock.LockAsync())
            {
                if (RootFolders.TryGetValue(clientContext, out var rootFolder) is false)
                {
                    clientContext.Load(clientContext.Web.Lists);
                    var list = clientContext.Web.Lists.GetByTitle("ドキュメント");
                    clientContext.Load(list, o => o.ForceCheckout);
                    await clientContext.ExecuteQueryRetryAsync();
                    if (list.ForceCheckout != forceCheckOut)
                    {
                        list.ForceCheckout = forceCheckOut;
                        list.Update();
                    }

                    clientContext.Load(list.RootFolder);
                    await clientContext.ExecuteQueryRetryAsync();

                    rootFolder = list.RootFolder;
                    
                    RootFolders.Add(clientContext, rootFolder);
                }
                
                return rootFolder;
            }
        }

        public static async ValueTask<(Folder fileFolder, string fileName)> GetPathFolderAndFileNameAsync(Folder folder, string path)
        {
            var fileName = path.Split('\\', '/').TakeLast(1).Single();
            var folderPaths = path.Split('\\', '/').SkipLast(1);
            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    var currentFolder = await GetEntityAsync(folder, pathItem) as Folder;
                    if (currentFolder is null)
                    {
                        try
                        {
                            currentFolder = await folder.CreateFolderAsync(pathItem);
                            await ClearEntityCacheAsync(folder);
                        }
                        catch
                        {
                            // Cacheが古い可能性があるのでCacheクリア
                            await ClearEntityCacheAsync(folder);
                            currentFolder = await GetEntityAsync(folder, pathItem) as Folder;
                            if (currentFolder is null)
                                throw;
                        }
                    }

                    folder = currentFolder;
                }
            }

            return (folder, fileName);
        }

        public static async ValueTask<File?> GetFileAsync(Folder folder, string path, bool createWhenNotExist)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileNameAsync(folder, path);
            File? file;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                file = await GetEntityAsync(fileFolder, fileName) as File;
                if (createWhenNotExist && file is null)
                {
                    try
                    {
                        await using var stream = new MemoryStream();
                        file = await fileFolder.UploadFileAsync(fileName, stream, false);
                        await ClearEntityCacheAsync(fileFolder);
                    }
                    catch
                    {
                        // Cacheが古い可能性があるのでCacheクリア
                        await ClearEntityCacheAsync(folder);
                        file = await GetEntityAsync(fileFolder, fileName) as File;
                        if (file is null)
                            throw;
                    }
                }
            }

            return file;
        }
        
        public static async ValueTask DeleteFileAsync(Folder folder, File file)
        {
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                file.DeleteObject();
                await file.Context.ExecuteQueryRetryAsync();
                await ClearEntityCacheAsync(folder);
                await ClearEntityCacheAsync(file);
            }
        }
        
        public static async ValueTask DeleteFileAsync(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, false);
            if (file is null)
                return;
            await DeleteFileAsync(folder, file);
        }

        public static async ValueTask<Stream> PullFileStreamAsync(File file)
        {
            var memoryStream = new MemoryStream();
            ClientResult<Stream> openBinaryStream;
            using (await ClientContextLocks[file.Context].LockAsync())
            {
                openBinaryStream = file.OpenBinaryStream();
                await file.Context.ExecuteQueryRetryAsync();
                
                await using var stream = openBinaryStream.Value;
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public static async ValueTask<File> PushFileStreamAsync(Folder folder, string path, Stream stream)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileNameAsync(folder, path);
            File? uploadFile;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                uploadFile = await fileFolder.UploadFileAsync(fileName, stream, true);
                if (await IsLockedAsync(uploadFile, false))
                {
                    await UpdateLockedFileAsync(uploadFile, false);
                }
                await ClearEntityCacheAsync(fileFolder);
            }
            if (uploadFile is null) throw new InvalidOperationException();

            return uploadFile;
        }

        public static async ValueTask<Folder> CreateFolderAsync(Folder folder, string path)
        {
            var folderPaths = path.Split('\\', '/');

            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    var currentFolder = await GetEntityAsync(folder, pathItem) as Folder;
                    if (currentFolder is null)
                    {
                        currentFolder = await folder.CreateFolderAsync(pathItem);
                        await ClearEntityCacheAsync(folder);
                    }

                    folder = currentFolder;
                }
            }

            return folder;
        }

        public static async ValueTask<Folder?> GetFolderAsync(Folder folder, string path)
        {
            var folderPaths = path.Split('\\', '/');

            var currentFolder = folder;
            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    currentFolder = await GetEntityAsync(currentFolder, pathItem) as Folder;

                    if (currentFolder is null)
                        break;
                }
            }

            return currentFolder;
        }
        
        public static async ValueTask DeleteFolderAsync(Folder parentFolder, Folder folder)
        {
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                folder.DeleteObject();
                await folder.Context.ExecuteQueryRetryAsync();
                await ClearEntityCacheAsync(parentFolder);
            }
        }
        
        public static async ValueTask DeleteFolderAsync(Folder parentFolder, string path)
        {
            var folder = await GetFolderAsync(parentFolder, path);
            if (folder is null)
                return;
            await DeleteFolderAsync(parentFolder, folder);
        }

        public static async ValueTask<string[]> GetEntitiesAsync(Folder folder)
        {
            return (await GetEntitiesInternalAsync(folder, true))
                .Select(o => o.Key)
                .ToArray();
        }

        public static async ValueTask<bool> IsLockedAsync(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, false);
            return await IsLockedAsync(file, true);
        }

        public static async ValueTask<bool> LockAsync(Folder folder, string path, bool force)
        {
            var file = await GetFileAsync(folder, path, true);
            if (file is null)
                throw new InvalidOperationException();

            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                if (force)
                    file.UndoCheckOut();
                
                file.CheckOut();
                try
                {
                    await file.Context.ExecuteQueryRetryAsync();
                }
                catch
                {
                    return false;
                }
                await ClearEntityCacheAsync(file);
            }

            return true;
        }
        
        public static async ValueTask<bool> UnlockAsync(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, false);
            if (file is null)
                return false;

            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                file.CheckIn("", CheckinType.MajorCheckIn);
                try
                {
                    await file.Context.ExecuteQueryRetryAsync();
                }
                catch
                {
                    return false;
                }
                await ClearEntityCacheAsync(file);
            }

            return true;
        }

        public static async ValueTask<string?> GetLockedInfoAsync(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, true);
            if (file is null)
                throw new InvalidOperationException();

            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                try
                {
                    file.Context.Load(file.CheckedOutByUser,
                        o => o.Title, o => o.LoginName);
                    await file.Context.ExecuteQueryRetryAsync();

                    if (file.CheckedOutByUser.ServerObjectIsNull is true)
                        return null;

                    return file.CheckedOutByUser.Title;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}