using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Rml.Auth
{
    public static class Auth
    {
        private static readonly Dictionary<(string clientId, string tenantId), IPublicClientApplication> Applications =
            new();

        public static async Task<AuthenticationResult> AuthMsalAsync(string url, string clientId, string tenantId, IAccount account, CancellationToken cancellationToken)
        {
            var uri = new Uri(url);
            var scope = $"{uri.Scheme}://{uri.Authority}//AllSites.Manage";

            IPublicClientApplication publicClientApplication;
            if (Applications.TryGetValue((clientId, tenantId), out publicClientApplication) is false)
            {
                publicClientApplication = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithDefaultRedirectUri()
                    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                    .Build();

                Applications.Add((clientId, tenantId), publicClientApplication);
            }

            try
            {
                if (account is null)
                {
                    return await AcquireTokenInteractive();
                }

                return await AcquireTokenSilent();
            }
            catch (MsalUiRequiredException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                return await AcquireTokenInteractive();
            }

            async ValueTask<AuthenticationResult> AcquireTokenSilent()
            {
                return await publicClientApplication
                    .AcquireTokenSilent(new[]
                    {
                        scope
                    }, account)
                    .ExecuteAsync(cancellationToken);
            }

            async ValueTask<AuthenticationResult> AcquireTokenInteractive()
            {
                if (account is null)
                {
                    return await publicClientApplication
                        .AcquireTokenInteractive(new[]
                        {
                            scope
                        })
                        .WithUseEmbeddedWebView(false)
                        .ExecuteAsync(cancellationToken);
                }

                return await publicClientApplication
                    .AcquireTokenInteractive(new[]
                    {
                        scope
                    })
                    .WithUseEmbeddedWebView(false)
                    .WithAccount(account)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync(cancellationToken);
            }
        }

        public static async Task<string> GetTokenMsalAsync(string url, string clientId, string tenantId, CancellationToken cancellationToken)
        {
            return (await AuthMsalAsync(url, clientId, tenantId, null, cancellationToken)).AccessToken;
        }
    }
}