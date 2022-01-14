using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;

namespace Rml.Sharepoint
{
    public static class Sharepoint
    {
        public static async Task<ClientContext?> GetClientContextAsync(string url, string token)
        {
            var clientContext = new ClientContext(url);
            clientContext.ExecutingWebRequest += (_, args) =>
                args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + token;

            try
            {
                clientContext.Load(clientContext.Web);
                await clientContext.ExecuteQueryAsync();
            }
            catch
            {
                return null;
            }

            return clientContext;
        }

        public static async Task<Folder?> GetRootFolder(ClientContext clientContext)
        {
            clientContext.Load(clientContext.Web, p => p.Title);
            await clientContext.ExecuteQueryAsync();

            clientContext.Load(clientContext.Web.Lists);
            await clientContext.ExecuteQueryAsync();

            var list = clientContext.Web.Lists.GetByTitle("ドキュメント");
            clientContext.Load(list);
            clientContext.Load(list.RootFolder);
            await clientContext.ExecuteQueryAsync();

            return list.RootFolder;
        }

        public static async Task<(Folder fileFolder, string fileName)> GetPathFolderAndFileName(Folder folder, string path)
        {
            var fileName = path.Split('\\', '/').TakeLast(1).Single();
            var folderPaths = path.Split('\\', '/').SkipLast(1);
            foreach (var pathItem in folderPaths)
            {
                folder.Context.Load(folder, o => o.Folders);
                await folder.Context.ExecuteQueryAsync();
                folder = folder.Folders.FirstOrDefault(o => o.Name == pathItem) ?? await folder.CreateFolderAsync(pathItem);
            }

            return (folder, fileName);
        }

        public static async Task<File?> GetFileAsync(Folder folder, string path)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileName(folder, path);
            return await fileFolder.GetFileAsync(fileName);
        }

        public static async Task<Stream> PullFileStream(File file)
        {
            var memoryStream = new MemoryStream();
            var openBinaryStream = file.OpenBinaryStream();
            await file.Context.ExecuteQueryAsync();
            await using var stream = openBinaryStream.Value;
            await stream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }

        public static async Task<File> PushFileStream(Folder folder, string path, Stream stream)
        {
            var (fileFolder, fileName) = await GetPathFolderAndFileName(folder, path);
            var uploadFile = await fileFolder.UploadFileAsync(fileName, stream, true);
            if (uploadFile is null) throw new InvalidOperationException();

            return uploadFile;
        }
    }
}