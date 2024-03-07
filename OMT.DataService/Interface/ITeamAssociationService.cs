using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ITeamAssociationService
    {
        ResultDTO AddTeamAssociation(TeamAssociationCreateDTO teamAssociationCreateDTO);
        ResultDTO DeleteTeamAssociation(int associationid);
        ResultDTO GetTeamAssociationList();
        ResultDTO GetTeamAssociationByTeamId(int teamid);
    }
}
