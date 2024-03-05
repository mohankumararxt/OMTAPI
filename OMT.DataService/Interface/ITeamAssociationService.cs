using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
