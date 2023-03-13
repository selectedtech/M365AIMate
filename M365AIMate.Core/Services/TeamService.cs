
using Microsoft.Graph.Models;

namespace M365AIMate.Core.Services;

internal partial class TeamService : BaseService
{
    public TeamService(string clientId, string clientSecret, string tenantId, string scopes, string openAIKey) : base(clientId, clientSecret, tenantId, scopes, openAIKey)
    {

    }
    internal async Task<IEnumerable<Team>> GetTeams()
    {
        var graphClient = GetGraphClient();
        var teams = await graphClient.Teams.GetAsync();
        return teams.Value;
    }

    internal async Task<Team> GetTeam(string teamId)
    {
        var graphClient = GetGraphClient();
        var team = await graphClient.Teams[teamId].GetAsync();
        return team;
    }

    internal async Task<List<Team>> CreateTeams(int numberOfTeams)
    {

        List<Team> resultList = new();
        
        var graphClient = GetGraphClient();
        var openAiService = GetOpenAIService();

        for (int i = 0; i < numberOfTeams; i++)
        {
            var users = await graphClient.Users.GetAsync();
           // var randomUserId = users.Value[new Random().Next(0, users.Value.Count)].Id;
            var randomUserId = await GetRandomUserId();

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
            resultList.Add(result);
        }

        return resultList;
    }    

    internal async Task DeleteTeam(string id)
    {
        var graphClient = GetGraphClient();
        await graphClient.Teams[id].DeleteAsync();
    }
    internal async Task DeleteTeams(List<Team> teams)
    {
        foreach (Team team in teams)
        {
            await DeleteTeam(team.Id);
        }
    }
    internal async Task DeleteTeam(Team team)
    {
        await DeleteTeam(team.Id);
    }
}
