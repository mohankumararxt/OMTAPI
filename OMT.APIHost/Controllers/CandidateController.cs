using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly IActivityLogger _logger;
        private readonly ICandidateService _service;

        public CandidateController(IActivityLogger logger, ICandidateService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("log")]
        public IActionResult LogActivity([FromBody] string activity)
        {
            _logger.Log(activity);
            return Ok("Activity logged successfully.");
        }
    }

}
