using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace OMT.DataService.Service
{
    public class DateAndNphService : IDateAndNphService
    {
        private readonly OMTDataContext _oMTDataContext;

        public DateAndNphService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetCheckinDetails(GetCheckinDetailsDTO getCheckinDetailsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var pagination = getCheckinDetailsDTO.Pagination;

                var todaydate = DateTime.UtcNow.Date;
                var teamid = 0;

                if (getCheckinDetailsDTO.TeamId == null)
                {
                    teamid = _oMTDataContext.Teams.Where(x => x.TL_Userid == userid && x.IsActive).Select(x => x.TeamId).FirstOrDefault();

                }
                else
                {
                    teamid = (int)getCheckinDetailsDTO.TeamId;
                }

                if (teamid != 0)
                {
                    var checkindetails = (from ta in _oMTDataContext.TeamAssociation
                                          join uc in _oMTDataContext.User_Checkin on ta.UserId equals uc.UserId
                                          join up in _oMTDataContext.UserProfile on ta.UserId equals up.UserId
                                          join t in _oMTDataContext.Teams on ta.TeamId equals t.TeamId
                                          where ta.TeamId == teamid && up.IsActive && t.IsActive && uc.Prod_Util_Calculated == false && uc.CheckIn_date >= todaydate.AddDays(-1)
                                          orderby up.FirstName, uc.CheckIn_date
                                          select new
                                          {
                                              UserId = uc.UserId,
                                              UserName = up.FirstName + " " + up.LastName,
                                              Checkin = uc.Checkin,
                                              Checkout = uc.Checkout,
                                              Checkin_date = uc.CheckIn_date,
                                          }).ToList();

                    if (checkindetails.Count > 0)
                    {
                        if (pagination.IsPagination)
                        {
                            var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                            var paginatedData = checkindetails.Skip(skip).Take(pagination.NoOfRecords).ToList();
                            var totalRecords = checkindetails.Count;
                            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.NoOfRecords);

                            var paginationOutput = new PaginationOutputDTO
                            {
                                Records = paginatedData.Cast<object>().ToList(),
                                PageNo = pagination.PageNo,
                                NoOfPages = totalPages,
                                TotalCount = totalRecords,

                            };

                            resultDTO.Data = paginationOutput;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "List of checkin details";
                        }
                        else
                        {

                            resultDTO.Data = checkindetails;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "List of checkin details";
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Checkin details not found";
                        resultDTO.StatusCode = "404";
                    }
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

        public ResultDTO UpdateCheckinDetails(UpdateCheckinDetailsDTO updateCheckinDetailsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                DateTime todayUtc = DateTime.UtcNow.Date; // Today at midnight in UTC
                DateTime endtime = todayUtc.AddHours(12).AddMinutes(30);

                if (DateTime.UtcNow > endtime)
                {
                    resultDTO.Data = null;
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "You can't update the Checkin date after 6 PM.";
                }
                else
                {

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
