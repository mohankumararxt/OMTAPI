﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class DashboardController : BaseController

    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpPost]
        [Route("LiveStatusReport")]

        public ResultDTO LiveStatusReport(LiveStatusReportDTO liveStatusReportDTO)
        {
            return _dashboardService.LiveStatusReport(liveStatusReportDTO);
        }

        [HttpPost]
        [Route("AgentCompletionCount")]

        public ResultDTO AgentCompletionCount(AgentDashDTO agentDashDTO)
        {
            var userid = UserId;
            return _dashboardService.AgentCompletionCount(agentDashDTO,userid);
        }
    }
}
