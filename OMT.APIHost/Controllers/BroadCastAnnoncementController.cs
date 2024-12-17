using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BroadCastAnnoncementController : ControllerBase
    {

        private readonly IBroadCastAnnoncementService _broadCastAnnoncementService;
        public BroadCastAnnoncementController(IBroadCastAnnoncementService broadCastAnnoncementService)
        {
            _broadCastAnnoncementService = broadCastAnnoncementService;
        }


        [HttpGet]
        [Route("GetBroadCastAnnoncementMessages")]
        public ResultDTO GetBroadCastAnnoncementMessages()
        {
            return _broadCastAnnoncementService.GetBroadCastAnnoncementMessages();
        }
    }
}
