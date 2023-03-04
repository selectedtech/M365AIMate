using Azure.Identity;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Identity.Client;

namespace M365AIMate.Core;

public class GraphClientService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _scopes;
        private readonly IPublicClientApplication _publicClientApp;
        private GraphServiceClient _graphClient;

        public GraphClientService(string clientId, string clientSecret, string tenantId, string scopes)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _scopes = scopes;
          
        }

        public  GraphServiceClient GetGraphClient()
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

        public async Task<IEnumerable<Site>> GetSites()
        {
            var graphClient = GetGraphClient();
            var sites = await graphClient.Sites.GetAsync();
            return sites.Value;
        }

        public async Task<IEnumerable<SitePage>> GetPages(string siteId)
        {
            var graphClient =  GetGraphClient();
            var pages = await graphClient.Sites[siteId].Pages.GetAsync();
            return pages.Value;
        }

        public async Task<SitePage> CreatePage(string siteId, SitePage page)
        {
            var graphClient =  GetGraphClient();
            var createdPage = await graphClient.Sites[siteId].Pages.PostAsync(page);
        return createdPage;
        }

    public async Task UpdatePage(string pageId, SitePage page)
    {
        var graphClient =  GetGraphClient();
        await graphClient.Sites[page.Id].Pages[pageId].PatchAsync(page);
        }

        public async Task DeletePage(string pageId, string siteId)
        {
            var graphClient =  GetGraphClient();
            await graphClient.Sites[siteId].Pages[pageId].DeleteAsync();
        }
    }

