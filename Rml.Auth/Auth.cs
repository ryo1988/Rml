using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Rml.Auth
{
    public static class Auth
    {
        private static readonly Dictionary<(string clientId, string tenantId), IPublicClientApplication> Applications =
            new();
        private static readonly Dictionary<(string clientId, string tenantId), IAccount> Accounts =
            new();

        public static async Task<AuthenticationResult> AuthMsalAsync(string url, string clientId, string tenantId, IAccount? account, CancellationToken cancellationToken)
        {
            var uri = new Uri(url);
            var scope = $"{uri.Scheme}://{uri.Authority}//AllSites.Manage";
            var applicationKey = (clientId, tenantId);

            if (Applications.TryGetValue(applicationKey, out var publicClientApplication) is false)
            {
                publicClientApplication = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithRedirectUri("http://localhost")
                    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                    .Build();

                Applications.Add(applicationKey, publicClientApplication);
            }

            if (account is null && Accounts.TryGetValue(applicationKey, out var cachedAccount))
            {
                account = cachedAccount;
            }

            if (account is null)
            {
                account = (await publicClientApplication.GetAccountsAsync()).FirstOrDefault();
            }

            try
            {
                if (account is null)
                {
                    return CacheAccount(await Task.Run(AcquireTokenInteractive, cancellationToken));
                }

                return CacheAccount(await Task.Run(AcquireTokenSilent, cancellationToken));
            }
            catch (MsalUiRequiredException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                return CacheAccount(await Task.Run(AcquireTokenInteractive, cancellationToken));
            }

            AuthenticationResult CacheAccount(AuthenticationResult authenticationResult)
            {
                if (authenticationResult.Account is not null)
                {
                    Accounts[applicationKey] = authenticationResult.Account;
                }

                return authenticationResult;
            }

            async Task<AuthenticationResult> AcquireTokenSilent()
            {
                return await publicClientApplication
                    .AcquireTokenSilent(new[]
                    {
                        scope
                    }, account)
                    .ExecuteAsync(cancellationToken);
            }

            async Task<AuthenticationResult> AcquireTokenInteractive()
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