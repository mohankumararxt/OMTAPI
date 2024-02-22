using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ITeamsService
    {
        ResultDTO CreateTeams(TeamsCreateDTO createTeamsDTO);
        ResultDTO DeleteTeam(int teamId);
        ResultDTO GetTeamById(int teamId);
        ResultDTO GetTeamsList();
        ResultDTO teamFilterBYKeyword(string teamname);
        ResultDTO UpdateTeams(TeamsResponseDTO teamsResponseDTO);
    }
}
