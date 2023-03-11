using Microsoft.Graph.Beta.Models;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365AIMate.Core
{
    public partial class TeamService : BaseService
    {
        public TeamService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey) : base(clientId, clientSecret, tenantId, scopes, openAIKey)
        {

        }
        public async Task<IEnumerable<Team>> GetTeams()
        {
            var graphClient = GetGraphClient();
            var teams = await graphClient.Teams.GetAsync();
            return teams.Value;
        }

        public async Task CreateTeams(int numberOfTeams)
        {
            var graphClient = GetGraphClient();
            var openAiService = GetOpenAIService();

            for (int i = 0; i < numberOfTeams; i++)
            {
                var users = await graphClient.Users.GetAsync();
                var randomUserId = users.Value[new Random().Next(0, users.Value.Count)].Id;

                var teamName = await GenerateText("Give me a random Team name");
                var teamdescription = await GenerateText(string.Format("Give me a random Team description for the team name of '{0}'", teamName));

                var requestBody = new Team
                {
                    DisplayName = teamName,
                    Description = teamdescription,
                    Members = new List<ConversationMember>
                        {
                            new ConversationMember
                            {
                                OdataType = "#microsoft.graph.aadUserConversationMember",
                                Roles = new List<string>
                                {
                                    "owner",
                                },
                                AdditionalData = new Dictionary<string, object>
                                {
                                    {
                                        "user@odata.bind" , $"https://graph.microsoft.com/v1.0/users(\'{randomUserId}\')"
                                    },
                                },
                            },
                        },
                    AdditionalData = new Dictionary<string, object>
                        {
                            {
                                "template@odata.bind" , "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"
                            },
                        },
                };
                var result = await graphClient.Teams.PostAsync(requestBody);
            }
        }
    }
}
