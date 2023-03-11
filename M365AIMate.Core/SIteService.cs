using Azure.Identity;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Identity.Client;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services;

namespace M365AIMate.Core;

public partial class SiteService : BaseService
{

    private readonly IPublicClientApplication _publicClientApp;
    private GraphServiceClient _graphClient;
    private IPnPContextFactory _pnPContextFactory;

    public SiteService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey, IPnPContextFactory pnPContextFactory) : base(clientId, clientSecret, tenantId, scopes, openAIKey)
    {
        _pnPContextFactory = pnPContextFactory;
    }

 

    public async Task<int> CreateSite()
    {

        using (var context = await _pnPContextFactory.CreateAsync("SiteToWorkWith"))
        {
            // Use the GetSharePointAdmin extension method on any PnPContext 
            // to tap into the SharePoint admin features
            var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();

            var communicationSiteToCreate = new CommunicationSiteOptions(new Uri("https://tgodev.sharepoint.com/sites/M365AIMate-SC001"), "My communication site")
            {
                Description = "My site description",
                Language = Language.English,                
                Owner = "me@tgodev.onmicrosoft.com"
            };

            SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
            {
                WaitForAsyncProvisioning = true
            };

            using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions))
            {
                // Do work on the created site collection via the newSiteContext
            }
        }

        return 1;
    }

    public async Task<IEnumerable<Site>> GetSites()
    {
        var graphClient = GetGraphClient();
        var sites = await graphClient.Sites.GetAsync();
        return sites.Value;
    }

    public async Task<IEnumerable<SitePage>> GetPages(string siteId)
    {
        var graphClient = GetGraphClient();
        var pages = await graphClient.Sites[siteId].Pages.GetAsync();
        return pages.Value;
    }

    public async Task<SitePage> CreatePage(string siteId, SitePage page)
    {
        var graphClient = GetGraphClient();
        var createdPage = await graphClient.Sites[siteId].Pages.PostAsync(page);
        return createdPage;
    }

    public async Task UpdatePage(string pageId, SitePage page)
    {
        var graphClient = GetGraphClient();
        await graphClient.Sites[page.Id].Pages[pageId].PatchAsync(page);
    }

    public async Task DeletePage(string pageId, string siteId)
    {
        var graphClient = GetGraphClient();
        await graphClient.Sites[siteId].Pages[pageId].DeleteAsync();
    }
}

