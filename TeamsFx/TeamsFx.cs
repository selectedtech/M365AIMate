// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Microsoft.TeamsFx;

/// <summary>
/// Top Level API in TeamsFx SDK.
/// </summary>
public class TeamsFx
{
    private readonly ILogger<TeamsFx> _logger;
    //private readonly ILogger<MsGraphAuthProvider> _authLogger;

    /// <summary>
    /// The constructor of TeamsFx.
    /// </summary>
    public TeamsFx(ILogger<TeamsFx> logger)
    {
        _logger = logger;
       // _authLogger = authLogger;
    }

    /// <summary>
    /// Get Microsoft graph client.
    /// </summary>
    /// <param name="credential">Token credential instance.</param>
    /// <param name="scopes">The string of Microsoft Token scopes of access separated by space. Default value is `.default`.</param>
    /// <param name="logger">Logger of MsGraphAuthProvider class. If the value is null, it will use the logger constructed by DI during TeamsFx class initialization.</param>
    /// <returns>Graph client with specified scopes.</returns>
    public GraphServiceClient CreateMicrosoftGraphClient(TokenCredential credential, string scopes = ".default")
    {
        _logger.LogInformation("Create Microsoft Graph Client");
      //  logger ??= _authLogger;
        string[] scopeArray = scopes.Split(',');
        
        //var authProvider = new MsGraphAuthProvider(credential, scopes, logger);
        //Changing authentication to directly handing over the TokenCrendential and scopes to the GraphServiceClient
        //circumventing the need for a MsGraphProviderCredential as such
        
        var client = new GraphServiceClient(credential,scopeArray);
        return client;
    }

    /// <summary>
    /// Get Microsoft graph client.
    /// </summary>
    /// <param name="credential">Token credential instance.</param>
    /// <param name="scopes">The array of Microsoft Token scopes of access. Default value is `[.default]`.</param>
    /// <param name="logger">Logger of MsGraphAuthProvider class. If the value is null, it will use the logger constructed by DI during TeamsFx class initialization.</param>
    /// <returns>Graph client with specified scopes.</returns>
    public GraphServiceClient CreateMicrosoftGraphClient(TokenCredential credential, string[] scopes)
    {
        _logger.LogInformation("Create Microsoft Graph Client");
      //  logger ??= _authLogger;
        //var authProvider = new MsGraphAuthProvider(credential, scopes, logger);
        var client = new GraphServiceClient(credential, scopes);
        return client;
    }
}