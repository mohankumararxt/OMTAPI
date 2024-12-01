using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateGOCController : ControllerBase
    {

        private readonly IUpdateGOCService _updateGOCService;

        public UpdateGOCController(IUpdateGOCService updateGOCService)
        {
            _updateGOCService = updateGOCService;
        }

        [HttpPost]
        [Route("UpdateGetOrderCalculation")]

        public ResultDTO UpdateGetOrderCalculation()
        {
            return _updateGOCService.UpdateGetOrderCalculation();
        }
    }
}
