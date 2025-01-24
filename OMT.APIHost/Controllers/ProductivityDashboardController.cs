using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductivityDashboardController : ControllerBase
    {
        private readonly IProductivityDashboardService _productivityDashboardService;

        public ProductivityDashboardController(IProductivityDashboardService productivityDashboardService)
        {
            _productivityDashboardService = productivityDashboardService;
        }

        [HttpPost]
        [Route("GetTeamProductivity")]

        public ResultDTO GetTeamProductivity()
        {
            return _productivityDashboardService.GetTeamProductivity();
        }
    }
}
