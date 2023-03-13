using Microsoft.Graph.Beta.Models;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;

namespace M365AIMate.Core.Services;

internal class SiteService : BaseService
{


    public SiteService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey, IPnPContextFactory pnPContextFactory, string tenantBaseUrl) : base(clientId, clientSecret, tenantId, scopes, openAIKey, pnPContextFactory, tenantBaseUrl)
    {
        
    }

 
    
    internal async Task<ISite> CreateSite()
    {
        ISite site = null;
        var pnPContextFactory = GetPnPContextFactory();

        using (var context = await pnPContextFactory.CreateAsync("SiteToWorkWith"))
        {
            // Use the GetSharePointAdmin extension method on any PnPContext 
            // to tap into the SharePoint admin features
            var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();

            var siteTitle = await GenerateText("Please create a funny 3 word intranet site name");
            var siteUrl = Uri.EscapeDataString(siteTitle);
            var siteDescription = await GenerateText(string.Format("Please create a funny intranet site description based on the site name '{0}'",siteTitle));           

            var communicationSiteToCreate = new CommunicationSiteOptions(new Uri(GetTenantBaseUrl()+"/sites/"+siteUrl), siteTitle)
            {
                Description = siteDescription,
                Language = Language.English,
                Owner = await GetRandomUserEmail()
            };

            SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
            {
                WaitForAsyncProvisioning = true
            };

            using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions))
            {
                // Do work on the created site collection via the newSiteContext
                site = newSiteContext.Site;
            }
        }

        return site;
    }

    //protected async Task<IEnumerable<Site>> GetSites()
    //{
    //    var graphClient = GetGraphClient();
    //    var sites = await graphClient.Sites.GetAsync();
    //    return sites.Value;
    //}

    //protected async Task<IEnumerable<SitePage>> GetPages(string siteId)
    //{
    //    var graphClient = GetGraphClient();
    //    var pages = await graphClient.Sites[siteId].Pages.GetAsync();
    //    return pages.Value;
    //}

    //protected async Task<SitePage> CreatePage(string siteId, SitePage page)
    //{        
    //    var graphClient = GetGraphClient();
    //    var createdPage = await graphClient.Sites[siteId].Pages.PostAsync(page);
    //    return createdPage;
    //}

    //protected async Task UpdatePage(string pageId, SitePage page)
    //{
    //    var graphClient = GetGraphClient();
    //    await graphClient.Sites[page.Id].Pages[pageId].PatchAsync(page);
    //}

    //protected async Task DeletePage(string pageId, string siteId)
    //{
    //    var graphClient = GetGraphClient ();
    //    await graphClient.Sites[siteId].Pages[pageId].DeleteAsync();
    //}
}

