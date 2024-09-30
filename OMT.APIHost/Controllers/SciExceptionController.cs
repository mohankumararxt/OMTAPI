using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SciExceptionController : ControllerBase
    {
        private readonly ISciExceptionService _sciExceptionService;
        public SciExceptionController(ISciExceptionService sciExceptionService)
        {
            _sciExceptionService = sciExceptionService;
        }

        [HttpPost]
        [Route("UploadSciExceptionReport")]
        public ResultDTO UploadSciExceptionReport(UploadSciExceptionReportDTO uploadSciExceptionReportDTO)
        {
            return _sciExceptionService.UploadSciExceptionReport(uploadSciExceptionReportDTO);
        }

        [HttpGet]
        [Route("GetSciExceptionReport")]
        public ResultDTO GetSciExceptionReport(GetSciExceptionReportDTO getSciExceptionReportDTO)
        {
            return _sciExceptionService.GetSciExceptionReport(getSciExceptionReportDTO);
        }
    }
}
