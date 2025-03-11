using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Threading.Tasks;

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
        [Route("GetBroadCastAnnouncements")]
        public async Task<ResultDTO> GetBroadCastAnnouncements([FromQuery] int pageNumber, int pageSize)
        {
            return await _broadCastAnnouncementService.GetBroadCastAnnouncements(pageNumber, pageSize);
        }

        [HttpPost]
        [Route("CreateNewBroadCastMessages")]
        public async Task<ResultDTO> CreateNewBroadCastMessages([FromBody] BroadCastAnnouncementRequestDTO broadCastAnnouncementRequestDTO)
        {
            return await _broadCastAnnouncementService.CreateNewBroadCastMessages(broadCastAnnouncementRequestDTO);
        }

        [HttpGet]
        [Route("FilterBroadCastWithStartDateAndEndDate")]
        public async Task<ResultDTO> FilterBroadCastWithStartDateAndEndDate()
        {
            return await _broadCastAnnouncementService.FilterBroadCastWithStartDateAndEndDate();
        }

        [HttpPut]
        [Route("UpdateSoftDeleteFlag")]
        public async Task<ResultDTO> UpdateSoftDeleteFlag([FromQuery] int Id)
        {
            return await _broadCastAnnouncementService.UpdateSoftDeleteFlag(Id);
        }
    }
}
