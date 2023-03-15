﻿//// Copyright (c) Microsoft Corporation.
//// Licensed under the MIT License.
//namespace Microsoft.TeamsFx;

//using global::Azure.Core;

//using Microsoft.Extensions.Logging;
//using Microsoft.Graph;

///// <summary>
///// Microsoft Graph auth provider for Teams Framework.
///// </summary>
//public class MsGraphAuthProvider : Kiota.Abstractions.Authentication.IAuthenticationProvider
//{
//    private const string DefaultScope = ".default";
//    private readonly TokenCredential _credential;
//    private readonly ILogger _logger;
//    readonly internal string[] _scopes;

//    /// <summary>
//    /// Constructor of MsGraphAuthProvider.
//    /// </summary>
//    /// <param name="credential">Credential used to invoke Microsoft Graph APIs.</param>
//    /// <param name="scopes">The string of Microsoft Token scopes of access separated by space. Default value is `.default`.</param>
//    /// <param name="logger">Logger of MsGraphAuthProvider Class.</param>
//    /// <returns>
//    /// An instance of MsGraphAuthProvider.
//    /// </returns>
//    public MsGraphAuthProvider(TokenCredential credential, string scopes = DefaultScope, ILogger<MsGraphAuthProvider> logger = null)
//    {
//        _credential = credential;
//        _logger = logger;
//        if (scopes == "")
//        {
//            scopes = DefaultScope;
//        }
//        _scopes = scopes.Split(' ');
//        _logger?.LogInformation($"Create Microsoft Graph Authentication Provider with scopes: {_scopes}");
//    }

//    /// <summary>
//    /// Constructor of MsGraphAuthProvider.
//    /// </summary>
//    /// <param name="credential">Credential used to invoke Microsoft Graph APIs.</param>
//    /// <param name="scopes">The scopes required for the token.</param>
//    /// <param name="logger">Logger of MsGraphAuthProvider Class.</param>
//    /// <returns>
//    /// An instance of MsGraphAuthProvider.
//    /// </returns>
//    public MsGraphAuthProvider(TokenCredential credential, string[] scopes, ILogger<MsGraphAuthProvider> logger = null)
//    {
//        _credential = credential;
//        _logger = logger;
//        if (string.Join("", scopes) == "")
//        {
//            _scopes = new string[] { DefaultScope };
//        }
//        else
//        {
//            _scopes = scopes;
//        }
//        _logger?.LogInformation($"Create Microsoft Graph Authentication Provider with scopes: {_scopes}");
//    }

//    /// <summary>
//    /// Authenticates the specified request message.
//    /// </summary>
//    /// <param name="request">The System.Net.Http.HttpRequestMessage to authenticate.</param>
//    /// <returns>The task to await.</returns>
//    public async Task AuthenticateRequestAsync(HttpRequestMessage request)
//    {
//        var tokenRequestContext = new TokenRequestContext(_scopes);
//        var accessToken = await _credential.GetTokenAsync(tokenRequestContext, new CancellationToken()).ConfigureAwait(false);
//        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);
//    }

//    /// <summary>
//    /// Get access token for Microsoft Graph API requests.
//    /// </summary>
//    /// <returns>Access token from the credential.</returns>
//    /// <exception cref="ExceptionCode.InternalError">When get access token failed due to empty token or unknown other problems.</exception>
//    /// <exception cref="ExceptionCode.TokenExpiredError">When SSO token has already expired.</exception>
//    /// <exception cref="ExceptionCode.UiRequiredError">When need user consent to get access token.</exception>
//    /// <exception cref="ExceptionCode.ServiceError">When failed to get access token from AAD server.</exception>
//    public async Task<string> GetAccessTokenAsync()
//    {
//        _logger?.LogInformation($"Get Graph Access token with {_scopes}");
//        var tokenRequestContext = new TokenRequestContext(_scopes);

//        var accessToken = await _credential.GetTokenAsync(tokenRequestContext, new CancellationToken()).ConfigureAwait(false);
//        if (accessToken.Token.Length == 0)
//        {
//            var errorMsg = "Graph access token is undefined or empty";
//            _logger?.LogError(errorMsg);
//            throw new ExceptionWithCode(errorMsg, ExceptionCode.InternalError);
//        }
//        return accessToken.Token;
//    }
//}