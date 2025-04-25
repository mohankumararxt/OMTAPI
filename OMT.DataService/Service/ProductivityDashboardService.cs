using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OMT.DataService.Service
{
    public class ProductivityDashboardService : IProductivityDashboardService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ProductivityDashboardService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetTeamProductivity(GetTeamProd_UtilDTO getTeamProd_UtilDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetTeamProd_ResponseDTO> prod_Util = new List<GetTeamProd_ResponseDTO>();

                //var prod_Util = _oMTDataContext.Prod_Util
                //                                         .Join(_oMTDataContext.TeamAssociation,
                //                                             pu => pu.AgentUserId,
                //                                             ta => ta.UserId,
                //                                             (pu, ta) => new { pu, ta })
                //                                         .Join(_oMTDataContext.UserProfile,
                //                                             pu_ta => pu_ta.pu.AgentUserId,
                //                                             up => up.UserId,
                //                                             (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                //                                         .Where(data => data.ta.TeamId == getTeamProd_UtilDTO.TeamId
                //                                                     && data.up.IsActive == true
                //                                                     && data.pu.Createddate.Date >= getTeamProd_UtilDTO.FromDate.Date
                //                                                     && data.pu.Createddate.Date <= getTeamProd_UtilDTO.ToDate.Date)
                //                                         .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                //                                         .Select(group => new
                //                                         {
                //                                             AgentId = group.Key.AgentUserId,
                //                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                //                                             DatewiseData = group
                //                                                 .GroupBy(g => g.pu.Createddate.Date)
                //                                                 .Select(g => new
                //                                                 {
                //                                                     Date = g.Key,
                //                                                     Productivity = g.Sum(x => x.pu.Productivity_Percentage)
                //                                                 })
                //                                                 .OrderBy(d => d.Date)
                //                                                 .ToList(),

                //                                             // Calculate average ignoring 0 productivity values
                //                                             AverageProductivity = group.Any(g => g.pu.Productivity_Percentage > 0)
                //                                                                    ? (int)Math.Round(group.Where(g => g.pu.Productivity_Percentage > 0)
                //                                                                                           .Average(g => g.pu.Productivity_Percentage))
                //                                                                    : 0
                //                                         })
                //                                         .ToList()
                //                                         .Select(x => new GetTeamProd_ResponseDTO
                //                                         {
                //                                             AgentName = x.AgentName,
                //                                             DatewiseData = x.DatewiseData
                //                                                 .Select(d => new GetTeamProd_DatewisedataDTO
                //                                                 {
                //                                                     Date = d.Date.ToString("MM-dd-yyyy"),
                //                                                     Productivity = d.Productivity
                //                                                 })
                //                                                 .ToList(),
                //                                             OverallProductivity = x.AverageProductivity
                //                                         })
                //                                         .ToList();

                var teamid = 0;

                if (getTeamProd_UtilDTO.TeamId == null)
                {
                    teamid = _oMTDataContext.Teams.Where(x => x.TL_Userid == userid && x.IsActive).Select(x => x.TeamId).FirstOrDefault();

                }
                else
                {
                    teamid = (int)getTeamProd_UtilDTO.TeamId;
                }


                if (teamid != 0)
                {
                    prod_Util = _oMTDataContext.Prod_Util
                                                         .Join(_oMTDataContext.Teams,
                                                             pu => pu.TL_Userid,  // Match TL_UserId from Prod_Util
                                                             t => t.TL_Userid,    // Match TL_UserId from Teams
                                                             (pu, t) => new { pu, t })
                                                         .Join(_oMTDataContext.UserProfile,
                                                             pu_t => pu_t.pu.AgentUserId,
                                                             up => up.UserId,
                                                             (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                         .Where(data => data.t.TeamId == teamid // Use TeamId from Teams table
                                                                     && data.up.IsActive == true
                                                                     && data.pu.Createddate.Date >= getTeamProd_UtilDTO.FromDate.Date
                                                                     && data.pu.Createddate.Date <= getTeamProd_UtilDTO.ToDate.Date)
                                                         .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                         .Select(group => new
                                                         {
                                                             AgentId = group.Key.AgentUserId,
                                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                             DatewiseData = group
                                                                 .GroupBy(g => g.pu.Createddate.Date)
                                                                 .Select(g => new
                                                                 {
                                                                     Date = g.Key,
                                                                     Productivity = g.Sum(x => x.pu.Productivity_Percentage)
                                                                 })
                                                                 .OrderBy(d => d.Date)
                                                                 .ToList(),

                                                             // Calculate average ignoring 0 productivity values
                                                             AverageProductivity = group.Any(g => g.pu.Productivity_Percentage > 0)
                                                                                 ? (int)Math.Round(group.Where(g => g.pu.Productivity_Percentage > 0)
                                                                                                        .Average(g => g.pu.Productivity_Percentage),MidpointRounding.AwayFromZero)
                                                                                 : 0
                                                         })
                                                         .ToList()
                                                         .Select(x => new GetTeamProd_ResponseDTO
                                                         {
                                                             AgentName = x.AgentName,
                                                             DatewiseData = x.DatewiseData
                                                                 .Select(d => new GetTeamProd_DatewisedataDTO
                                                                 {
                                                                     Date = d.Date.ToString("MM-dd-yyyy"),
                                                                     Productivity = d.Productivity
                                                                 })
                                                                 .ToList(),
                                                             OverallProductivity = x.AverageProductivity
                                                         })
                                                         .ToList();

                }

                if (prod_Util.Count > 0)
                {

                    GetTeamProd_AverageDTO getTeamProd_AverageDTO = new GetTeamProd_AverageDTO();

                    getTeamProd_AverageDTO.TeamProductivity = prod_Util;
                    getTeamProd_AverageDTO.TotalOverallProductivity = prod_Util
                                                                               .Where(x => x.OverallProductivity > 0)   // Exclude zero values
                                                                               .Any()                                   // Check if any non-zero values exist
                                                                               ? (int)Math.Round(prod_Util.Where(x => x.OverallProductivity > 0)
                                                                                                          .Average(x => x.OverallProductivity), MidpointRounding.AwayFromZero)  // Calculate average
                                                                               : 0;


                    resultDTO.Data = getTeamProd_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "Team productivity details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team productivity details not found";
                    resultDTO.StatusCode = "404";
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetTeamUtilization(GetTeamProd_UtilDTO getTeamProd_UtilDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetTeamUtil_ResponseDTO> prod_Util = new List<GetTeamUtil_ResponseDTO>();

                //var prod_Util = _oMTDataContext.Prod_Util
                //                                          .Join(_oMTDataContext.TeamAssociation,
                //                                                pu => pu.AgentUserId,
                //                                                ta => ta.UserId,
                //                                                (pu, ta) => new { pu, ta })
                //                                          .Join(_oMTDataContext.UserProfile,
                //                                                pu_ta => pu_ta.pu.AgentUserId,
                //                                                up => up.UserId,
                //                                                (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                //                                          .Where(data => data.ta.TeamId == getTeamProd_UtilDTO.TeamId
                //                                                      && data.up.IsActive == true
                //                                                      && data.pu.Createddate.Date >= getTeamProd_UtilDTO.FromDate.Date
                //                                                      && data.pu.Createddate.Date <= getTeamProd_UtilDTO.ToDate.Date)
                //                                          .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                //                                          .Select(group => new
                //                                          {
                //                                              AgentId = group.Key.AgentUserId,
                //                                              AgentName = group.Key.FirstName + " " + group.Key.LastName,
                //                                              DatewiseData = group
                //                                                  .GroupBy(g => g.pu.Createddate.Date) // Group by Date
                //                                                  .Select(g => new
                //                                                  {
                //                                                      Date = g.Key,
                //                                                      Utilization = g.Sum(x => x.pu.Utilization_Percentage)
                //                                                  })
                //                                                  .OrderBy(d => d.Date)
                //                                                  .ToList(),

                //                                              // Calculate average ignoring 0 Utilization values

                //                                              AverageUtilization = group.Any(g => g.pu.Utilization_Percentage > 0)
                //                                                                    ? Math.Round(group.Where(g => g.pu.Utilization_Percentage > 0)
                //                                                                                      .Average(g => g.pu.Utilization_Percentage))
                //                                                                    : 0
                //                                          }).ToList()
                //                                          .Select(x => new GetTeamUtil_ResponseDTO
                //                                          {
                //                                              AgentName = x.AgentName,
                //                                              DatewiseData = x.DatewiseData
                //                                                  .Select(d => new GetTeamUtil_DatewisedataDTO
                //                                                  {
                //                                                      Date = d.Date.ToString("MM-dd-yyyy"),
                //                                                      Utilization = d.Utilization
                //                                                  })
                //                                                  .ToList(),
                //                                              OverallUtilization = (int)x.AverageUtilization
                //                                          })
                //                                          .ToList();

                var teamid = 0;

                if (getTeamProd_UtilDTO.TeamId == null)
                {
                    teamid = _oMTDataContext.Teams.Where(x => x.TL_Userid == userid && x.IsActive).Select(x => x.TeamId).FirstOrDefault();

                }
                else
                {
                    teamid = (int)getTeamProd_UtilDTO.TeamId;
                }


                if (teamid != 0)
                {

                    prod_Util = _oMTDataContext.Prod_Util
                                                             .Join(_oMTDataContext.Teams,
                                                                 pu => pu.TL_Userid,  // Match TL_UserId from Prod_Util
                                                                 t => t.TL_Userid,    // Match TL_UserId from Teams
                                                                 (pu, t) => new { pu, t })
                                                             .Join(_oMTDataContext.UserProfile,
                                                                 pu_t => pu_t.pu.AgentUserId,
                                                                 up => up.UserId,
                                                                 (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                             .Where(data => data.t.TeamId == teamid // Use TeamId from Teams table
                                                                         && data.up.IsActive == true
                                                                         && data.pu.Createddate.Date >= getTeamProd_UtilDTO.FromDate.Date
                                                                         && data.pu.Createddate.Date <= getTeamProd_UtilDTO.ToDate.Date)
                                                             .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                              .Select(group => new
                                                              {
                                                                  AgentId = group.Key.AgentUserId,
                                                                  AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                  DatewiseData = group
                                                                      .GroupBy(g => g.pu.Createddate.Date) // Group by Date
                                                                      .Select(g => new
                                                                      {
                                                                          Date = g.Key,
                                                                          Utilization = g.Sum(x => x.pu.Utilization_Percentage)
                                                                      })
                                                                      .OrderBy(d => d.Date)
                                                                      .ToList(),

                                                                  // Calculate average ignoring 0 Utilization values

                                                                  AverageUtilization = group.Any(g => g.pu.Utilization_Percentage > 0)
                                                                                                                                  ? Math.Round(group.Where(g => g.pu.Utilization_Percentage > 0)
                                                                                                                                                    .Average(g => g.pu.Utilization_Percentage), MidpointRounding.AwayFromZero)
                                                                                                                                  : 0
                                                              }).ToList()
                                                                .Select(x => new GetTeamUtil_ResponseDTO
                                                                {
                                                                    AgentName = x.AgentName,
                                                                    DatewiseData = x.DatewiseData
                                                                        .Select(d => new GetTeamUtil_DatewisedataDTO
                                                                        {
                                                                            Date = d.Date.ToString("MM-dd-yyyy"),
                                                                            Utilization = d.Utilization
                                                                        })
                                                                        .ToList(),
                                                                    OverallUtilization = (int)x.AverageUtilization
                                                                })
                                                                .ToList();
                }


                if (prod_Util.Count > 0)
                {
                    GetTeamUtil_AverageDTO getTeamUtil_AverageDTO = new GetTeamUtil_AverageDTO();

                    getTeamUtil_AverageDTO.TeamUtilization = prod_Util;

                    getTeamUtil_AverageDTO.TotalOverallUtilization = prod_Util
                                                                                .Where(x => x.OverallUtilization > 0)
                                                                                .Any()
                                                                                ? (int)Math.Round(prod_Util.Where(x => x.OverallUtilization > 0)
                                                                                                            .Average(x => x.OverallUtilization), MidpointRounding.AwayFromZero)
                                                                                : 0;


                    resultDTO.Data = getTeamUtil_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "Team Utilization details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Team Utilization details not found";
                    resultDTO.StatusCode = "404";
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetTeamProdUtil(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            throw new NotImplementedException();
        }

        public ResultDTO GetAgentProductivity(GetAgentProd_UtilDTO getAgentProdUtilDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var agent_prod = (from pp in _oMTDataContext.Productivity_Percentage
                                  join up in _oMTDataContext.UserProfile on pp.AgentUserId equals up.UserId
                                  join ss in _oMTDataContext.SkillSet on pp.SkillSetId equals ss.SkillSetId
                                  where up.IsActive && pp.AgentUserId == getAgentProdUtilDTO.UserId
                                        && pp.Createddate.Date >= getAgentProdUtilDTO.FromDate.Date
                                        && pp.Createddate.Date <= getAgentProdUtilDTO.ToDate.Date
                                  group pp by new { pp.AgentUserId, pp.SkillSetId, ss.SkillSetName } into skillGroup
                                  select new GetAgentProd_ResponseDTO
                                  {
                                      SkillSet = skillGroup.Key.SkillSetName,
                                      DatewiseData = skillGroup.OrderBy(p => p.Createddate)
                                                               .Select(p => new GetTeamProd_DatewisedataDTO
                                                               {
                                                                   Date = p.Createddate.ToString("MM/dd/yyyy"),
                                                                   Productivity = p.ProductivityPercentage
                                                               }).ToList(),

                                      OverallProductivity = skillGroup.Any(p => p.ProductivityPercentage > 0)
                                                                      ? (int)Math.Round(skillGroup.Where(p => p.ProductivityPercentage > 0)
                                                                                                  .Average(p => p.ProductivityPercentage), MidpointRounding.AwayFromZero)
                                                                      : 0
                                  }).ToList();


                if (agent_prod.Count > 0)
                {
                    GetAgentProd_AverageDTO getAgentProd_AverageDTO = new GetAgentProd_AverageDTO();

                    getAgentProd_AverageDTO.AgentProductivity = agent_prod;
                    getAgentProd_AverageDTO.TotalOverallProductivity = agent_prod
                                                                               .Where(x => x.OverallProductivity > 0)
                                                                               .Any()
                                                                               ? (int)Math.Round(agent_prod.Where(x => x.OverallProductivity > 0)
                                                                                                          .Average(x => x.OverallProductivity), MidpointRounding.AwayFromZero)
                                                                               : 0;


                    resultDTO.Data = getAgentProd_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "Agent productivity details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Agent productivity details not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetAgentUtilization(GetAgentProd_UtilDTO getAgentProdUtilDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var agent_util = (from pp in _oMTDataContext.Productivity_Percentage
                                  join up in _oMTDataContext.UserProfile on pp.AgentUserId equals up.UserId
                                  join ss in _oMTDataContext.SkillSet on pp.SkillSetId equals ss.SkillSetId
                                  where up.IsActive && pp.AgentUserId == getAgentProdUtilDTO.UserId
                                   && pp.Createddate.Date >= getAgentProdUtilDTO.FromDate.Date && pp.Createddate.Date <= getAgentProdUtilDTO.ToDate.Date
                                  group pp by new { pp.AgentUserId, pp.SkillSetId, ss.SkillSetName } into skillGroup
                                  select new GetAgentUtil_ResponseDTO
                                  {
                                      SkillSet = skillGroup.Key.SkillSetName,
                                      DatewiseData = skillGroup.OrderBy(p => p.Createddate)
                                                              .Select(p => new GetTeamUtil_DatewisedataDTO
                                                              {
                                                                  Date = p.Createddate.ToString("MM/dd/yyyy"),
                                                                  Utilization = p.Utilization
                                                              }).ToList(),

                                      OverallUtilization = skillGroup.Any(p => p.Utilization > 0)
                                                                     ? (int)Math.Round(skillGroup.Where(p => p.Utilization > 0)
                                                                                                 .Average(p => p.Utilization), MidpointRounding.AwayFromZero)
                                                                     : 0
                                  }).ToList();

                if (agent_util.Count > 0)
                {

                    GetAgentUtil_AverageDTO getAgentUtil_AverageDTO = new GetAgentUtil_AverageDTO();

                    getAgentUtil_AverageDTO.AgentUtilization = agent_util;
                    getAgentUtil_AverageDTO.TotalOverallUtilization = agent_util
                                                                               .Where(x => x.OverallUtilization > 0)
                                                                               .Any()
                                                                               ? (int)Math.Round(agent_util.Where(x => x.OverallUtilization > 0)
                                                                                                          .Average(x => x.OverallUtilization), MidpointRounding.AwayFromZero)
                                                                               : 0;


                    resultDTO.Data = getAgentUtil_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "Agent Utilization details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Agent Utilization details not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSkillSetWiseProductivity(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetSkillsetWiseProd_ResponseDTO> ss_prod = new List<GetSkillsetWiseProd_ResponseDTO>();
                GetSkillsetProd_AverageDTO getProd_AverageDTO = new GetSkillsetProd_AverageDTO();

                var skillsetIds = getSkillsetWiseProductivity_DTO.SkillSetId;
                int? teamid = null;
                int roleid = (int)_oMTDataContext.UserProfile
                                                .Where(x => x.UserId == UserId && x.IsActive)
                                                .Select(x => x.RoleId)
                                                .FirstOrDefault();

                if (getSkillsetWiseProductivity_DTO.TeamId == null)
                {
                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                                                .Where(x => x.TL_Userid == UserId && x.IsActive)
                                                .Select(x => (int?)x.TeamId)
                                                .FirstOrDefault() ?? 0;
                    }
                    else if (roleid == 2 || roleid == 4)
                    {
                        teamid = null;
                    }
                }
                else
                {
                    teamid = (int)getSkillsetWiseProductivity_DTO.TeamId;
                }

                if (getSkillsetWiseProductivity_DTO.IsSplit == 1)
                {
                    if (teamid != null)
                    {
                        ss_prod = _oMTDataContext.Productivity_Percentage
                                                                         .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                         .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                         .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.t, data.up, ss.SkillSetName })
                                                                         .Where(data => data.t.TeamId == teamid && data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                        data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                        data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                         .AsEnumerable()
                                                                         .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                         .Select(group => new GetSkillsetWiseProd_ResponseDTO
                                                                         {
                                                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                             SkillSet = group.Key.SkillSetName,
                                                                             DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                 {
                                                                                     Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                     Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList(),
                                                                             OverallProductivity = (int)Math.Round(
                                                                                 group.GroupBy(g => g.pu.Createddate.Date)
                                                                                     .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                     .Where(sum => sum > 0)
                                                                                     .DefaultIfEmpty(0)
                                                                                     .Average(), MidpointRounding.AwayFromZero)
                                                                         })
                                                                         .ToList();
                    }
                    else
                    {
                        ss_prod = _oMTDataContext.Productivity_Percentage
                                                                         .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                         .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                         .Where(data => data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                        data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                        data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                         .AsEnumerable()
                                                                         .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                         .Select(group => new GetSkillsetWiseProd_ResponseDTO
                                                                         {
                                                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                             SkillSet = group.Key.SkillSetName,
                                                                             DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                 {
                                                                                     Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                     Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList(),
                                                                             OverallProductivity = (int)Math.Round(
                                                                                 group.GroupBy(g => g.pu.Createddate.Date)
                                                                                     .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                     .Where(sum => sum > 0)
                                                                                     .DefaultIfEmpty(0)
                                                                                     .Average(), MidpointRounding.AwayFromZero)
                                                                         })
                                                                         .ToList();
                    }

                    // Calculate total productivity for IsSplit = 1
                    var skillsetOverallProductivity = ss_prod
                                                             .GroupBy(x => x.SkillSet)
                                                             .Select(group => new
                                                             {
                                                                 SkillSet = group.Key,
                                                                 OverallProductivity = Math.Round(
                                                                     group
                                                                         .SelectMany(x => x.DatewiseData)
                                                                         .GroupBy(d => d.Date) //  Group by Date
                                                                         .Select(d => d.Where(x => x.Productivity > 0) //  Ignore 0 values per date
                                                                                       .Select(x => x.Productivity)
                                                                                       .DefaultIfEmpty() // Avoid empty sequences
                                                                                       .Average()) //  Get average Productivity per Date
                                                                         .Where(avg => avg > 0) //  Ignore 0 values in final calculation
                                                                         .DefaultIfEmpty() // Avoid empty sequences
                                                                         .Average(), //  Final Average across all Dates
                                                                     MidpointRounding.AwayFromZero)
                                                             })
                                                             .ToList();


                    getProd_AverageDTO.TotalOverallProductivity = skillsetOverallProductivity.Where(x => x.OverallProductivity > 0).Any()
                                                                  ? (int)Math.Round(skillsetOverallProductivity.Where(x => x.OverallProductivity > 0).Average(x => x.OverallProductivity), MidpointRounding.AwayFromZero)
                                                                  : 0;

                }
                else if (getSkillsetWiseProductivity_DTO.IsSplit == 0)
                {
                    if (teamid == null)
                    {
                        ss_prod = _oMTDataContext.Productivity_Percentage
                                                                          .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                          .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                          .Where(data => data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                         data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                         data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                          .AsEnumerable()
                                                                          .GroupBy(data => data.SkillSetName)
                                                                          .Select(group => new GetSkillsetWiseProd_ResponseDTO
                                                                          {
                                                                              SkillSet = group.Key,
                                                                              DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                  .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                  {
                                                                                      Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                      Productivity = (int)Math.Round(
                                                                                          g.Select(x => x.pu.ProductivityPercentage)
                                                                                              .Where(p => p > 0)
                                                                                              .DefaultIfEmpty(0)
                                                                                              .Average(),
                                                                                          MidpointRounding.AwayFromZero)
                                                                                  })
                                                                                  .OrderBy(d => d.Date)
                                                                                  .ToList(),
                                                                              OverallProductivity = (int)Math.Round(
                                                                                  group.GroupBy(g => g.pu.Createddate.Date)
                                                                                      .Select(g => g.Select(x => x.pu.ProductivityPercentage)
                                                                                          .Where(p => p > 0)
                                                                                          .DefaultIfEmpty(0)
                                                                                          .Average())
                                                                                      .Where(avg => avg > 0)
                                                                                      .DefaultIfEmpty(0)
                                                                                      .Average(),
                                                                                  MidpointRounding.AwayFromZero)
                                                                          })
                                                                          .ToList();
                    }
                    else
                    {
                        ss_prod = _oMTDataContext.Productivity_Percentage
                                                                          .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                          .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                          .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.t, data.up, ss.SkillSetName })
                                                                          .Where(data => data.t.TeamId == teamid && data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                         data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                         data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                          .AsEnumerable()
                                                                          .GroupBy(data => data.SkillSetName) // Group by SkillSet only
                                                                          .Select(group => new GetSkillsetWiseProd_ResponseDTO
                                                                          {
                                                                              SkillSet = group.Key,
                                                                              DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                  .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                  {
                                                                                      Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                      Productivity = (int)Math.Round(
                                                                                          g.Select(x => x.pu.ProductivityPercentage)
                                                                                              .Where(p => p > 0)
                                                                                              .DefaultIfEmpty(0)
                                                                                              .Average(),
                                                                                          MidpointRounding.AwayFromZero)
                                                                                  })
                                                                                  .OrderBy(d => d.Date)
                                                                                  .ToList(),
                                                                              OverallProductivity = (int)Math.Round(
                                                                                  group.GroupBy(g => g.pu.Createddate.Date)
                                                                                      .Select(g => g.Select(x => x.pu.ProductivityPercentage)
                                                                                          .Where(p => p > 0)
                                                                                          .DefaultIfEmpty(0)
                                                                                          .Average())
                                                                                      .Where(avg => avg > 0)
                                                                                      .DefaultIfEmpty(0)
                                                                                      .Average(),
                                                                                  MidpointRounding.AwayFromZero)
                                                                          })
                                                                          .ToList();

                    }

                    var validProductivities = ss_prod
                                                     .Select(x => x.OverallProductivity)
                                                     .Where(p => p > 0)
                                                     .ToList();

                    getProd_AverageDTO.TotalOverallProductivity = validProductivities.Any()
                        ? (int)Math.Round(validProductivities.Average(), MidpointRounding.AwayFromZero)
                        : 0;

                }


                // Final Response
                if (ss_prod.Count > 0)
                {
                    GetSkillsetProd_AverageDTO getProd_AverageDTO1 = new GetSkillsetProd_AverageDTO
                    {
                        Productivity = ss_prod,
                        TotalOverallProductivity = getProd_AverageDTO.TotalOverallProductivity
                    };

                    resultDTO.Data = getProd_AverageDTO1;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SkillSet productivity details fetched successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SkillSet Productivity details not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }

            return resultDTO;

        }

        public ResultDTO GetSkillSetWiseUtilization(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetSkillsetWiseUtil_ResponseDTO> ss_util = new List<GetSkillsetWiseUtil_ResponseDTO>();
                GetSkillsetUtil_AverageDTO getUtil_AverageDTO = new GetSkillsetUtil_AverageDTO();

                var skillsetIds = getSkillsetWiseProductivity_DTO.SkillSetId;
                int? teamid = null;
                int roleid = (int)_oMTDataContext.UserProfile
                                                .Where(x => x.UserId == UserId && x.IsActive)
                                                .Select(x => x.RoleId)
                                                .FirstOrDefault();

                if (getSkillsetWiseProductivity_DTO.TeamId == null)
                {
                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                                                .Where(x => x.TL_Userid == UserId && x.IsActive)
                                                .Select(x => (int?)x.TeamId)
                                                .FirstOrDefault() ?? 0;
                    }
                    else if (roleid == 2 || roleid == 4)
                    {
                        teamid = null;
                    }
                }
                else
                {
                    teamid = (int)getSkillsetWiseProductivity_DTO.TeamId;
                }

                if (getSkillsetWiseProductivity_DTO.IsSplit == 0)
                {
                    if (teamid != null)
                    {
                        ss_util = _oMTDataContext.Productivity_Percentage
                                                                        .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                        .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                        .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.t, data.up, ss.SkillSetName })
                                                                        .Where(data => data.t.TeamId == teamid && data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                       data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                       data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                        .AsEnumerable()
                                                                        .GroupBy(data => data.SkillSetName) // Group by SkillSet only
                                                                        .Select(group => new GetSkillsetWiseUtil_ResponseDTO
                                                                        {
                                                                            SkillSet = group.Key,
                                                                            DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                {
                                                                                    Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                    Utilization = (int)Math.Round(
                                                                                        g.Select(x => x.pu.Utilization)
                                                                                            .Where(p => p > 0)
                                                                                            .DefaultIfEmpty(0)
                                                                                            .Average(),
                                                                                        MidpointRounding.AwayFromZero)
                                                                                })
                                                                                .OrderBy(d => d.Date)
                                                                                .ToList(),
                                                                            OverallUtilization = (int)Math.Round(
                                                                                group.GroupBy(g => g.pu.Createddate.Date)
                                                                                    .Select(g => g.Select(x => x.pu.Utilization)
                                                                                        .Where(p => p > 0)
                                                                                        .DefaultIfEmpty(0)
                                                                                        .Average())
                                                                                    .Where(avg => avg > 0)
                                                                                    .DefaultIfEmpty(0)
                                                                                    .Average(),
                                                                                MidpointRounding.AwayFromZero)
                                                                        })
                                                                        .ToList();
                    }
                    else
                    {
                        ss_util = _oMTDataContext.Productivity_Percentage
                                                                         .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                         .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                         .Where(data => data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                        data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                        data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                         .AsEnumerable()
                                                                         .GroupBy(data => data.SkillSetName)
                                                                         .Select(group => new GetSkillsetWiseUtil_ResponseDTO
                                                                         {
                                                                             SkillSet = group.Key,
                                                                             DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                 {
                                                                                     Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                     Utilization = (int)Math.Round(
                                                                                         g.Select(x => x.pu.Utilization)
                                                                                             .Where(p => p > 0)
                                                                                             .DefaultIfEmpty(0)
                                                                                             .Average(),
                                                                                         MidpointRounding.AwayFromZero)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList(),
                                                                             OverallUtilization = (int)Math.Round(
                                                                                 group.GroupBy(g => g.pu.Createddate.Date)
                                                                                     .Select(g => g.Select(x => x.pu.Utilization)
                                                                                         .Where(p => p > 0)
                                                                                         .DefaultIfEmpty(0)
                                                                                         .Average())
                                                                                     .Where(avg => avg > 0)
                                                                                     .DefaultIfEmpty(0)
                                                                                     .Average(),
                                                                                 MidpointRounding.AwayFromZero)
                                                                         })
                                                                         .ToList();
                    }

                    var validUtilization = ss_util
                                                    .Select(x => x.OverallUtilization)
                                                    .Where(p => p > 0)
                                                    .ToList();

                    getUtil_AverageDTO.TotalOverallUtilization = validUtilization.Any()
                        ? (int)Math.Round(validUtilization.Average(), MidpointRounding.AwayFromZero)
                        : 0;
                }

                else if (getSkillsetWiseProductivity_DTO.IsSplit == 1)
                {
                    if (teamid != null)
                    {
                        ss_util = _oMTDataContext.Productivity_Percentage
                                                                         .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                         .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                         .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.t, data.up, ss.SkillSetName })
                                                                         .Where(data => data.t.TeamId == teamid && data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                        data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                        data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                         .AsEnumerable()
                                                                         .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                         .Select(group => new GetSkillsetWiseUtil_ResponseDTO
                                                                         {
                                                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                             SkillSet = group.Key.SkillSetName,
                                                                             DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                 {
                                                                                     Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                     Utilization = g.Sum(x => x.pu.Utilization)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList(),
                                                                             OverallUtilization = (int)Math.Round(
                                                                                 group.GroupBy(g => g.pu.Createddate.Date)
                                                                                     .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                     .Where(sum => sum > 0)
                                                                                     .DefaultIfEmpty(0)
                                                                                     .Average(), MidpointRounding.AwayFromZero)
                                                                         })
                                                                         .ToList();
                    }
                    else
                    {
                        ss_util = _oMTDataContext.Productivity_Percentage
                                                                        .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                        .Join(_oMTDataContext.SkillSet, data => data.pu.SkillSetId, ss => ss.SkillSetId, (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                        .Where(data => data.up.IsActive && skillsetIds.Contains(data.pu.SkillSetId) &&
                                                                                       data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date &&
                                                                                       data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                        .AsEnumerable()
                                                                        .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                         .Select(group => new GetSkillsetWiseUtil_ResponseDTO
                                                                         {
                                                                             AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                             SkillSet = group.Key.SkillSetName,
                                                                             DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                 {
                                                                                     Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                     Utilization = g.Sum(x => x.pu.Utilization)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList(),
                                                                             OverallUtilization = (int)Math.Round(
                                                                                 group.GroupBy(g => g.pu.Createddate.Date)
                                                                                     .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                     .Where(sum => sum > 0)
                                                                                     .DefaultIfEmpty(0)
                                                                                     .Average(), MidpointRounding.AwayFromZero)
                                                                         })
                                                                         .ToList();
                    }

                    // Calculate total Utilization for IsSplit = 1
                    var skillsetOverallUtilization = ss_util
                                                            .GroupBy(x => x.SkillSet)
                                                            .Select(group => new
                                                            {
                                                                SkillSet = group.Key,
                                                                OverallUtilization = Math.Round(
                                                                    group
                                                                        .SelectMany(x => x.DatewiseData)
                                                                        .GroupBy(d => d.Date)
                                                                        .Select(d => d.Where(x => x.Utilization > 0)
                                                                                      .Select(x => x.Utilization)
                                                                                      .DefaultIfEmpty()
                                                                                      .Average())
                                                                        .Where(avg => avg > 0)
                                                                        .DefaultIfEmpty()
                                                                        .Average(),
                                                                    MidpointRounding.AwayFromZero)
                                                            })
                                                            .ToList();

                    getUtil_AverageDTO.TotalOverallUtilization = skillsetOverallUtilization.Where(x => x.OverallUtilization > 0).Any()
                                                                  ? (int)Math.Round(skillsetOverallUtilization.Where(x => x.OverallUtilization > 0).Average(x => x.OverallUtilization), MidpointRounding.AwayFromZero)
                                                                  : 0;
                }
                if (ss_util.Count > 0)
                {
                    GetSkillsetUtil_AverageDTO getUtil_AverageDTO1 = new GetSkillsetUtil_AverageDTO
                    {
                        Utilization = ss_util,
                        TotalOverallUtilization = getUtil_AverageDTO.TotalOverallUtilization
                    };

                    resultDTO.Data = getUtil_AverageDTO1;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SkillSet productivity details fetched successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SkillSet Productivity details not found";
                    resultDTO.StatusCode = "404";
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSorWiseProductivity(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {

                List<GetSorWiseProductivity_ResponseDTO> sor_prod = new List<GetSorWiseProductivity_ResponseDTO>();
                GetSorProd_AverageDTO getProd_AverageDTO = new GetSorProd_AverageDTO();

                int? teamid = null;
                int roleid = (int)_oMTDataContext.UserProfile
                                                .Where(x => x.UserId == UserId && x.IsActive)
                                                .Select(x => x.RoleId)
                                                .FirstOrDefault();

                if (getSorWiseProductivity_DTO.TeamId == null)
                {
                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                                                .Where(x => x.TL_Userid == UserId && x.IsActive)
                                                .Select(x => (int?)x.TeamId)
                                                .FirstOrDefault() ?? 0;
                    }
                    else if (roleid == 2 || roleid == 4)
                    {
                        teamid = null;
                    }
                }
                else
                {
                    teamid = (int)getSorWiseProductivity_DTO.TeamId;
                }

                if (getSorWiseProductivity_DTO.IsSplit == 1)
                {
                    if (teamid != null)
                    {
                        sor_prod = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                                .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                                 .Where(data => data.t.TeamId == teamid
                                                                                             && data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                                 .Select(group => new GetSorWiseProductivity_ResponseDTO
                                                                                 {
                                                                                     //AgentId = group.Key.AgentUserId,
                                                                                     AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                                     DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                         .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                                         {
                                                                                                             Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                                             Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                                         })
                                                                                                         .OrderBy(d => d.Date)
                                                                                                         .ToList(),
                                                                                     OverallProductivity = (int)Math.Round(
                                                                                                             group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                                 .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                                                 .Where(sum => sum > 0)
                                                                                                                 .DefaultIfEmpty(0)
                                                                                                                 .Average(), MidpointRounding.AwayFromZero)
                                                                                 }).ToList();




                    }
                    else
                    {
                        sor_prod = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                                 .Where(data => data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                                 .Select(group => new GetSorWiseProductivity_ResponseDTO
                                                                                 {
                                                                                     //AgentId = group.Key.AgentUserId,
                                                                                     AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                                     DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                         .Select(g => new GetTeamProd_DatewisedataDTO
                                                                                                         {
                                                                                                             Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                                             Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                                         })
                                                                                                         .OrderBy(d => d.Date)
                                                                                                         .ToList(),
                                                                                     OverallProductivity = (int)Math.Round(
                                                                                                             group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                                 .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                                                 .Where(sum => sum > 0)
                                                                                                                 .DefaultIfEmpty(0)
                                                                                                                 .Average(), MidpointRounding.AwayFromZero)
                                                                                 }).ToList();

                    }

                    // Calculate total productivity for IsSplit = 1
                    var validProductivities = sor_prod
                                                      .Select(x => x.OverallProductivity)
                                                      .Where(p => p > 0)
                                                      .ToList();

                    getProd_AverageDTO.TotalOverallProductivity = validProductivities.Any()
                                                                  ? (int)Math.Round(validProductivities.Average(), MidpointRounding.AwayFromZero)
                                                                  : 0;

                }
                else if (getSorWiseProductivity_DTO.IsSplit == 0)
                {
                    List<GetTeamProd_DatewisedataDTO> datewiseData = new List<GetTeamProd_DatewisedataDTO>();

                    if (teamid == null)
                    {
                        datewiseData = _oMTDataContext.Productivity_Percentage
                                                                                 .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                                 .Where(data => data.up.IsActive
                                                                                     && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                     && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                     && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(g => g.pu.Createddate.Date) 
                                                                                 .Select(dateGroup => new GetTeamProd_DatewisedataDTO
                                                                                 {
                                                                                     Date = dateGroup.Key.ToString("MM-dd-yyyy"),
                                                                                     Productivity = (int)Math.Round(
                                                                                         dateGroup.GroupBy(g => g.pu.AgentUserId)
                                                                                             .Select(userGroup => userGroup.Sum(x => x.pu.ProductivityPercentage)) 
                                                                                             .Where(sum => sum > 0) 
                                                                                             .DefaultIfEmpty(0)
                                                                                             .Average(), 
                                                                                         MidpointRounding.AwayFromZero)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList();



                    }
                    else
                    {
                        datewiseData = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                                .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                                 .Where(data => data.t.TeamId == teamid
                                                                                             && data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(dateGroup => new GetTeamProd_DatewisedataDTO
                                                                                 {
                                                                                     Date = dateGroup.Key.ToString("MM-dd-yyyy"),
                                                                                     Productivity = (int)Math.Round(
                                                                                         dateGroup.GroupBy(g => g.pu.AgentUserId)
                                                                                             .Select(userGroup => userGroup.Sum(x => x.pu.ProductivityPercentage))
                                                                                             .Where(sum => sum > 0)
                                                                                             .DefaultIfEmpty(0)
                                                                                             .Average(),
                                                                                         MidpointRounding.AwayFromZero)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList();
                    }

                    var validProductivities2 = datewiseData
                            .Select(x => x.Productivity)
                            .Where(p => p > 0)
                            .ToList();

                    getProd_AverageDTO.TotalOverallProductivity = validProductivities2.Any()
                        ? (int)Math.Round(validProductivities2.Average(), MidpointRounding.AwayFromZero)
                        : 0;

                    sor_prod.Add(new GetSorWiseProductivity_ResponseDTO
                    {
                        AgentName = null,
                        DatewiseData = datewiseData,
                        OverallProductivity = getProd_AverageDTO.TotalOverallProductivity
                    });

                }

                if (sor_prod.Count > 0)
                {

                    GetSorProd_AverageDTO getProd_AverageDTO1 = new GetSorProd_AverageDTO
                    {
                        Productivity = sor_prod,
                        TotalOverallProductivity = getProd_AverageDTO.TotalOverallProductivity
                    };

                    resultDTO.Data = getProd_AverageDTO1;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SOR productivity details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SOR Productivity details not found";
                    resultDTO.StatusCode = "404";
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSorWiseUtilization(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetSorWiseUtil_ResponseDTO> sor_util = new List<GetSorWiseUtil_ResponseDTO>();
                GetSorUtil_AverageDTO getUtil_AverageDTO = new GetSorUtil_AverageDTO();

                int? teamid = null;
                int roleid = (int)_oMTDataContext.UserProfile
                                                .Where(x => x.UserId == UserId && x.IsActive)
                                                .Select(x => x.RoleId)
                                                .FirstOrDefault();

                if (getSorWiseProductivity_DTO.TeamId == null)
                {
                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                                                .Where(x => x.TL_Userid == UserId && x.IsActive)
                                                .Select(x => (int?)x.TeamId)
                                                .FirstOrDefault() ?? 0;
                    }
                    else if (roleid == 2 || roleid == 4)
                    {
                        teamid = null;
                    }
                }
                else
                {
                    teamid = (int)getSorWiseProductivity_DTO.TeamId;
                }

                if (getSorWiseProductivity_DTO.IsSplit == 1)
                {
                    if (teamid != null)
                    {
                        sor_util = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                                .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                                 .Where(data => data.t.TeamId == teamid
                                                                                             && data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                                 .Select(group => new GetSorWiseUtil_ResponseDTO
                                                                                 {
                                                                                     //AgentId = group.Key.AgentUserId,
                                                                                     AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                                     DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                         .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                                         {
                                                                                                             Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                                             Utilization = g.Sum(x => x.pu.Utilization)
                                                                                                         })
                                                                                                         .OrderBy(d => d.Date)
                                                                                                         .ToList(),
                                                                                     OverallUtilization = (int)Math.Round(
                                                                                                             group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                                 .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                                                 .Where(sum => sum > 0)
                                                                                                                 .DefaultIfEmpty(0)
                                                                                                                 .Average(), MidpointRounding.AwayFromZero)
                                                                                 }).ToList();




                    }
                    else
                    {
                        sor_util = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                                 .Where(data => data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(data => new { data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                                 .Select(group => new GetSorWiseUtil_ResponseDTO
                                                                                 {
                                                                                     //AgentId = group.Key.AgentUserId,
                                                                                     AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                                     DatewiseData = group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                         .Select(g => new GetTeamUtil_DatewisedataDTO
                                                                                                         {
                                                                                                             Date = g.Key.ToString("MM-dd-yyyy"),
                                                                                                             Utilization = g.Sum(x => x.pu.Utilization)
                                                                                                         })
                                                                                                         .OrderBy(d => d.Date)
                                                                                                         .ToList(),
                                                                                     OverallUtilization = (int)Math.Round(
                                                                                                             group.GroupBy(g => g.pu.Createddate.Date)
                                                                                                                 .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                                                 .Where(sum => sum > 0)
                                                                                                                 .DefaultIfEmpty(0)
                                                                                                                 .Average(), MidpointRounding.AwayFromZero)
                                                                                 }).ToList();

                    }

                    // Calculate total Utilization for IsSplit = 1
                    var validUtilizations = sor_util
                                                      .Select(x => x.OverallUtilization)
                                                      .Where(p => p > 0)
                                                      .ToList();

                   getUtil_AverageDTO.TotalOverallUtilization = validUtilizations.Any()
                                                                  ? (int)Math.Round(validUtilizations.Average(), MidpointRounding.AwayFromZero)
                                                                  : 0;

                }
                else if (getSorWiseProductivity_DTO.IsSplit == 0)
                {
                    List<GetTeamUtil_DatewisedataDTO> datewiseData = new List<GetTeamUtil_DatewisedataDTO>();

                    if (teamid == null)
                    {
                        datewiseData = _oMTDataContext.Productivity_Percentage
                                                                                 .Join(_oMTDataContext.UserProfile, pu => pu.AgentUserId, up => up.UserId, (pu, up) => new { pu, up })
                                                                                 .Where(data => data.up.IsActive
                                                                                     && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                     && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                     && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(g => g.pu.Createddate.Date) 
                                                                                 .Select(dateGroup => new GetTeamUtil_DatewisedataDTO
                                                                                 {
                                                                                     Date = dateGroup.Key.ToString("MM-dd-yyyy"),
                                                                                    Utilization = (int)Math.Round(
                                                                                         dateGroup.GroupBy(g => g.pu.AgentUserId) 
                                                                                             .Select(userGroup => userGroup.Sum(x => x.pu.Utilization)) 
                                                                                             .Where(sum => sum > 0) 
                                                                                             .DefaultIfEmpty(0)
                                                                                             .Average(), 
                                                                                         MidpointRounding.AwayFromZero)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList();



                    }
                    else
                    {
                        datewiseData = _oMTDataContext.Productivity_Percentage
                                                                                .Join(_oMTDataContext.Teams, pu => pu.TlUserId, t => t.TL_Userid, (pu, t) => new { pu, t })
                                                                                .Join(_oMTDataContext.UserProfile, pu_t => pu_t.pu.AgentUserId, up => up.UserId, (pu_t, up) => new { pu_t.pu, pu_t.t, up })
                                                                                 .Where(data => data.t.TeamId == teamid
                                                                                             && data.up.IsActive
                                                                                             && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                             && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                             && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                                 .AsEnumerable()
                                                                                 .GroupBy(g => g.pu.Createddate.Date)
                                                                                 .Select(dateGroup => new GetTeamUtil_DatewisedataDTO
                                                                                 {
                                                                                     Date = dateGroup.Key.ToString("MM-dd-yyyy"),
                                                                                     Utilization = (int)Math.Round(
                                                                                         dateGroup.GroupBy(g => g.pu.AgentUserId)
                                                                                             .Select(userGroup => userGroup.Sum(x => x.pu.Utilization))
                                                                                             .Where(sum => sum > 0)
                                                                                             .DefaultIfEmpty(0)
                                                                                             .Average(),
                                                                                         MidpointRounding.AwayFromZero)
                                                                                 })
                                                                                 .OrderBy(d => d.Date)
                                                                                 .ToList();
                    }

                    var validUtilizations2 = datewiseData
                            .Select(x => x.Utilization)
                            .Where(p => p > 0)
                            .ToList();

                    getUtil_AverageDTO.TotalOverallUtilization = validUtilizations2.Any()
                                                                                   ? (int)Math.Round(validUtilizations2.Average(), MidpointRounding.AwayFromZero)
                                                                                   : 0;

                    sor_util.Add(new GetSorWiseUtil_ResponseDTO
                    {
                        AgentName = null,
                        DatewiseData = datewiseData,
                        OverallUtilization = getUtil_AverageDTO.TotalOverallUtilization
                    });

                }

                if (sor_util.Count > 0)
                {

                    GetSorUtil_AverageDTO getUtil_AverageDTO1 = new GetSorUtil_AverageDTO
                    {
                        Utilization= sor_util,
                       TotalOverallUtilization= getUtil_AverageDTO.TotalOverallUtilization
                    };

                    resultDTO.Data = getUtil_AverageDTO1;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SOR Utilization details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SOR Productivity details not found";
                    resultDTO.StatusCode = "404";
                }

               
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }


    }
}
