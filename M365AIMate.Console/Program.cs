// See https://aka.ms/new-console-template for more information
using AIPageMate.Core;
using Microsoft.Extensions.Configuration;



var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


Console.WriteLine("Hello, World!");


var _clientId = configuration["clientID"];
var _clientSecret = configuration["clientSecret"];
var _tenantId = configuration["tenantID"];

var graphClient = new GraphClientService(_clientId, _clientSecret, _tenantId, "");

var sites = await graphClient.GetSites();

Console.WriteLine(sites.Count());

