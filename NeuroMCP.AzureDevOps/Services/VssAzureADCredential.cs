using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace NeuroMCP.AzureDevOps.Services;

/// <summary>
/// Custom credential implementation that uses Azure.Identity for authentication with Azure DevOps
/// </summary>
public class VssAzureADCredential : VssCredentials
{
    private readonly TokenCredential _credential;

    public VssAzureADCredential(TokenCredential credential)
    {
        _credential = credential;
    }

    public override bool IsAuthenticationChallenge(HttpResponseMessage response)
    {
        return response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
    }

    public override async Task<HttpResponseMessage> HandleAuthenticationChallenge(HttpResponseMessage response)
    {
        var request = response.RequestMessage;

        // Add Bearer token from Azure Identity
        var tokenRequestContext = new TokenRequestContext(new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" }); // Azure DevOps scope
        var token = await _credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        // Clone the request (because the original request's content might have been disposed)
        var clonedRequest = await CloneHttpRequestMessageAsync(request);

        using var client = new HttpClient();
        return await client.SendAsync(clonedRequest);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Manually copy the content if needed
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            if (request.Content.Headers != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return clone;
    }

    public override VssCredentialsType CredentialType => VssCredentialsType.Oauth;
}