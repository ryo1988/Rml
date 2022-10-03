using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Nito.AsyncEx;
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
                    folder.Context.Load(folder, o => o.Folders);
                    await folder.Context.ExecuteQueryRetryAsync();
                }
                
                folder = folder.Folders.FirstOrDefault(o => o.Name == pathItem) ??
                         await folder.CreateFolderAsync(pathItem);
            }

            return (folder, fileName);
        }

        public static async ValueTask<File?> GetFileAsync(Folder folder, string path, bool createWhenNotExist)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileName(folder, path);
            File? file;
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                fileFolder.Context.Load(fileFolder, o => o.Files);
                await fileFolder.Context.ExecuteQueryRetryAsync();

                file = fileFolder.Files.SingleOrDefault(o => o.Name == fileName);
                if (createWhenNotExist && file is null)
                {
                    await using var stream = new MemoryStream();
                    file = await fileFolder.UploadFileAsync(fileName, stream, false);
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
            }
            await using var stream = openBinaryStream.Value;
            await stream.CopyToAsync(memoryStream);

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
                    folder.Context.Load(folder, o => o.Folders);
                    await folder.Context.ExecuteQueryRetryAsync();
                }
                folder = folder.Folders.FirstOrDefault(o => o.Name == pathItem) ??
                         await folder.CreateFolderAsync(pathItem);
            }

            return folder;
        }

        public static async ValueTask<Folder?> GetFolder(Folder folder, string path)
        {
            var folderPaths = path.Split('\\', '/');

            var currentFolder = folder;
            foreach (var pathItem in folderPaths)
            {
                using (await ClientContextLocks[folder.Context].LockAsync())
                {
                    currentFolder.Context.Load(currentFolder, o => o.Folders);
                    await currentFolder.Context.ExecuteQueryRetryAsync();
                }
                currentFolder = currentFolder.Folders.FirstOrDefault(o => o.Name == pathItem);

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
            }
        }

        public static async ValueTask<string[]> GetEntities(Folder folder)
        {
            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                folder.Context.Load(folder, o => o.Folders);
                folder.Context.Load(folder, o => o.Files);
                await folder.Context.ExecuteQueryRetryAsync();
            }

            return folder.Folders
                .Select(o => o.Name)
                .Concat(folder.Files.Select(o => o.Name))
                .ToArray();
        }

        public static async ValueTask<bool> IsLocked(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, false);
            if (file is null)
                return false;

            using (await ClientContextLocks[folder.Context].LockAsync())
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

        public static async ValueTask<bool> Lock(Folder folder, string path)
        {
            var file = await GetFileAsync(folder, path, true);
            if (file is null)
                throw new InvalidOperationException();

            using (await ClientContextLocks[folder.Context].LockAsync())
            {
                file.CheckOut();
                await file.Context.ExecuteQueryRetryAsync();
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
                await file.Context.ExecuteQueryRetryAsync();
            }

            return true;
        }
    }
}