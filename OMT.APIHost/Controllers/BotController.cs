using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;


namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(AuthenticationSchemes = "Bearer")]
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;

        public BotController(IBotService botService)
        {
            _botService = botService;
        }

        //[HttpPost("test")]
        //public async Task<IActionResult> Test(string name)
        //{
        //    Console.WriteLine("Controller hit");
        //    var result = await _botService.CallCandidateApiAsync(name);
        //    return Ok(result);
        //}

        [HttpPut]
        [Route("Update_Checkindate_Bot")]

        public ResultDTO Update_Checkindate_Bot([FromBody] UpdateCheckindateBotRequestDTO updateCheckindateBotRequestDTO)
        {
            return _botService.Update_Checkindate_Bot(updateCheckindateBotRequestDTO);
        }

        [HttpPut]
        [Route("Retrieve_SystemPending_Orders")]

        public ResultDTO Retrieve_SystemPending_Orders([FromBody] RetrieveSystemPendingOrdersRequsetDTO retrieveSystemPendingOrdersRequsetDTO)
        {
            return _botService.Retrieve_SystemPending_Orders(retrieveSystemPendingOrdersRequsetDTO);
        }

        [HttpGet]
        [Route("sorlist")]
        public ResultDTO GetSORList()
        {
            return _botService.GetSORList();
        }

        [HttpGet]
        [Route("list/{sorid:int}")]
        public ResultDTO GetSkillSetListBySORId(int sorid)
        {
            return _botService.GetSkillSetListBySORId(sorid);
        }

        [HttpGet]
        [Route("list/{sorname}")]
        public ResultDTO GetSkillSetListBySOR(string sorname)
        {
            return _botService.GetSkillSetListBySOR(sorname);
        }

        [HttpPost]
        [Route("GetUnassignedOrderCounts")]

        public ResultDTO GetUnassignedOrderCounts(GetUnassignedOrderCountsDTO getUnassignedOrderCountsDTO)
        {
            return _botService.GetUnassignedOrderCounts(getUnassignedOrderCountsDTO);
        }
    }
}
