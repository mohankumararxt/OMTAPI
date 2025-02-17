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
                                                         AverageProductivity = group.Any(g => g.pu.Productivity_Percentage > 0)
                                                                                    ? (int)Math.Round(group.Where(g => g.pu.Productivity_Percentage > 0)
                                                                                                           .Average(g => g.pu.Productivity_Percentage))
                                                                                    : 0})
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
                    getTeamProd_AverageDTO.TotalOverallProductivity = prod_Util
                                                                               .Where(x => x.OverallProductivity > 0)   // Exclude zero values
                                                                               .Any()                                   // Check if any non-zero values exist
                                                                               ? (int)Math.Round(prod_Util.Where(x => x.OverallProductivity > 0)
                                                                                                          .Average(x => x.OverallProductivity))  // Calculate average
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
                                                                      Utilization = g.Sum(x => x.pu.Utilization_Percentage)
                                                                  })
                                                                  .OrderBy(d => d.Date)
                                                                  .ToList(),

                                                          // Calculate average ignoring 0 Utilization values
                                                              
                                                          AverageUtilization = group.Any(g => g.pu.Utilization_Percentage > 0)
                                                                                    ? Math.Round(group.Where(g => g.pu.Utilization_Percentage > 0)
                                                                                                      .Average(g => g.pu.Utilization_Percentage))
                                                                                    : 0}).ToList()
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

                    getTeamUtil_AverageDTO.TotalOverallUtilization = prod_Util
                                                                                .Where(x => x.OverallUtilization > 0)   
                                                                                .Any()                                
                                                                                ? (int)Math.Round(prod_Util.Where(x => x.OverallUtilization > 0)
                                                                                                            .Average(x => x.OverallUtilization))  
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

                                      OverallProductivity = agentGroup.Any(p => p.Productivity_Percentage > 0)
                                                                      ? (int)Math.Round(agentGroup.Where(p => p.Productivity_Percentage > 0)
                                                                                                  .Average(p => p.Productivity_Percentage))
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

        public ResultDTO GetAgentUtilization(GetAgentProd_UtilDTO getAgentProdUtilDTO, int UserId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var agent_util = (from pu in _oMTDataContext.Prod_Util
                                  join up in _oMTDataContext.UserProfile on pu.AgentUserId equals up.UserId
                                  where up.IsActive && pu.AgentUserId == UserId
                                   && pu.Createddate.Date >= getAgentProdUtilDTO.FromDate.Date && pu.Createddate.Date <= getAgentProdUtilDTO.ToDate.Date
                                  group pu by pu.AgentUserId into agentGroup
                                  select new GetAgentUtil_ResponseDTO
                                  {
                                      DatewiseData = agentGroup.OrderBy(p => p.Createddate)
                                                               .Select(p => new GetTeamUtil_DatewisedataDTO
                                                               {
                                                                   Date = p.Createddate.ToString("MM/dd/yyyy"),
                                                                   Utilization = p.Utilization_Percentage
                                                               }).ToList(),

                                      OverallUtilization = agentGroup.Any(p => p.Utilization_Percentage > 0)
                                                                     ? (int)Math.Round(agentGroup.Where(p => p.Utilization_Percentage > 0)
                                                                                                 .Average(p => p.Utilization_Percentage))
                                                                     : 0}).ToList();

                if (agent_util.Count > 0)
                {
                    resultDTO.Data = agent_util;
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

        public ResultDTO GetSkillSetWiseProductivity(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetSkillsetWiseProd_ResponseDTO> ss_prod = new List<GetSkillsetWiseProd_ResponseDTO>();

                var skillsetIds = getSkillsetWiseProductivity_DTO.SkillSetId;

                if (getSkillsetWiseProductivity_DTO.TeamId != null)
                {
                    ss_prod = _oMTDataContext.Productivity_Percentage
                                                                        .Join(_oMTDataContext.TeamAssociation,
                                                                            pu => pu.AgentUserId,
                                                                            ta => ta.UserId,
                                                                            (pu, ta) => new { pu, ta })
                                                                        .Join(_oMTDataContext.UserProfile,
                                                                            pu_ta => pu_ta.pu.AgentUserId,
                                                                            up => up.UserId,
                                                                            (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                                        .Join(_oMTDataContext.SkillSet,
                                                                            data => data.pu.SkillSetId,
                                                                            ss => ss.SkillSetId,
                                                                            (data, ss) => new { data.pu, data.ta, data.up, ss.SkillSetName })
                                                                        .Where(data => data.ta.TeamId == getSkillsetWiseProductivity_DTO.TeamId
                                                                                    && data.up.IsActive == true
                                                                                    && skillsetIds.Contains(data.pu.SkillSetId)
                                                                                    && data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date
                                                                                    && data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                        .AsEnumerable()
                                                                        .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                        .Select(group => new
                                                                        {
                                                                            AgentId = group.Key.AgentUserId,
                                                                            AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                            SkillSet = group.Key.SkillSetName,
                                                                            DatewiseData = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => new
                                                                                {
                                                                                    Date = g.Key,
                                                                                    Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                })
                                                                                .OrderBy(d => d.Date)
                                                                                .ToList(),

                                                                            // Calculate average ignoring 0 productivity values
                                                                            AverageProductivity = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                .Where(sum => sum > 0)
                                                                                .DefaultIfEmpty(0)
                                                                                .Average()
                                                                        })
                                                                        .ToList()
                                                                        .Select(x => new GetSkillsetWiseProd_ResponseDTO
                                                                        {
                                                                            AgentName = x.AgentName,
                                                                            SkillSet = x.SkillSet,
                                                                            DatewiseData = x.DatewiseData
                                                                                .Select(d => new GetTeamProd_DatewisedataDTO
                                                                                {
                                                                                    Date = d.Date.ToString("MM-dd-yyyy"),
                                                                                    Productivity = d.Productivity
                                                                                })
                                                                                .ToList(),
                                                                            OverallProductivity = (int)Math.Round(x.AverageProductivity) // Ensure integer output
                                                                        })
                                                                        .ToList();
                }

                else if (getSkillsetWiseProductivity_DTO.TeamId == null)
                {
                    ss_prod = _oMTDataContext.Productivity_Percentage
                                                                      .Join(_oMTDataContext.UserProfile,
                                                                           pu => pu.AgentUserId,
                                                                           up => up.UserId,
                                                                           (pu, up) => new { pu, up })
                                                                      .Join(_oMTDataContext.SkillSet,
                                                                          data => data.pu.SkillSetId,
                                                                          ss => ss.SkillSetId,
                                                                          (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                      .Where(data => data.up.IsActive == true
                                                                                  && skillsetIds.Contains(data.pu.SkillSetId)
                                                                                  && data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date
                                                                                  && data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                      .AsEnumerable()
                                                                      .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                      .Select(group => new
                                                                      {
                                                                          AgentId = group.Key.AgentUserId,
                                                                          AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                          SkillSet = group.Key.SkillSetName,
                                                                          DatewiseData = group
                                                                              .GroupBy(g => g.pu.Createddate.Date)
                                                                              .Select(g => new
                                                                              {
                                                                                  Date = g.Key,
                                                                                  Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                              })
                                                                              .OrderBy(d => d.Date)
                                                                              .ToList(),

                                                                          // Calculate average ignoring 0 productivity values
                                                                          AverageProductivity = group
                                                                              .GroupBy(g => g.pu.Createddate.Date)
                                                                              .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                              .Where(sum => sum > 0) // Ignore 0 values
                                                                              .DefaultIfEmpty(0) // Avoid division by zero
                                                                              .Average() // Compute average
                                                                      })
                                                                      .ToList()
                                                                      .Select(x => new GetSkillsetWiseProd_ResponseDTO
                                                                      {
                                                                          AgentName = x.AgentName,
                                                                          SkillSet = x.SkillSet,
                                                                          DatewiseData = x.DatewiseData
                                                                              .Select(d => new GetTeamProd_DatewisedataDTO
                                                                              {
                                                                                  Date = d.Date.ToString("MM-dd-yyyy"),
                                                                                  Productivity = d.Productivity
                                                                              })
                                                                              .ToList(),
                                                                          OverallProductivity = (int)Math.Round(x.AverageProductivity) // Ensure integer output
                                                                      })
                                                                      .ToList();
                }

                if (ss_prod.Count > 0)
                {

                    GetSkillsetProd_AverageDTO getProd_AverageDTO = new GetSkillsetProd_AverageDTO();

                    getProd_AverageDTO.Productivity = ss_prod; getProd_AverageDTO.TotalOverallProductivity = ss_prod
                                                                                                                     .Where(x => x.OverallProductivity > 0)   // Exclude zero productivity values
                                                                                                                     .Any()                                   // Check if there are any non-zero values
                                                                                                                     ? (int)Math.Round(ss_prod.Where(x => x.OverallProductivity > 0)
                                                                                                                                              .Average(x => x.OverallProductivity))  // Calculate average
                                                                                                                     : 0;                                     // Default to 0 if no non-zero values


                    resultDTO.Data = getProd_AverageDTO;
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

        public ResultDTO GetSkillSetWiseUtilization(GetSkillsetWiseProductivity_DTO getSkillsetWiseProductivity_DTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetSkillsetWiseUtil_ResponseDTO> ss_util = new List<GetSkillsetWiseUtil_ResponseDTO>();

                var skillsetIds = getSkillsetWiseProductivity_DTO.SkillSetId;

                if (getSkillsetWiseProductivity_DTO.TeamId != null)
                {
                    ss_util = _oMTDataContext.Productivity_Percentage
                                                                        .Join(_oMTDataContext.TeamAssociation,
                                                                            pu => pu.AgentUserId,
                                                                            ta => ta.UserId,
                                                                            (pu, ta) => new { pu, ta })
                                                                        .Join(_oMTDataContext.UserProfile,
                                                                            pu_ta => pu_ta.pu.AgentUserId,
                                                                            up => up.UserId,
                                                                            (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                                        .Join(_oMTDataContext.SkillSet,
                                                                            data => data.pu.SkillSetId,
                                                                            ss => ss.SkillSetId,
                                                                            (data, ss) => new { data.pu, data.ta, data.up, ss.SkillSetName })
                                                                        .Where(data => data.ta.TeamId == getSkillsetWiseProductivity_DTO.TeamId
                                                                                    && data.up.IsActive == true
                                                                                    && skillsetIds.Contains(data.pu.SkillSetId)
                                                                                    && data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date
                                                                                    && data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                        .AsEnumerable()
                                                                        .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                        .Select(group => new
                                                                        {
                                                                            AgentId = group.Key.AgentUserId,
                                                                            AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                            SkillSet = group.Key.SkillSetName,
                                                                            DatewiseData = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => new
                                                                                {
                                                                                    Date = g.Key,
                                                                                    Utilization = g.Sum(x => x.pu.Utilization)
                                                                                })
                                                                                .OrderBy(d => d.Date)
                                                                                .ToList(),

                                                                            AverageUtilization = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                .Where(sum => sum > 0)
                                                                                .DefaultIfEmpty(0)
                                                                                .Average()
                                                                        })
                                                                        .ToList()
                                                                        .Select(x => new GetSkillsetWiseUtil_ResponseDTO
                                                                        {
                                                                            AgentName = x.AgentName,
                                                                            SkillSet = x.SkillSet,
                                                                            DatewiseData = x.DatewiseData
                                                                                .Select(d => new GetTeamUtil_DatewisedataDTO
                                                                                {
                                                                                    Date = d.Date.ToString("MM-dd-yyyy"),
                                                                                    Utilization = d.Utilization
                                                                                })
                                                                                .ToList(),
                                                                            OverallUtilization = (int)Math.Round(x.AverageUtilization) 
                                                                        })
                                                                        .ToList();
                }

                else if (getSkillsetWiseProductivity_DTO.TeamId == null)
                {
                    ss_util = _oMTDataContext.Productivity_Percentage
                                                                      .Join(_oMTDataContext.UserProfile,
                                                                           pu => pu.AgentUserId,
                                                                           up => up.UserId,
                                                                           (pu, up) => new { pu, up })
                                                                      .Join(_oMTDataContext.SkillSet,
                                                                          data => data.pu.SkillSetId,
                                                                          ss => ss.SkillSetId,
                                                                          (data, ss) => new { data.pu, data.up, ss.SkillSetName })
                                                                      .Where(data => data.up.IsActive == true
                                                                                  && skillsetIds.Contains(data.pu.SkillSetId)
                                                                                  && data.pu.Createddate.Date >= getSkillsetWiseProductivity_DTO.FromDate.Date
                                                                                  && data.pu.Createddate.Date <= getSkillsetWiseProductivity_DTO.ToDate.Date)
                                                                       .AsEnumerable()
                                                                        .GroupBy(data => new { data.SkillSetName, data.pu.AgentUserId, data.up.FirstName, data.up.LastName })
                                                                        .Select(group => new
                                                                        {
                                                                            AgentId = group.Key.AgentUserId,
                                                                            AgentName = group.Key.FirstName + " " + group.Key.LastName,
                                                                            SkillSet = group.Key.SkillSetName,
                                                                            DatewiseData = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => new
                                                                                {
                                                                                    Date = g.Key,
                                                                                    Utilization = g.Sum(x => x.pu.Utilization)
                                                                                })
                                                                                .OrderBy(d => d.Date)
                                                                                .ToList(),

                                                                            AverageUtilization = group
                                                                                .GroupBy(g => g.pu.Createddate.Date)
                                                                                .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                .Where(sum => sum > 0)
                                                                                .DefaultIfEmpty(0)
                                                                                .Average()
                                                                        })
                                                                        .ToList()
                                                                        .Select(x => new GetSkillsetWiseUtil_ResponseDTO
                                                                        {
                                                                            AgentName = x.AgentName,
                                                                            SkillSet = x.SkillSet,
                                                                            DatewiseData = x.DatewiseData
                                                                                .Select(d => new GetTeamUtil_DatewisedataDTO
                                                                                {
                                                                                    Date = d.Date.ToString("MM-dd-yyyy"),
                                                                                    Utilization = d.Utilization
                                                                                })
                                                                                .ToList(),
                                                                            OverallUtilization = (int)Math.Round(x.AverageUtilization) 
                                                                        })
                                                                        .ToList();
                }

                if (ss_util.Count > 0)
                {
                    GetSkillsetUtil_AverageDTO getProd_AverageDTO = new GetSkillsetUtil_AverageDTO();

                    getProd_AverageDTO.Utilization = ss_util;
                    getProd_AverageDTO.TotalOverallUtilization = ss_util
                                                                        .Where(x => x.OverallUtilization > 0)   
                                                                        .Any()                                
                                                                        ? (int)Math.Round(ss_util.Where(x => x.OverallUtilization > 0)
                                                                                                 .Average(x => x.OverallUtilization))  
                                                                        : 0;                                    



                    resultDTO.Data = getProd_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SkillSet Utilization details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SkillSet Utilization details not found";
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

        public ResultDTO GetSorWiseProductivity(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetTeamProd_ResponseDTO> sor_prod = new List<GetTeamProd_ResponseDTO>();

                if (getSorWiseProductivity_DTO.TeamId != null)
                {
                    sor_prod = _oMTDataContext.Productivity_Percentage
                                                                          .Join(_oMTDataContext.TeamAssociation,
                                                                              pu => pu.AgentUserId,
                                                                              ta => ta.UserId,
                                                                              (pu, ta) => new { pu, ta })
                                                                          .Join(_oMTDataContext.UserProfile,
                                                                              pu_ta => pu_ta.pu.AgentUserId,
                                                                              up => up.UserId,
                                                                              (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                                          .Where(data => data.ta.TeamId == getSorWiseProductivity_DTO.TeamId
                                                                                      && data.up.IsActive == true
                                                                                      && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                      && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                      && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                          .AsEnumerable() // Switch to in-memory processing
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
                                                                                      Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                                  })
                                                                                  .OrderBy(d => d.Date)
                                                                                  .ToList(),

                                                                              // Calculate average ignoring 0 productivity values
                                                                              AverageProductivity = group
                                                                                  .GroupBy(g => g.pu.Createddate.Date)
                                                                                  .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                                  .Where(sum => sum > 0) // Ignore 0 values
                                                                                  .DefaultIfEmpty(0) // Avoid division by zero
                                                                                  .Average() // Compute average
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
                                                                              OverallProductivity = (int)Math.Round(x.AverageProductivity) // Ensure integer output
                                                                          })
                                                                          .ToList();

                }

                else if (getSorWiseProductivity_DTO.TeamId == null)
                {
                    sor_prod = _oMTDataContext.Productivity_Percentage
                                                                       .Join(_oMTDataContext.UserProfile,
                                                                           pu => pu.AgentUserId,
                                                                           up => up.UserId,
                                                                           (pu, up) => new { pu, up })
                                                                       .Where(data => data.up.IsActive == true
                                                                                   && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                   && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                   && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                       .AsEnumerable() // Switch to in-memory processing
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
                                                                                   Productivity = g.Sum(x => x.pu.ProductivityPercentage)
                                                                               })
                                                                               .OrderBy(d => d.Date)
                                                                               .ToList(),

                                                                           // Calculate average ignoring 0 productivity values
                                                                           AverageProductivity = group
                                                                               .GroupBy(g => g.pu.Createddate.Date)
                                                                               .Select(g => g.Sum(x => x.pu.ProductivityPercentage))
                                                                               .Where(sum => sum > 0) // Ignore 0 values
                                                                               .DefaultIfEmpty(0) // Avoid division by zero
                                                                               .Average()
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
                                                                           OverallProductivity = (int)Math.Round(x.AverageProductivity) // Ensure integer output
                                                                       })
                                                                       .ToList();

                }
                if (sor_prod.Count > 0)
                {

                    GetSorProd_AverageDTO getProd_AverageDTO = new GetSorProd_AverageDTO();

                    getProd_AverageDTO.Productivity = sor_prod;
                    getProd_AverageDTO.TotalOverallProductivity = sor_prod
                                                                          .Where(x => x.OverallProductivity > 0)   
                                                                          .Any()                                   
                                                                          ? (int)Math.Round(sor_prod.Where(x => x.OverallProductivity > 0)
                                                                                                    .Average(x => x.OverallProductivity))  
                                                                          : 0;                              



                    resultDTO.Data = getProd_AverageDTO;
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

        public ResultDTO GetSorWiseUtilization(GetSorWiseProductivity_DTO getSorWiseProductivity_DTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<GetTeamUtil_ResponseDTO> sor_util = new List<GetTeamUtil_ResponseDTO>();

                if (getSorWiseProductivity_DTO.TeamId != null)
                {
                    sor_util = _oMTDataContext.Productivity_Percentage
                                                                          .Join(_oMTDataContext.TeamAssociation,
                                                                              pu => pu.AgentUserId,
                                                                              ta => ta.UserId,
                                                                              (pu, ta) => new { pu, ta })
                                                                          .Join(_oMTDataContext.UserProfile,
                                                                              pu_ta => pu_ta.pu.AgentUserId,
                                                                              up => up.UserId,
                                                                              (pu_ta, up) => new { pu_ta.pu, pu_ta.ta, up })
                                                                          .Where(data => data.ta.TeamId == getSorWiseProductivity_DTO.TeamId
                                                                                      && data.up.IsActive == true
                                                                                      && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                      && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                      && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                          .AsEnumerable() // Switch to in-memory processing
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
                                                                                      Utilization = g.Sum(x => x.pu.Utilization)

                                                                                  })
                                                                                  .OrderBy(d => d.Date)
                                                                                  .ToList(),

                                                                              // Calculate average ignoring 0 productivity values
                                                                              AverageUtilization = group
                                                                                  .GroupBy(g => g.pu.Createddate.Date)
                                                                                  .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                  .Where(sum => sum > 0) // Ignore 0 values
                                                                                  .DefaultIfEmpty(0) // Avoid division by zero
                                                                                  .Average() // Compute average
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
                                                                              OverallUtilization = (int)Math.Round(x.AverageUtilization) // Ensure integer output
                                                                          })
                                                                          .ToList();

                }

                else if (getSorWiseProductivity_DTO.TeamId == null)
                {
                    sor_util = _oMTDataContext.Productivity_Percentage
                                                                       .Join(_oMTDataContext.UserProfile,
                                                                           pu => pu.AgentUserId,
                                                                           up => up.UserId,
                                                                           (pu, up) => new { pu, up })
                                                                       .Where(data => data.up.IsActive == true
                                                                                   && data.pu.SystemofRecordId == getSorWiseProductivity_DTO.SystemOfRecordId
                                                                                   && data.pu.Createddate.Date >= getSorWiseProductivity_DTO.FromDate.Date
                                                                                   && data.pu.Createddate.Date <= getSorWiseProductivity_DTO.ToDate.Date)
                                                                       .AsEnumerable() // Switch to in-memory processing
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
                                                                                      Utilization = g.Sum(x => x.pu.Utilization)

                                                                                  })
                                                                                  .OrderBy(d => d.Date)
                                                                                  .ToList(),

                                                                           // Calculate average ignoring 0 productivity values
                                                                           AverageUtilization = group
                                                                                  .GroupBy(g => g.pu.Createddate.Date)
                                                                                  .Select(g => g.Sum(x => x.pu.Utilization))
                                                                                  .Where(sum => sum > 0) // Ignore 0 values
                                                                                  .DefaultIfEmpty(0) // Avoid division by zero
                                                                                  .Average() // Compute average
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
                                                                              OverallUtilization = (int)Math.Round(x.AverageUtilization) // Ensure integer output
                                                                          })
                                                                          .ToList();

                }
                if (sor_util.Count > 0)
                {

                    GetSorUtil_AverageDTO getUtil_AverageDTO = new GetSorUtil_AverageDTO();

                    getUtil_AverageDTO.Utilization = sor_util;
                    getUtil_AverageDTO.TotalOverallUtilization = sor_util
                                                                         .Where(x => x.OverallUtilization > 0)   
                                                                         .Any()                               
                                                                         ? (int)Math.Round(sor_util.Where(x => x.OverallUtilization > 0)
                                                                                                   .Average(x => x.OverallUtilization))   
                                                                         : 0;                                   



                    resultDTO.Data = getUtil_AverageDTO;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SOR Utilization details fetched successfully";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SOR Utilization details not found";
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
