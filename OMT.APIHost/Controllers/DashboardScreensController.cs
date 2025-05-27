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

        [HttpGet]
        [Route("GetVolumeProjection")]

        public ResultDTO GetVolumeProjection(VolumeProjectionInputDTO volumeProjectionInputDTO)
        {
            return _dashboardScreensService.GetVolumeProjection(volumeProjectionInputDTO);
        }
    }
}
