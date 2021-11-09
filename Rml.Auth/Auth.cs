using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Rml.Auth
{
    public static class Auth
    {
        public static async Task<string> GetTokenMsalAsync(string url, string clientId, string tenantId, CancellationToken cancellationToken)
        {
            var uri = new Uri(url);

            var publicClientApp = PublicClientApplicationBuilder
                .Create(clientId)
                .WithDefaultRedirectUri()
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .Build();

            var authResult = await publicClientApp
                .AcquireTokenInteractive(new[]
                {
                    $"{uri.Scheme}://{uri.Authority}//AllSites.Manage"
                })
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken);

            return authResult.AccessToken;
        }
    }
}