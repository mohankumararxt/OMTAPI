using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class DateAndNphServiceController : BaseController
    {
        private readonly IDateAndNphService _dateAndNphService;

        public DateAndNphServiceController(IDateAndNphService dateAndNphService)
        {
            _dateAndNphService = dateAndNphService;
        }

        [HttpPost]
        [Route("GetCheckinDetails")]
        public ResultDTO GetCheckinDetails([FromBody] GetCheckinDetailsDTO getCheckinDetailsDTO)
        {
            var userid = UserId;
            return _dateAndNphService.GetCheckinDetails(getCheckinDetailsDTO,userid);
        }

        [HttpPut]
        [Route("UpdateCheckinDetails")]
        public ResultDTO UpdateCheckinDetails(UpdateCheckinDetailsDTO updateCheckinDetailsDTO)
        {
            return _dateAndNphService.UpdateCheckinDetails(updateCheckinDetailsDTO);
        }
    }
}
