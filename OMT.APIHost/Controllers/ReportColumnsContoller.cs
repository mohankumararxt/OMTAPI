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
    [Authorize]
    public class ReportColumnsContoller : BaseController
    {
        private readonly IReportColumnsService _reportColumnsService;
        public ReportColumnsContoller(IReportColumnsService reportColumnsService)
        {
            _reportColumnsService = reportColumnsService;
        }

        [HttpGet]
        [Route("skillsetlist/{skillsetid:int?}")]
        public ResultDTO GetReportColumnlist(int? skillsetid)
        {
            return _reportColumnsService.GetReportColumnlist(skillsetid);
        }

        [HttpPost]
        [Route("CreateReportColumns")]
        public ResultDTO CreateReportColumns([FromBody] CreateReportColumnsDTO createReportColumnsDTO)
        {
            ResultDTO resultDTO = _reportColumnsService.CreateReportColumns(createReportColumnsDTO);
            return resultDTO;
        }
    }
}