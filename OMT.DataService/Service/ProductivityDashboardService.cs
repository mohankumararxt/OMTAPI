using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
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

        public ResultDTO GetTeamProductivity(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                // List<GetTeamProd_Util_DatewisedataDTO> datewisedetails = new List<GetTeamProd_Util_DatewisedataDTO>();

                var prod_Util = _oMTDataContext.Prod_Util
                                                         .Join(_oMTDataContext.TeamAssociation,
                                                             pu => pu.AgentUserId,
                                                             ta => ta.UserId,
                                                             (pu, ta) => new { pu, ta })
                                                         .Join(_oMTDataContext.UserProfile,
                                                             pu_ta => pu_ta.pu.AgentUserId,
                                                             up => up.UserId,
                                                             (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                         .Where(data => data.ta.TeamId == getTeamProd_UtilDTO.TeamId
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
                                                             AverageProductivity = (int)Math.Round(
                                                         group
                                                             .Where(g => g.pu.Productivity_Percentage > 0)
                                                             .Select(g => g.pu.Productivity_Percentage)
                                                             .Average())
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

                if (prod_Util.Count > 0)
                {

                    GetTeamProd_AverageDTO getTeamProd_AverageDTO = new GetTeamProd_AverageDTO();

                    getTeamProd_AverageDTO.TeamProductivity = prod_Util;
                    getTeamProd_AverageDTO.TotalAverageProductivity = (int)Math.Round(prod_Util.Average(x => x.OverallProductivity));


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

        public ResultDTO GetTeamUtilization(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {

                var prod_Util = _oMTDataContext.Prod_Util
                                                          .Join(_oMTDataContext.TeamAssociation,
                                                                pu => pu.AgentUserId,
                                                                ta => ta.UserId,
                                                                (pu, ta) => new { pu, ta })
                                                          .Join(_oMTDataContext.UserProfile,
                                                                pu_ta => pu_ta.pu.AgentUserId,
                                                                up => up.UserId,
                                                                (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                          .Where(data => data.ta.TeamId == getTeamProd_UtilDTO.TeamId
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
                                                                      Utilization = g.Sum(x => x.pu.Utilization_Percentage) // Sum of productivity for that day
                                                                  })
                                                                  .OrderBy(d => d.Date)
                                                                  .ToList(),

                                                              // Calculate average ignoring 0 Utilization values
                                                              AverageUtilization = Math.Round(
                                                                                             group
                                                                                                 .Where(g => g.pu.Utilization_Percentage > 0)
                                                                                                 .Select(g => g.pu.Utilization_Percentage)
                                                                                                 .Average())


                                                          })
                                                          .ToList()
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


                if (prod_Util.Count > 0)
                {
                    GetTeamUtil_AverageDTO getTeamUtil_AverageDTO = new GetTeamUtil_AverageDTO();

                    getTeamUtil_AverageDTO.TeamUtilization = prod_Util;
                    getTeamUtil_AverageDTO.TotalAverageUtilization = (int)Math.Round(prod_Util.Average(x => x.OverallUtilization));


                    resultDTO.Data = getTeamUtil_AverageDTO;
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

        public ResultDTO GetTeamProdUtil(GetTeamProd_UtilDTO getTeamProd_UtilDTO)
        {
            throw new NotImplementedException();
        }

        public ResultDTO GetAgentProductivity(GetAgentProd_UtilDTO getAgentProdUtilDTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var agent_prod = (from pu in _oMTDataContext.Prod_Util
                                  join up in _oMTDataContext.UserProfile on pu.AgentUserId equals up.UserId
                                  where up.IsActive && pu.AgentUserId == UserId
                                   && pu.Createddate.Date >= getAgentProdUtilDTO.FromDate.Date && pu.Createddate.Date <= getAgentProdUtilDTO.ToDate.Date
                                  group pu by pu.AgentUserId into agentGroup
                                  select new GetAgentProd_ResponseDTO
                                  {
                                      DatewiseData = agentGroup.OrderBy(p => p.Createddate)
                                                               .Select(p => new GetTeamProd_DatewisedataDTO
                                                               {
                                                                   Date = p.Createddate.ToString("MM/dd/yyyy"),
                                                                   Productivity = p.Productivity_Percentage
                                                               }).ToList(),

                                      OverallProductivity = agentGroup
                                                               .Where(p => p.Productivity_Percentage > 0)  // Exclude zero values
                                                               .Any()
                                                               ? (int)Math.Round((agentGroup.Where(p => p.Productivity_Percentage > 0).Average(p => p.Productivity_Percentage)))
                                                               : 0}).ToList();

                if (agent_prod.Count > 0)
                {
                    resultDTO.Data = agent_prod;
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
    }
}
