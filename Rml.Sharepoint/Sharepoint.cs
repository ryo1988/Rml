using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;

namespace Rml.Sharepoint
{
    public static class Sharepoint
    {
        public static async Task<ClientContext> GetClientContextAsync(string url, string token)
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

        public static async Task<Folder> GetRootFolder(ClientContext clientContext)
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

        public static async Task<Stream> PullFileStream(File file)
        {
            var memoryStream = new MemoryStream();
            if (file is not null)
            {
                var openBinaryStream = file.OpenBinaryStream();
                await file.Context.ExecuteQueryAsync();
                await using var stream = openBinaryStream.Value;
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public static async Task<File> PushFileStream(Folder folder, string path, Stream stream)
        {
            var uploadFile = await folder.UploadFileAsync(path, stream, true);
            if (uploadFile is null) throw new InvalidOperationException();

            return uploadFile;
        }
    }
}