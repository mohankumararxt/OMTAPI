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

    }
}