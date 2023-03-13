using M365AIMate.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace M365AIMate.Test
{
    [TestClass]
    public class TeamsTests
    {

        string _clientId = "";
        string _clientSecret = "";
        string _tenantId = "";
        string _openAIApiKey = "";
        string _tenantBaseUrl = "";

        Generator _generator;

        public TeamsTests()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

            var secretConfig = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<TeamsTests>()
                .Build();

            _clientId = secretConfig["clientID"];
            _clientSecret = secretConfig["clientSecret"];
            _tenantId = secretConfig["tenantID"];
            _openAIApiKey = secretConfig["ChatGPTApiKey"];
            _tenantBaseUrl = configuration["PnPCore:Sites:SiteToWorkWith:SiteUrl"];


            _generator = new Generator(_clientId, _clientSecret, _tenantId, _openAIApiKey, null, _tenantBaseUrl);
        }


        [TestMethod]
        public async Task TestCreateTeamsAsync()
        {
            int numberOfTeams = 3;
            var teams = await _generator.CreateTeams(numberOfTeams);

            Assert.IsTrue(teams.Count() == numberOfTeams);

            await _generator.DeleteTeams(teams);
        }

        [TestMethod]
        public async Task TestCreateTeamAsync()
        {
            int numberOfTeams = 1;
            var teams = await _generator.CreateTeams(numberOfTeams);

            Assert.IsTrue(teams.Count() == numberOfTeams);

            await _generator.DeleteTeams(teams);
        }

        [TestMethod]
        public async Task TestDeleteTeamAsync()
        {
            int numberOfTeams = 1;
            var teams = await _generator.CreateTeams(numberOfTeams);            

            await _generator.DeleteTeams(teams);

            Assert.IsNull(await _generator.GetTeam(teams[0].Id));
        }
    }
}