using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SciExceptionController : ControllerBase
    {
        private readonly ISciExceptionService _sciExceptionService;
        public SciExceptionController(ISciExceptionService sciExceptionService)
        {
            _sciExceptionService = sciExceptionService;
        }

        [HttpPost]
        [Route("UploadSciExceptionReport")]
        public ResultDTO UploadSciExceptionReport([FromBody] UploadSciExceptionReportDTO uploadSciExceptionReportDTO)
        {
            return _sciExceptionService.UploadSciExceptionReport(uploadSciExceptionReportDTO);
        }

        [HttpPost]
        [Route("GetSciExceptionReport")]
        public ResultDTO GetSciExceptionReport([FromBody] GetSciExceptionReportDTO getSciExceptionReportDTO)
        {
            return _sciExceptionService.GetSciExceptionReport(getSciExceptionReportDTO);
        }

        [HttpGet]
        [Route("GetTatStatus")]

        public ResultDTO GetTatStatus()
        {
            return _sciExceptionService.GetTatStatus();
        }

        [HttpGet]
        [Route("GetSciPendingStatusSkillsetsList")]

        public ResultDTO GetSciPendingStatusSkillsetsList()
        {
            return _sciExceptionService.GetSciPendingStatusSkillsetsList();
        }

        [HttpPut]
        [Route("UpdateTat")]
        public ResultDTO UpdateTat(UpdateTatDTO updateTatDTO)
        {
            return _sciExceptionService.UpdateTat(updateTatDTO);
        }
    }
}
