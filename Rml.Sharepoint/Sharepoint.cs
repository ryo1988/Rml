using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

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
    }
}