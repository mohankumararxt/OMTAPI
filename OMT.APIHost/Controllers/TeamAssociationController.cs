using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// TeamAssociationController handles api's for associating users with teams
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamAssociationController : ControllerBase
    {
        private readonly ITeamAssociationService _teamAssociationService;

        public TeamAssociationController(ITeamAssociationService teamAssociationService)
        {
            _teamAssociationService = teamAssociationService;
        }

        /// <summary>
        /// Get list of all team associations
        /// </summary>
        /// <returns>The function returns list of teams and the users associated with it</returns>
        [HttpGet]
        [Route("list")]
        public ResultDTO GetTeamAssociationList()
        {
            return _teamAssociationService.GetTeamAssociationList();
        }

        /// <summary>
        /// Add new team association
        /// </summary>
        /// <param name="teamAssociationCreateDTO">TeamAssociationCreateDTO</param>
        /// <returns>Returns success message after addition of team association</returns>
        [HttpPost]
        [Route("new")]
        public ResultDTO AddTeamAssociation([FromBody] TeamAssociationCreateDTO teamAssociationCreateDTO)
        {
            return _teamAssociationService.AddTeamAssociation(teamAssociationCreateDTO);
        }

        /// <summary>
        /// Delete a team association
        /// </summary>
        /// <param name="associationid">Association Id</param>
        /// <returns>Returns success message after deletion of team association</returns>
        [HttpDelete]
        [Route("delete/{associationid:int}")]
        public ResultDTO DeleteTeamAssociation(int associationid)
        {
            return _teamAssociationService.DeleteTeamAssociation(associationid);
        }

        /// <summary>
        /// Get list of all team associations by team id
        /// </summary>
        /// <param name="teamid">Team Id</param>
        /// <returns>Returns list of users under the selected team</returns>
        [HttpGet]
        [Route("list/{teamid:int}")]
        public ResultDTO GetTeamAssociationListByTeamId(int teamid)
        {
            return _teamAssociationService.GetTeamAssociationByTeamId(teamid);
        }

        [HttpPut]
        [Route("Update")]
        public ResultDTO UpdateTeamAssociation([FromBody] UpdateTeamAssociationDTO teamAssociationDTO)
        {
            return _teamAssociationService.UpdateTeamAssociation(teamAssociationDTO);
        }
    }
}