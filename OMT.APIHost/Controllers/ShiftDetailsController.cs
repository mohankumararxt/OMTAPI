using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftDetailsController : BaseController
    {
        private readonly IShiftDetailsService _shiftDetailsService;

        public ShiftDetailsController(IShiftDetailsService shiftDetailsService)
        {
            _shiftDetailsService = shiftDetailsService;
        }

        [HttpPost]
        [Route("CreateShiftDetails")]

        public ResultDTO CreateShiftDetails(ShiftDetailsDTO shiftDetailsDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.CreateShiftDetails(shiftDetailsDTO,userid);
        }

        [HttpGet]
        [Route("GetShiftDetails")]

        public ResultDTO GetShiftDetails()
        {
            return _shiftDetailsService.GetShiftDetails();
        }

        [HttpDelete]
        [Route("DeleteShiftDetails/{shiftcodeid:int}")]

        public ResultDTO DeleteShiftDetails(int ShiftCodeId)
        {
            return _shiftDetailsService.DeleteShiftDetails(ShiftCodeId);
        }

        [HttpPut]
        [Route("UpdateShiftDetails")]

        public ResultDTO UpdateShiftDetails(EditShiftDetailsDTO editShiftDetailsDTO)
        {
            var userid = UserId;
            return _shiftDetailsService.UpdateShiftDetails(editShiftDetailsDTO,userid);
        }

    }
}
