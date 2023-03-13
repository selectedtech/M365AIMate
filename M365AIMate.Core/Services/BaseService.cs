using Azure.Identity;
using Beta = Microsoft.Graph.Beta;
using Microsoft.Graph.Models;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using PnP.Core.Services;
using Microsoft.Graph;

namespace M365AIMate.Core.Services;

internal abstract class BaseService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string _scopes;
    private readonly string _tenantBaseUrl; 
    private Microsoft.Graph.GraphServiceClient _graphClient;
    private Microsoft.Graph.Beta.GraphServiceClient _betaGraphClient;
    private readonly string _openAIKey;
    private OpenAIService _openAIService;
    private IPnPContextFactory _pnPContextFactory;


    //Ctor if you need to create Sites with PnP
    public BaseService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey, IPnPContextFactory pnPContextFactory, string tenantBaseUrl)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tenantId = tenantId;
        _scopes = scopes;
        _openAIKey = openAIKey;
        _pnPContextFactory = pnPContextFactory;
        _tenantBaseUrl = tenantBaseUrl;
    }        

    //Ctor if you only need to use Graph
    public BaseService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tenantId = tenantId;
        _scopes = scopes;
        _openAIKey = openAIKey;
        _pnPContextFactory = null;
    }

    #region Services
    protected IPnPContextFactory GetPnPContextFactory()
    {
        return _pnPContextFactory;
    }

    protected string GetTenantBaseUrl()
    {
        return _tenantBaseUrl.TrimEnd('/');
    }

    protected Beta.GraphServiceClient GetBetaGraphClient()
    {
        if (_betaGraphClient == null)
        {
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                    _tenantId, _clientId, _clientSecret, options);

            _betaGraphClient = new Beta.GraphServiceClient(clientSecretCredential);

        }

        return _betaGraphClient;
    }

    protected GraphServiceClient GetGraphClient()
    {
        if (_graphClient == null)
        {
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                    _tenantId, _clientId, _clientSecret, options);

            _graphClient = new GraphServiceClient(clientSecretCredential);

        }

        return _graphClient;
    }

    protected OpenAIService GetOpenAIService()
    {
        if(_openAIService == null)
        { 
            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = _openAIKey
            });
        }

        return _openAIService;
    }
    #endregion

    #region OpenAI
    protected async Task<string> GenerateText(string prompt)
    {
        var openAIService = GetOpenAIService();

        var completionResult = await openAIService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = prompt,
            Model = Models.TextDavinciV3
        });

        return completionResult.Choices.FirstOrDefault().Text.Replace("\n", "");

    }
    #endregion

    #region Graph Base Methods

    protected async Task<string> GetRandomUserId()
    {
        var graphClient = GetGraphClient();
        var users = await graphClient.Users.GetAsync();
        return users.Value[new Random().Next(0, users.Value.Count)].Id;
    }

    protected async Task<string> GetRandomUserEmail()
    {
        var graphClient = GetGraphClient();
        var users = await graphClient.Users.GetAsync();
        return users.Value[new Random().Next(0, users.Value.Count)].Mail;
    }


    #endregion
}
