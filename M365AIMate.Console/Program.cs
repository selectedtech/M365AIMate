// See https://aka.ms/new-console-template for more information
using M365AIMate.Core;
using Microsoft.Extensions.Configuration;
using PnP.Core.Services.Builder.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using static System.Formats.Asn1.AsnWriter;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
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


var _clientId = configuration["clientID"];
var _clientSecret = configuration["clientSecret"];
var _tenantId = configuration["tenantID"];

using (var scope = host.Services.CreateScope())
{
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

    var graphClient = new ObjectService(_clientId, _clientSecret, _tenantId, "", pnpContextFactory);

    var sites = await graphClient.GetSites();

    Console.WriteLine(sites.Count());

    var x = await graphClient.CreateSite();

    sites = await graphClient.GetSites();

    Console.WriteLine(sites.Count());

}

