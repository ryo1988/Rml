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

        private static async ValueTask<Dictionary<string, ClientObject>> GetEntitiesInternal(Folder folder, bool useLock)
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
        
        private static async ValueTask<ClientObject?> GetEntity(Folder folder, string name)
        {
            using (await EntityCacheLock.LockAsync())
            {
                var entities = await GetEntitiesInternal(folder, false);
                return entities.TryGetValue(name, out var entity) ? entity : null;
            }
        }

        private static async ValueTask ClearEntityCache(ClientObject clientObject)
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

        public static async ValueTask ClearEntityCache()
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

        private static async ValueTask<bool> IsLocked(File? file, bool useLock)
        {
            if (file is null)
                return false;

            using (useLock ? await ClientContextLocks[file.Context].LockAsync() : Disposable.Create(null))
            {
                file.Context.Load(file, o => o.CheckedOutByUser);
                await file.Context.ExecuteQueryRetryAsync();
            }

            return file.CheckedOutByUser.ServerObjectIsNull switch
            {
                false => true,
                true => false,
                _ => throw new InvalidOperationException(),
            };
        }

        private static async ValueTask UpdateLockedFile(File file, bool useLock)
        {
            using (useLock ? await ClientContextLocks[file.Context].LockAsync() : Disposable.Create(null))
            {
                file.CheckIn("", CheckinType.MajorCheckIn);
                file.CheckOut();
                await file.Context.ExecuteQueryRetryAsync();
                await ClearEntityCache(file);
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

        public static async ValueTask<Folder?> GetRootFolder(ClientContext clientContext, bool forceCheckOut)
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

        public static async ValueTask<(Folder fileFolder, string fileName)> GetPathFolderAndFileName(Folder folder, string path)
        {
            var fileName = path.Split('\\', '/').TakeLast(1).Single();
            var folderPaths = path.Split('\\', '/').SkipLast(1);
            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    var currentFolder = await GetEntity(folder, pathItem) as Folder;
                    if (currentFolder is null)
                    {
                        currentFolder = await folder.CreateFolderAsync(pathItem);
                        await ClearEntityCache(folder);
                    }

                    folder = currentFolder;
                }
            }

            return (folder, fileName);
        }

        public static async ValueTask<File?> GetFileAsync(Folder folder, string path, bool createWhenNotExist)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileName(folder, path);
            File? file;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                file = await GetEntity(fileFolder, fileName) as File;
                if (createWhenNotExist && file is null)
                {
                    await using var stream = new MemoryStream();
                    file = await fileFolder.UploadFileAsync(fileName, stream, false);
                    await ClearEntityCache(fileFolder);
                }
            }

            return file;
        }
        
        public static async ValueTask DeleteFile(Folder folder, string path)
        {
            var getFile = await GetFileAsync(folder, path, false);
            if (getFile is null)
                return;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                getFile.DeleteObject();
                await getFile.Context.ExecuteQueryRetryAsync();
                await ClearEntityCache(folder);
                await ClearEntityCache(getFile);
            }
        }

        public static async ValueTask<Stream> PullFileStream(File file)
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

        public static async ValueTask<File> PushFileStream(Folder folder, string path, Stream stream)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileName(folder, path);
            File? uploadFile;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                uploadFile = await fileFolder.UploadFileAsync(fileName, stream, true);
                if (await IsLocked(uploadFile, false))
                {
                    await UpdateLockedFile(uploadFile, false);
                }
                await ClearEntityCache(fileFolder);
            }
            if (uploadFile is null) throw new InvalidOperationException();

            return uploadFile;
        }

        public static async ValueTask<Folder> CreateFolder(Folder folder, string path)
        {
            var folderPaths = path.Split('\\', '/');

            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    var currentFolder = await GetEntity(folder, pathItem) as Folder;
                    if (currentFolder is null)
                    {
                        currentFolder = await folder.CreateFolderAsync(pathItem);
                        await ClearEntityCache(folder);
                    }

                    folder = currentFolder;
                }
            }

            return folder;
        }

        public static async ValueTask<Folder?> GetFolder(Folder folder, string path)
        {
            var folderPaths = path.Split('\\', '/');

            var currentFolder = folder;
            foreach (var pathItem in folderPaths)
            {
                currentFolder = await GetEntity(currentFolder, pathItem) as Folder;

                if (currentFolder is null)
                    break;
            }

            return currentFolder;
        }
        
        public static async ValueTask DeleteFolder(Folder folder, string path)
        {
            var getFolder = await GetFolder(folder, path);
            if (getFolder is null)
                return;
            
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                getFolder.DeleteObject();
                await getFolder.Context.ExecuteQueryRetryAsync();
                await ClearEntityCache(folder);
            }
        }

        public static async ValueTask<string[]> GetEntities(Folder folder)
        {
            return (await GetEntitiesInternal(folder, true))
                .Select(o => o.Key)
                .ToArray();
        }

        public static async ValueTask<bool> IsLocked(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, false);
            return await IsLocked(file, true);
        }

        public static async ValueTask<bool> Lock(Folder folder, string path, bool force)
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
                await ClearEntityCache(file);
            }

            return true;
        }
        
        public static async ValueTask<bool> Unlock(Folder folder, string path)
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
                await ClearEntityCache(file);
            }

            return true;
        }

        public static async ValueTask<string?> GetLockedInfo(Folder folder, string path)
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