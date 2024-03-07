using OMT.DTO;

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
