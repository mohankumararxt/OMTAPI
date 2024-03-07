using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    /// <summary>
    /// SkillSetController handles api's for skill sets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SkillSetController : ControllerBase
    {
        private readonly ISkillSetService _skillsetService;
        public SkillSetController(ISkillSetService skillsetService)
        {
            _skillsetService = skillsetService;
        }

        /// <summary>
        /// Get list of all skill sets
        /// </summary>
        /// <returns>Returns a list of skill sets</returns>
        [HttpGet]
        [Route("list")]
        public ResultDTO GetSkillSetList()
        {
            return _skillsetService.GetSkillSetList();
        }

        /// <summary>
        /// Add a new skill set
        /// </summary>
        /// <param name="skillSetCreateDTO">SkillSetCreateDTO</param>
        /// <returns>Returns success message after addition of skill set</returns>
        [HttpPost]
        [Route("new")]

        public ResultDTO CreateSkillSet([FromBody] SkillSetCreateDTO skillSetCreateDTO)
        {
            ResultDTO resultDTO = _skillsetService.CreateSkillSet(skillSetCreateDTO);
            return resultDTO;

        }

        /// <summary>
        /// Delete a skill set
        /// </summary>
        /// <param name="skillsetId"> SkillSet Id</param>
        /// <returns>Returns success message after deletion of skill set</returns>
        [HttpDelete]
        [Route("delete/{skillsetId:int}")]

        public ResultDTO DeleteSkillSet(int skillsetId)
        {
            return _skillsetService.DeleteSkillSet(skillsetId);
        }

        /// <summary>
        /// Update a skill set
        /// </summary>
        /// <param name="skillSetResponseDTO">SkillSetResponseDTO</param>
        /// <returns>Returns updated skill set</returns>
        [HttpPut]
        [Route("update")]
        public ResultDTO Update([FromBody]SkillSetResponseDTO skillSetResponseDTO)
        {
            return _skillsetService.UpdateSkillSet(skillSetResponseDTO);
        }
    }
}
