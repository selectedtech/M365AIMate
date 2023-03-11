using Azure.Identity;
using Microsoft.Graph.Beta;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
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
            return new OpenAIService(new OpenAiOptions(){
                ApiKey = _openAIKey
            });
        }
    }
}
