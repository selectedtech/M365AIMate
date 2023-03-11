using Azure.Identity;
using Microsoft.Graph.Beta;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365AIMate.Core
{
    public abstract class BaseService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _scopes;
        private GraphServiceClient _graphClient;
        private readonly string _openAIKey;
        private OpenAIService _openAIService;

        public BaseService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _scopes = scopes;
            _openAIKey = openAIKey;
        }

        public GraphServiceClient GetGraphClient()
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

        public OpenAIService GetOpenAIService()
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

        public async Task<string> GenerateText(string prompt)
        {
            var openAIService = GetOpenAIService();

            var completionResult = await openAIService.Completions.CreateCompletion(new CompletionCreateRequest()
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3
            });

            return completionResult.Choices.FirstOrDefault().Text.Replace("\n", "");

        }
    }
}
