using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IRolesService
    {
        ResultDTO CreateRoles(string rolename);
        ResultDTO DeleteRoles(int roleid);
        ResultDTO GetRoles();
        ResultDTO UpdateRoles(RolesResponseDTO rolesResponseDTO);
    }
}
