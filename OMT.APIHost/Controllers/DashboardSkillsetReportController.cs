using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardSkillsetReportController : ControllerBase
    {

        private readonly IDashboardSkillsetReportService _dashboardSkillsetReportService;
        public DashboardSkillsetReportController(IDashboardSkillsetReportService dashboardSkillsetReportService)
        {
            _dashboardSkillsetReportService = dashboardSkillsetReportService;
        }

        //[HttpPost]
        //[Route("SkillsetWiseReports")]

        //public ResultDTO SkillsetWiseReports([FromBody] DashboardSkillsetReportsDTO skillsetWiseReportsDTO)
        //{
        //    return _dashboardSkillsetReportService.DashboardSkillsetWiseReports(skillsetWiseReportsDTO);
        //}


        [HttpGet]
        [Route("DashboardReports")]

        public ResultDTO DashboardReports([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            return _dashboardSkillsetReportService.DashboardReports(fromDate, toDate);
        }
    }
}
