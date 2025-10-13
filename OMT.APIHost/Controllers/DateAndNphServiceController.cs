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
            return _dateAndNphService.GetCheckinDetails(getCheckinDetailsDTO, userid);
        }

        [HttpPut]
        [Route("UpdateCheckinDetails")]
        public ResultDTO UpdateCheckinDetails([FromBody] UpdateCheckinDetailsDTO updateCheckinDetailsDTO)
        {
            return _dateAndNphService.UpdateCheckinDetails(updateCheckinDetailsDTO);
        }

        [HttpGet]
        [Route("GetNonProductiveReasons")]
        public ResultDTO GetNonProductiveReasons()
        {
            return _dateAndNphService.GetNonProductiveReasons();
        }

        [HttpPost]
        [Route("ApplyNonProductiveHours")]
        public ResultDTO ApplyNonProductiveHours([FromBody] ApplyNonProductiveHoursDTO applyNonProductiveHoursDTO)
        {
            var userid = UserId;
            return _dateAndNphService.ApplyNonProductiveHours(applyNonProductiveHoursDTO, userid);
        }

        [HttpPost]
        [Route("GetNonProductiveRegularizations_Agent")]

        public ResultDTO GetNonProductiveRegularizations_Agent([FromBody] PaginationInputDTO paginationInputDTO)
        {
            var userid = UserId;
            return _dateAndNphService.GetNonProductiveRegularizations_Agent(paginationInputDTO, userid);
        }

        [HttpGet]
        [Route("GetRegularizationStatus")]

        public ResultDTO GetRegularizationStatus()
        {
            return _dateAndNphService.GetRegularizationStatus();
        }

        [HttpPost]
        [Route("GetNonProductiveRegularizations")]

        public ResultDTO GetNonProductiveRegularizations([FromBody] GetCheckinDetailsDTO getCheckinDetailsDTO)
        {
            var userid = UserId;
            return _dateAndNphService.GetNonProductiveRegularizations(getCheckinDetailsDTO, userid);
        }

        [HttpPost]
        [Route("UpdateRegularizations")]

        public ResultDTO UpdateRegularizations([FromBody] UpdateRegularizationsDTO updateRegularizationsDTO)
        {
            var userid = UserId;
            return _dateAndNphService.UpdateRegularizations(updateRegularizationsDTO,userid);
        }
    }
}
