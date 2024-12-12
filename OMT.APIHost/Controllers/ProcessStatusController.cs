using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProcessStatusController : ControllerBase
    {
        private readonly IProcessStatusService _processStatusService;

        public ProcessStatusController(IProcessStatusService processStatusService)
        {
            _processStatusService = processStatusService;
        }

        [HttpGet]
        [Route("list/{systemofrecordid:int}")]

        public ResultDTO GetStatusList(int systemofrecordid)
        {
            return _processStatusService.GetStatusList(systemofrecordid);
        }
    }
}
