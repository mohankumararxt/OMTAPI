using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                                                                  .GroupBy(g => g.pu.Createddate.Date) // Group by Date
                                                                  .Select(g => new
                                                                  {
                                                                      Date = g.Key,
                                                                      Productivity = g.Sum(x => x.pu.Productivity_Percentage) // Sum of productivity for that day
                                                                  })
                                                                  .OrderBy(d => d.Date)
                                                                  .ToList(),
                                                              AverageProductivity = group.Average(g => g.pu.Productivity_Percentage) // Average across all dates for that agent
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
                                                              AverageProductivity = (decimal)x.AverageProductivity
                                                          })
                                                          .ToList();





                if (prod_Util.Count > 0)
                {
                    resultDTO.Data = prod_Util;
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
                                                              AverageUtilization = group.Average(g => g.pu.Utilization_Percentage) // Average across all dates for that agent
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
                                                              AverageUtilization = (decimal)x.AverageUtilization
                                                          })
                                                          .ToList();


                if (prod_Util.Count > 0)
                {
                    resultDTO.Data = prod_Util;
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
    }
}
