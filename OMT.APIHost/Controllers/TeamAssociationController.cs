using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
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
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public ResultDTO GetTeamAssociationList()
        {
            return _teamAssociationService.GetTeamAssociationList();
        }

        /// <summary>
        /// Add new team association
        /// </summary>
        /// <param name="teamAssociationCreateDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("new")]
        public ResultDTO AddTeamAssociation([FromBody]TeamAssociationCreateDTO teamAssociationCreateDTO)
        {
            return _teamAssociationService.AddTeamAssociation(teamAssociationCreateDTO);
        }

        /// <summary>
        /// Delete a team association
        /// </summary>
        /// <param name="associationid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete/{associationid:int}")]
        public ResultDTO DeleteTeamAssociation(int associationid)
        {
            return _teamAssociationService.DeleteTeamAssociation(associationid);
        }

        /// <summary>
        /// Get list of all team associations by team id
        /// </summary>
        /// <param name="teamid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("list/{teamid:int}")]
        public ResultDTO GetTeamAssociationListByTeamId(int teamid)
        {
            return _teamAssociationService.GetTeamAssociationByTeamId(teamid);
        }
    }
}
