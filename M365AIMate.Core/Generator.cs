using M365AIMate.Core.Services;
using Microsoft.Graph.Models;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365AIMate.Core;

public class Generator
{

    string _clientId = "";
    string _clientSecret = "";
    string _tenantId = "";
    string _openAIApiKey = "";
    string _tenantBaseUrl = "";
    IPnPContextFactory _pnpContextFactory;

    SiteService _siteService;
    TeamService _teamsService;

    public Generator(string clientId, string clientSecret, string tenantId, string openAIApiKey, IPnPContextFactory pnpContextFactory, string tenantBaseUrl)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tenantId = tenantId;
        _openAIApiKey = openAIApiKey;
        _pnpContextFactory = pnpContextFactory;
        _tenantBaseUrl = tenantBaseUrl;
    }
    
    #region Sites
    private SiteService GetSiteService()
    {
        if(_siteService == null)
        {
            _siteService = new SiteService(_clientId, _clientSecret, _tenantId, "", _openAIApiKey, _pnpContextFactory, _tenantBaseUrl);
        }

        return _siteService;
    }

    public List<ISite> CreateSites(int numberOfSites)
    {
        List<ISite> resultList = new();

        return resultList;
    }

    #endregion
    
    #region Teams

    private TeamService GetTeamService()
    {
        if(_teamsService == null)
        {
            _teamsService = new TeamService(_clientId, _clientSecret, _tenantId, "Group.ReadWrite.All;Directory.ReadWrite.All", _openAIApiKey);
        }

        return _teamsService;
    }

    public async Task<List<Team>> CreateTeams(int numberOfTeams)
    {        
        var svc = GetTeamService();
        return await svc.CreateTeams(numberOfTeams);
    }
    
    public async Task DeleteTeams(List<Team> teams)
    {
        var svc = GetTeamService();
        await svc.DeleteTeams(teams);
    }

    public async Task<Team> GetTeam(string id)
    {
        var svc = GetTeamService();
        return await svc.GetTeam(id);
    }

   

    #endregion
}
