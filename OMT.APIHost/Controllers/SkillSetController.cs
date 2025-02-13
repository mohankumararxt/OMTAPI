using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SkillSetController : BaseController
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
        [Route("GetSkillSetList/{skillsetid:int?}")]
        public ResultDTO GetSkillSetList(int? skillsetid)
        {
            return _skillsetService.GetSkillSetList(skillsetid);
        }

        /// <summary>
        /// Get list of skill sets by Skillset Id
        /// </summary>
        /// <returns>Returns a list of skill sets</returns>
        [HttpGet]
        [Route("list/{sorid:int}")]
        public ResultDTO GetSkillSetListBySORId(int sorid)
        {
            return _skillsetService.GetSkillSetListBySORId(sorid);
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
            var userid = UserId;
            ResultDTO resultDTO = _skillsetService.CreateSkillSet(skillSetCreateDTO,userid);
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
            var userid= UserId;
            return _skillsetService.DeleteSkillSet(skillsetId,userid);
        }

        /// <summary>
        /// Update a skill set
        /// </summary>
        /// <param name="skillSetUpdateDTO">SkillSetUpdateDTO</param>
        /// <returns>Returns updated skill set</returns>
        [HttpPut]
        [Route("update")]
        public ResultDTO Update([FromBody] SkillSetUpdateDTO skillSetUpdateDTO)
        {
            var userid = UserId;
            return _skillsetService.UpdateSkillSet(skillSetUpdateDTO,userid);
        }

        [HttpGet]
        [Route("GetListofHardStatenames/{skillsetid:int}")]
        public ResultDTO GetStatenameList(int skillsetid)
        {
            return _skillsetService.GetStatenameList(skillsetid);
        }

        [HttpPost]
        [Route("createTimeline")]
        public ResultDTO CreateTimeLine([FromBody] SkillSetTimeLineDTO skillSetTimeLineDTO)
        {
            ResultDTO resultDTO = _skillsetService.CreateTimeLine(skillSetTimeLineDTO);
            return resultDTO;
        }

        [HttpPut]
        [Route("updateTimeline")]
        public ResultDTO UpdateTimeLine([FromBody] SkillSetUpdateTimeLineDTO skillSetUpdateTimeLineDTO)
        {
            return _skillsetService.UpdateTimeLine(skillSetUpdateTimeLineDTO);
        }

        [HttpGet]
        [Route("timelinelist/{skillsetid:int?}")]
        public ResultDTO GetSkillSetTimelineList(int? skillsetid)
        {
            return _skillsetService.GetSkillSetTimelineList(skillsetid);
        }
    }
}