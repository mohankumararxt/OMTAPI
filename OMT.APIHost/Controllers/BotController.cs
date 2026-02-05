using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;


namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;

        public BotController(IBotService botService)
        {
            _botService = botService;
        }

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

    }
}
