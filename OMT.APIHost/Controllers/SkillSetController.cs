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
    public class SkillSetController : ControllerBase
    {
        private readonly ISkillSetService _skillsetService;
        public SkillSetController(ISkillSetService skillsetService)
        {
            _skillsetService = skillsetService;
        }


        [HttpGet]
        [Route("list")]
        public ResultDTO GetSkillSetList()
        {
            return _skillsetService.GetSkillSetList();
            //return skillsetlist;
        }

        [HttpPost]
        [Route("new")]

        public ResultDTO CreateSkillSet([FromBody] SkillSetCreateDTO skillSetCreateDTO)
        {
            ResultDTO resultDTO = _skillsetService.CreateSkillSet(skillSetCreateDTO);
            return resultDTO;

        }


        [HttpDelete]
        [Route("delete/{skillsetId:int}")]

        public ResultDTO DeleteSkillSet(int skillsetId)
        {
            return _skillsetService.DeleteSkillSet(skillsetId);
        }


        [HttpPut]
        [Route("update")]

        public ResultDTO Update([FromBody]SkillSetResponseDTO skillSetResponseDTO)
        {
            return _skillsetService.UpdateSkillSet(skillSetResponseDTO);
        }
    }
}
