using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductivityDashboardController : BaseController
    {
        private readonly IProductivityDashboardService _productivityDashboardService;

        public ProductivityDashboardController(IProductivityDashboardService productivityDashboardService)
        {
            _productivityDashboardService = productivityDashboardService;
        }

        [HttpPost]
        [Route("GetTeamProductivity")]

        public ResultDTO GetTeamProductivity(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            return _productivityDashboardService.GetTeamProductivity(getTeamProd_UtilDTO);
        }

        [HttpPost]
        [Route("GetTeamUtilization")]

        public ResultDTO GetTeamUtilization(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            return _productivityDashboardService.GetTeamUtilization(getTeamProd_UtilDTO);
        }

        [HttpPost]
        [Route("GetTeamProdUtil")]

        public ResultDTO GetTeamProdUtil(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            return _productivityDashboardService.GetTeamProdUtil(getTeamProd_UtilDTO);
        }

        [HttpPost]
        [Route("GetAgentProductivity")]

        public ResultDTO GetAgentProductivity(GetAgentProd_UtilDTO getAgentProdUtilDTO)
        {
            var userid = this.UserId;
            return _productivityDashboardService.GetAgentProductivity(getAgentProdUtilDTO,userid);
        }

        [HttpPost]
        [Route("GetAgentUtilization")]

        public ResultDTO GetAgentUtilization(GetAgentProd_UtilDTO getAgentProdUtilDTO)
        {
            var userid = this.UserId;
            return _productivityDashboardService.GetAgentUtilization(getAgentProdUtilDTO,userid);
        }
    }
}
