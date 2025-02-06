﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.APIHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
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

        [HttpPost]
        [Route("GetSkillSetWiseProductivity")]

        public ResultDTO GetSkillSetWiseProductivity(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO)
        {
            return _productivityDashboardService.GetSkillSetWiseProductivity(getSkillsetWiseProductivity_DTO);
        }

        [HttpPost]
        [Route("GetSorWiseProductivity")]

        public ResultDTO GetSorWiseProductivity(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO)
        {
            return _productivityDashboardService.GetSorWiseProductivity(getSorWiseProductivity_DTO);
        }

        [HttpPost]
        [Route("GetSorWiseUtilization")]

        public ResultDTO GetSorWiseUtilization(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO)
        {
            return _productivityDashboardService.GetSorWiseUtilization(getSorWiseProductivity_DTO);
        }
    }
}
