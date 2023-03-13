// See https://aka.ms/new-console-template for more information
using M365AIMate.Core;
using Microsoft.Extensions.Configuration;
using PnP.Core.Services.Builder.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using static System.Formats.Asn1.AsnWriter;
using OpenAI.GPT3.Extensions;
using M365AIMate.Core.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var secretConfig = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<Program>()
                .Build();

// Add and configure PnPCore and PnPCoreAuth services
// Check out the appsettings.json for the configuration details
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddPnPCore();
        services.Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"));
        services.AddPnPCoreAuthentication();
        services.Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"));
    })
    .UseConsoleLifetime()
    .Build();

await host.StartAsync();

Console.WriteLine("Hello, World!");


var clientId = secretConfig["clientID"];
var clientSecret = secretConfig["clientSecret"];
var tenantId = secretConfig["tenantID"];
var openAIApiKey = secretConfig["ChatGPTApiKey"];
var tenantBaseUrl = configuration["PnPCore:Sites:SiteToWorkWith:SiteUrl"];

using (var scope = host.Services.CreateScope())
{

    #region Test SiteService
    //var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

    //var graphClient = new SiteService(clientId, clientSecret, tenantId, "", openAIApiKey, pnpContextFactory, tenantBaseUrl);

    //var sites = await graphClient.GetSites();

    //Console.WriteLine(sites.Count());

    //var x = await graphClient.CreateSite();

    //sites = await graphClient.GetSites();

    //Console.WriteLine(sites.Count());

    #endregion

    #region Test TeamsService

    var gen = new Generator(clientId, clientSecret, tenantId, openAIApiKey, null, tenantBaseUrl);

    var newTeams = await gen.CreateTeams(3);

    Console.WriteLine(newTeams.Count());

    //var teams= await gen.GetTeams();
    //Console.WriteLine(teams.Count());



    //var TeamsClient = new TeamService(clientId, clientSecret, tenantId, "Group.ReadWrite.All;Directory.ReadWrite.All", openAIApiKey);

    //var teams = await TeamsClient.GetTeams();
    //Console.WriteLine(teams.Count());

    //await TeamsClient.CreateTeams(1);

    //teams = await TeamsClient.GetTeams();

    //Console.WriteLine(teams.Count());

    #endregion


}

