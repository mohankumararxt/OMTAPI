using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class DashboardScreensController : BaseController
    {

        private readonly IDashboardScreensService _dashboardScreensService;

        public DashboardScreensController(IDashboardScreensService dashboardScreensService)
        {
            _dashboardScreensService = dashboardScreensService;
        }

        [HttpGet]
        [Route("GetTodaysOrders")]

        public ResultDTO GetTodaysOrders()
        {
            return _dashboardScreensService.GetTodaysOrders();
        }

        [HttpPost]
        [Route("GetVolumeProjection")]

        public ResultDTO GetVolumeProjection(VolumeProjectionInputDTO volumeProjectionInputDTO)
        {
            return _dashboardScreensService.GetVolumeProjection(volumeProjectionInputDTO);
        }

        [HttpPost]
        [Route("GetSorCompletionCount")]

        public ResultDTO GetSorCompletionCount(SorCompletionCountInputDTO sorCompletionCountInputDTO)
        {
            return _dashboardScreensService.GetSorCompletionCount(sorCompletionCountInputDTO);
        }

        [HttpPost]
        [Route("GetWeeklyCompletion")]

        public ResultDTO GetWeeklyCompletion(WeeklyCompletionDTO weeklyCompletionDTO)
        {
            return _dashboardScreensService.GetWeeklyCompletion(weeklyCompletionDTO);
        }

        [HttpPost]
        [Route("GetMonthlyVolumeTrend")]

        public ResultDTO GetMonthlyVolumeTrend(MonthlyVolumeTrendDTO monthlyVolumeTrendDTO)
        {
            return _dashboardScreensService.GetMonthlyVolumeTrend(monthlyVolumeTrendDTO);
        }
    }
}
