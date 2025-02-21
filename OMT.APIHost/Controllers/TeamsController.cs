using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamsService _teamsService;
        public TeamsController(ITeamsService teamsService)
        {
            _teamsService = teamsService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createTeamsDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateTeams")]
        public ResultDTO CreateTeams([FromBody] TeamsCreateDTO createTeamsDTO)
        {
            ResultDTO resultDTO = _teamsService.CreateTeams(createTeamsDTO);
            return resultDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTeamsList")]
        public ResultDTO GetTeamsList()
        {
            return _teamsService.GetTeamsList();
            //return teamlist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        [HttpGet("{teamId:int}")]
        [Route("GetTeamById")]
        public ResultDTO GetTeamById(int teamId)
        {
            return _teamsService.GetTeamById(teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamsResponseDTO"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateTeam")]
        public ResultDTO UpdateTeam([FromBody] TeamsResponseDTO teamsResponseDTO)
        {
            return _teamsService.UpdateTeams(teamsResponseDTO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteTeam/{teamId:int}")]
        public ResultDTO DeleteTeam(int teamId)
        {
            return _teamsService.DeleteTeam(teamId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamname"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("teamFilterBYKeyword/{teamname}")]
        public ResultDTO teamFilterBYKeyword(string teamname)
        {
            return _teamsService.teamFilterBYKeyword(teamname);
        }

    }
}
