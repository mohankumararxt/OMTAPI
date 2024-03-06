using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// RolesController handles API's for roles available in the organization
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;
        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        /// <summary>
        /// Get list of roles
        /// </summary>
        /// <returns>Returns list of roles</returns>
        [HttpGet]
        [Route("list")]
        public ResultDTO GetRoles()
        {
            return _rolesService.GetRoles();
        }

        /// <summary>
        /// Add a new role
        /// </summary>
        /// <param name="rolename">Role Name</param>
        /// <returns>Returns success message after addition of role</returns>
        [HttpPost]
        [Route("new")]
        public ResultDTO CreateRoles([FromBody]string rolename)
        {
            return _rolesService.CreateRoles(rolename);
        }

        /// <summary>
        /// Delete a role 
        /// </summary>
        /// <param name="roleid">Role Id</param>
        /// <returns>Returns success message after deletion of role</returns>
        [HttpDelete]
        [Route("delete/{roleid:int}")]
        public  ResultDTO DeleteRoles(int roleid)
        {
            return _rolesService.DeleteRoles(roleid);
        }

        /// <summary>
        /// Update a role
        /// </summary>
        /// <param name="rolesResponseDTO">RolesResponseDTO</param>
        /// <returns>Returns updated role</returns>
        [HttpPut]
        [Route("update")]
        public ResultDTO UpdateRoles([FromBody]RolesResponseDTO rolesResponseDTO)
        {
            return _rolesService.UpdateRoles(rolesResponseDTO);
        }

    }
}
