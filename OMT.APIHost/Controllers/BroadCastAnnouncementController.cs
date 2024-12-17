using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BroadCastAnnouncementController : ControllerBase
    {

        private readonly IBroadCastAnnouncementService _broadCastAnnouncementService;
        public BroadCastAnnouncementController(IBroadCastAnnouncementService broadCastAnnouncementService)
        {
            _broadCastAnnouncementService = broadCastAnnouncementService;
        }


        [HttpGet]
        [Route("GetBroadCastAnnouncementMessages")]
        public ResultDTO GetBroadCastAnnouncementMessages()
        {
            return _broadCastAnnouncementService.GetBroadCastAnnouncementMessages();
        }

        [HttpGet]
        [Route("GetBroadCastAnnouncementBySoftDelete")]
        public ResultDTO GetBroadCastAnnouncementBySoftDelete()
        {
            return _broadCastAnnouncementService.GetBroadCastAnnouncementBySoftDelete();
        }

        [HttpPost]
        [Route("CreateNewBroadCastMessages")]
        public ResultDTO CreateNewBroadCastMessages([FromBody] BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO)
        {
            return _broadCastAnnouncementService.CreateNewBroadCastMessages(broadCastAnnouncementRequestDTO);
        }

        [HttpGet]
        [Route("FilterBroadCastWithStartDateAndEndDate")]
        public ResultDTO FilterBroadCastWithStartDateAndEndDate()
        {
            return _broadCastAnnouncementService.FilterBroadCastWithStartDateAndEndDate();   
        }

        [HttpPut]
        [Route("UpdateSoftDeleteFlag")]
        public ResultDTO UpdateSoftDeleteFlag([FromQuery] int Id)
        {
            return _broadCastAnnouncementService.UpdateSoftDeleteFlag(Id);  
        }

    }
}
