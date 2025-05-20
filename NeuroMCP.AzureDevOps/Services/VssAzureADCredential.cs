using Azure.Core;
using Azure.Identity;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMCP.AzureDevOps.Services
{
    /// <summary>
    /// VSS credential implementation using Azure AD authentication
    /// </summary>
    public class VssAzureADCredential : VssCredentials
    {
        private readonly TokenCredential _tokenCredential;
        private readonly string[] _scopes;

        public VssAzureADCredential(TokenCredential tokenCredential, string[] scopes)
        {
            _tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
            _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        }

        // These methods should not be implemented as overrides if they don't exist in the base class
        public bool IsAuthenticationChallenge(HttpResponseMessage response)
        {
            return response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }

        public async Task<bool> HandleAuthenticationChallenge(HttpResponseMessage response)
        {
            if (!IsAuthenticationChallenge(response))
            {
                return false;
            }

            // Get a new access token
            var token = await GetTokenAsync();
            var request = response.RequestMessage;

            if (request != null)
            {
                // Update the authorization header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            return false;
        }

        private async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            var accessToken = await _tokenCredential.GetTokenAsync(
                new TokenRequestContext(_scopes),
                cancellationToken);

            return accessToken.Token;
        }

        // Use property instead of override if CredentialType is not in the base class
        public string CredentialType => "AzureAD";
    }
}