using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var teamid = 0;
                int? roleid = 0;

                if (getCheckinDetailsDTO.TeamId == null)
                {
                    roleid = _oMTDataContext.UserProfile.Where(x => x.UserId == userid && x.IsActive).Select(x => x.RoleId).FirstOrDefault();

                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams.Where(x => x.TL_Userid == userid && x.IsActive).Select(x => x.TeamId).FirstOrDefault();
                    }
                    else
                    {
                        teamid = 0;
                    }

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
                                          where ta.TeamId == teamid
                                                && up.IsActive
                                                && t.IsActive
                                                && uc.Prod_Util_Calculated == false
                                                && uc.CheckIn_date >= todaydate.AddDays(-1) && uc.Checkout != null
                                          orderby up.FirstName, uc.Checkin
                                          select new
                                          {
                                              uc.Id,
                                              uc.UserId,
                                              up.FirstName,
                                              up.LastName,
                                              uc.Checkin,
                                              uc.Checkout,
                                              uc.CheckIn_date,
                                              t.TeamName,
                                          })
                                           .ToList()
                                           .Select(x => new
                                           {
                                               User_CheckinId = x.Id,
                                               UserId = x.UserId,
                                               UserName = x.FirstName + " " + x.LastName,
                                               Checkin = x.Checkin.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(x.Checkin.Value, istZone) : (DateTime?)null,
                                               Checkout = x.Checkout.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(x.Checkout.Value, istZone) : (DateTime?)null,
                                               Checkin_date = x.CheckIn_date,
                                               TeamName = x.TeamName,
                                           })
                                           .ToList();


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
                else if (teamid == 0 && roleid != 1)
                {

                    var checkindetails = (from ta in _oMTDataContext.TeamAssociation
                                          join uc in _oMTDataContext.User_Checkin on ta.UserId equals uc.UserId
                                          join up in _oMTDataContext.UserProfile on ta.UserId equals up.UserId
                                          join t in _oMTDataContext.Teams on ta.TeamId equals t.TeamId
                                          where up.IsActive
                                                && t.IsActive
                                                && uc.Prod_Util_Calculated == false
                                                && uc.CheckIn_date >= todaydate.AddDays(-1) && uc.Checkout != null
                                          orderby up.FirstName, uc.Checkin
                                          select new
                                          {
                                              uc.Id,
                                              uc.UserId,
                                              up.FirstName,
                                              up.LastName,
                                              uc.Checkin,
                                              uc.Checkout,
                                              uc.CheckIn_date,
                                              t.TeamName,
                                          })
                                          .ToList()
                                          .Select(x => new
                                          {
                                              User_CheckinId = x.Id,
                                              UserId = x.UserId,
                                              UserName = x.FirstName + " " + x.LastName,
                                              Checkin = x.Checkin.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(x.Checkin.Value, istZone) : (DateTime?)null,
                                              Checkout = x.Checkout.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(x.Checkout.Value, istZone) : (DateTime?)null,
                                              Checkin_date = x.CheckIn_date,
                                              TeamName = x.TeamName,
                                          })
                                          .ToList();


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
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Checkin details not found";
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

        public ResultDTO UpdateCheckinDetails(UpdateCheckinDetailsDTO updateCheckinDetailsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var uc = _oMTDataContext.User_Checkin.Where(x => x.Id == updateCheckinDetailsDTO.User_CheckinId && x.Prod_Util_Calculated == false && x.UserId == updateCheckinDetailsDTO.UserId).FirstOrDefault();

                if (uc == null)
                {
                    resultDTO.Data = null;
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The selected details are not found.";
                }
                else
                {
                    uc.Checkout = null;

                    _oMTDataContext.User_Checkin.Update(uc);
                    _oMTDataContext.SaveChanges();

                    resultDTO.StatusCode = "200";
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Checkout has been successfully removed.";
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

        public ResultDTO GetNonProductiveReasons()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var nphreasons = _oMTDataContext.NonProductiveReasons.Where(x => x.IsActive).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Non Productive Reasons";
                resultDTO.Data = nphreasons;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO ApplyNonProductiveHours(ApplyNonProductiveHoursDTO applyNonProductiveHoursDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var shifroasterdetails = (from sa in _oMTDataContext.ShiftAssociation
                                          join up in _oMTDataContext.UserProfile on sa.AgentEmployeeId equals up.EmployeeId
                                          join up2 in _oMTDataContext.UserProfile on sa.TLEmployeeId equals up2.EmployeeId
                                          where up.IsActive && up.UserId == userid && sa.IsActive && sa.ShiftDate == applyNonProductiveHoursDTO.NonProductiveHours_Date
                                          select new
                                          {
                                              tluserid = up2.UserId,
                                              primary_sorid = sa.PrimarySystemOfRecordId,
                                          }).FirstOrDefault();

                if (shifroasterdetails == null)
                {
                    resultDTO.Data = null;
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift roaster is not uploaded for the applied date,so you can't apply for regularization.";
                }
                else
                {
                    NonProductiveRegularization nonProductiveRegularization = new NonProductiveRegularization()
                    {
                        UserId = userid,
                        TlUserId = shifroasterdetails.tluserid,
                        Primary_SorId = shifroasterdetails.primary_sorid,
                        Reasons = applyNonProductiveHoursDTO.Reasons,
                        Remarks = applyNonProductiveHoursDTO.Remarks,
                        NonProductiveHours_Date = applyNonProductiveHoursDTO.NonProductiveHours_Date,
                        StartTime = applyNonProductiveHoursDTO.StartTime,
                        EndTime = applyNonProductiveHoursDTO.EndTime,
                        Applied_Hours = applyNonProductiveHoursDTO.Applied_Hours,
                        Regularization_Status = 1,
                        Applied_Time = DateTime.UtcNow,
                        UpdatedBy = null,
                        UpdatedTime = null,
                        TlDescription = null,
                    };

                    _oMTDataContext.NonProductiveRegularization.Add(nonProductiveRegularization);
                    _oMTDataContext.SaveChanges();


                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Regularization applied successfully";
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

        public ResultDTO GetNonProductiveRegularizations_Agent(PaginationInputDTO paginationInputDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var today = DateTime.UtcNow.Date;
                var fromDate = today.AddDays(-30);

                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var regularizations = (from npr in _oMTDataContext.NonProductiveRegularization
                                       join up in _oMTDataContext.UserProfile on npr.UserId equals up.UserId
                                       join up2 in _oMTDataContext.UserProfile on npr.TlUserId equals up2.UserId
                                       join sor in _oMTDataContext.SystemofRecord on npr.Primary_SorId equals sor.SystemofRecordId
                                       join r in _oMTDataContext.NonProductiveReasons on npr.Reasons equals r.Id
                                       join rs in _oMTDataContext.Regularization_Status on npr.Regularization_Status equals rs.Id
                                       where npr.UserId == userid && up.IsActive && npr.Applied_Time.Date >= fromDate && npr.Applied_Time.Date <= today
                                       orderby npr.Applied_Time
                                       select new
                                       {
                                           NonProductiveRegularizationId = npr.Id,
                                           Userid = userid,
                                           Tl_UserId = npr.TlUserId,
                                           TL_Name = up2.FirstName + " " + up2.LastName,
                                           Primary_SOR = sor.SystemofRecordName,
                                           Reasons = r.Reasons,
                                           Remarks = npr.Remarks,
                                           Date = npr.NonProductiveHours_Date.Date.ToString("dd-MM-yyyy"),
                                           StartTime = TimeZoneInfo.ConvertTimeFromUtc(npr.StartTime, istZone).ToString("HH:mm"),
                                           EndTime = TimeZoneInfo.ConvertTimeFromUtc(npr.EndTime, istZone).ToString("HH:mm"),
                                           Hours = npr.Applied_Hours,
                                           Status = rs.Status_Name,
                                           Applied_Time = TimeZoneInfo.ConvertTimeFromUtc(npr.Applied_Time, istZone).ToString("dd-MM-yyyy HH:mm"),
                                           Tl_Description = npr.TlDescription
                                       }).ToList();

                if (regularizations.Count > 0)
                {
                    if (paginationInputDTO.IsPagination)
                    {
                        var skip = (paginationInputDTO.PageNo - 1) * paginationInputDTO.NoOfRecords;
                        var paginatedData = regularizations.Skip(skip).Take(paginationInputDTO.NoOfRecords).ToList();
                        var totalRecords = regularizations.Count;
                        var totalPages = (int)Math.Ceiling((double)totalRecords / paginationInputDTO.NoOfRecords);

                        var paginationOutput = new PaginationOutputDTO
                        {
                            Records = paginatedData.Cast<object>().ToList(),
                            PageNo = paginationInputDTO.PageNo,
                            NoOfPages = totalPages,
                            TotalCount = totalRecords,

                        };

                        resultDTO.Data = paginationOutput;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "List of regularizations details";
                    }
                    else
                    {
                        resultDTO.Data = regularizations;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "List of regularizations details";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Regularization details not found";
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

        public ResultDTO GetRegularizationStatus()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var rstatus = _oMTDataContext.Regularization_Status.Where(x => x.IsActive && x.IsTlStatus).ToList();

                if (rstatus.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Non Productive Reasons";
                    resultDTO.Data = rstatus;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Non Productive Reasons not found";
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

        public ResultDTO GetNonProductiveRegularizations(GetCheckinDetailsDTO getCheckinDetailsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var pagination = getCheckinDetailsDTO.Pagination;
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var today = DateTime.UtcNow.Date;
                var fromDate = today.AddDays(-30);

                int? teamid = getCheckinDetailsDTO.TeamId;
                int? roleid = 0;

                // Determine team and role if TeamId not provided
                if (teamid == null)
                {
                    roleid = _oMTDataContext.UserProfile
                        .Where(x => x.UserId == userid && x.IsActive)
                        .Select(x => x.RoleId)
                        .FirstOrDefault();

                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                            .Where(x => x.TL_Userid == userid && x.IsActive)
                            .Select(x => x.TeamId)
                            .FirstOrDefault();
                    }
                    else
                    {
                        teamid = 0;
                    }
                }

                bool hasTeam = teamid.HasValue && teamid.Value != 0;

                if (hasTeam || (!hasTeam && roleid != 1))
                {
                    var regularizations = (from npr in _oMTDataContext.NonProductiveRegularization
                                           join up in _oMTDataContext.UserProfile on npr.UserId equals up.UserId
                                           join up2 in _oMTDataContext.UserProfile on npr.TlUserId equals up2.UserId
                                           join sor in _oMTDataContext.SystemofRecord on npr.Primary_SorId equals sor.SystemofRecordId
                                           join r in _oMTDataContext.NonProductiveReasons on npr.Reasons equals r.Id
                                           join rs in _oMTDataContext.Regularization_Status on npr.Regularization_Status equals rs.Id
                                           join t in _oMTDataContext.Teams on npr.TlUserId equals t.TL_Userid
                                           // join ta in _oMTDataContext.TeamAssociation on t.TeamId equals ta.TeamId
                                           where up.IsActive
                                                 && t.IsActive
                                                 && npr.Applied_Time.Date >= fromDate
                                                 && npr.Applied_Time.Date <= today
                                                 // apply team filter only if we actually have a team
                                                 && (!hasTeam || t.TeamId == teamid.Value)
                                           orderby npr.Applied_Time
                                           select new
                                           {
                                               NonProductiveRegularizationId = npr.Id,
                                               UserId = npr.UserId,
                                               AgentName = up.FirstName + " " + up.LastName,
                                               TL_UserId = npr.TlUserId,
                                               TL_Name = up2.FirstName + " " + up2.LastName,
                                               Primary_SOR = sor.SystemofRecordName,
                                               Reasons = r.Reasons,
                                               Remarks = npr.Remarks,
                                               Date = npr.NonProductiveHours_Date.Date.ToString("dd-MM-yyyy"),
                                               StartTime = TimeZoneInfo.ConvertTimeFromUtc(npr.StartTime, istZone).ToString("HH:mm"),
                                               EndTime = TimeZoneInfo.ConvertTimeFromUtc(npr.EndTime, istZone).ToString("HH:mm"),
                                               Hours = npr.Applied_Hours,
                                               Status = rs.Status_Name,
                                               StatusId = rs.Id,
                                               Applied_Time = TimeZoneInfo.ConvertTimeFromUtc(npr.Applied_Time, istZone).ToString("dd-MM-yyyy HH:mm"),
                                               Tl_Description = npr.TlDescription,
                                               IsEditable = true,

                                           }).ToList();

                    if (regularizations.Count > 0)
                    {
                        if (pagination.IsPagination)
                        {
                            var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                            var paginatedData = regularizations.Skip(skip).Take(pagination.NoOfRecords).ToList();
                            var totalRecords = regularizations.Count;
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
                            resultDTO.Message = "List of regularization details";
                        }
                        else
                        {

                            resultDTO.Data = regularizations;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "List of regularization details";
                        }

                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Regularization details not found";
                        resultDTO.StatusCode = "404";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Regularization details not found";
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

        public ResultDTO UpdateRegularizations(UpdateRegularizationsDTO updateRegularizationsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            string? connectionstring = _oMTDataContext.Database.GetConnectionString();
            using SqlConnection connection = new(connectionstring);

            try
            {
                // check if the regularized date is yesterday , then just update it, if any date less than yesterday, call recalculation sp

                var reg = _oMTDataContext.NonProductiveRegularization.Where(x => x.Id == updateRegularizationsDTO.Id).FirstOrDefault();

                if (reg != null)
                {
                    reg.Regularization_Status = updateRegularizationsDTO.Regularization_Status;
                    reg.UpdatedBy = userid;
                    reg.UpdatedTime = DateTime.UtcNow;
                    reg.TlDescription = updateRegularizationsDTO.TlDescription == null ? null : updateRegularizationsDTO.TlDescription;

                    _oMTDataContext.NonProductiveRegularization.Update(reg);
                    _oMTDataContext.SaveChanges();

                    var processDate = reg.NonProductiveHours_Date.Date;
                    var agentuserid = reg.UserId;

                    var utcNow = DateTime.UtcNow;

                    var todayUtc = utcNow.Date;
                    var yesterdayUtc = todayUtc.AddDays(-1);

                    // 8 PM IST = 2:30 PM UTC
                    var isAfter8PM_IST = utcNow.TimeOfDay >= new TimeSpan(14, 30, 0);

                    var shouldRecalculate =
                    (processDate < yesterdayUtc) || // older dates
                    (processDate == yesterdayUtc && isAfter8PM_IST); // yesterday after 8PM IST

                    if (shouldRecalculate)
                    {
                        using (SqlCommand spCommand = new SqlCommand("Master_Productivity_Percentage_For_User_Date", connection))
                        {
                            connection.Open();
                            spCommand.CommandType = CommandType.StoredProcedure;

                            SqlParameter returnValue = new SqlParameter
                            {
                                ParameterName = "@RETURN_VALUE",
                                Direction = ParameterDirection.ReturnValue
                            };

                            spCommand.Parameters.AddWithValue("@UserId", agentuserid);
                            spCommand.Parameters.AddWithValue("@ProcessDate", processDate);

                            spCommand.Parameters.Add(returnValue);
                            spCommand.ExecuteNonQuery();

                            int returnCode = (int)spCommand.Parameters["@RETURN_VALUE"].Value;

                            if (returnCode != 1)
                            {
                                throw new InvalidOperationException("Stored Procedure call failed.");
                            }
                            else
                            {
                                Console.WriteLine($"Productivity_Percentage table updated successfully.");
                            }

                        }

                        using (SqlCommand spCommand2 = new SqlCommand("Calculate_Prod_Util_For_User_Date", connection))
                        {
                            spCommand2.CommandType = CommandType.StoredProcedure;

                            SqlParameter returnValue2 = new SqlParameter
                            {
                                ParameterName = "@RETURN_VALUE",
                                Direction = ParameterDirection.ReturnValue
                            };

                            spCommand2.Parameters.AddWithValue("@UserId", agentuserid);
                            spCommand2.Parameters.AddWithValue("@ProcessDate", processDate);

                            spCommand2.Parameters.Add(returnValue2);
                            spCommand2.ExecuteNonQuery();

                            int returnCode2 = (int)spCommand2.Parameters["@RETURN_VALUE"].Value;

                            if (returnCode2 != 1)
                            {
                                throw new InvalidOperationException("Stored Procedure call failed.");
                            }
                            else
                            {
                                Console.WriteLine($"Prod_Util table updated successfully.");
                            }

                        }

                        //calculate monthly sor utilization

                        using (SqlCommand spCommand3 = new SqlCommand("Update_Monthly_Utilization_SOR_For_User_Date", connection))
                        {
                            spCommand3.CommandType = CommandType.StoredProcedure;

                            SqlParameter returnValue2 = new SqlParameter
                            {
                                ParameterName = "@RETURN_VALUE",
                                Direction = ParameterDirection.ReturnValue
                            };

                            spCommand3.Parameters.AddWithValue("@ProcessDate", processDate);

                            spCommand3.Parameters.Add(returnValue2);
                            spCommand3.ExecuteNonQuery();

                            int returnCode2 = (int)spCommand3.Parameters["@RETURN_VALUE"].Value;

                            if (returnCode2 != 1)
                            {
                                throw new InvalidOperationException("Stored Procedure call failed.");
                            }
                            else
                            {
                                Console.WriteLine($"Monthly_Utilization_SOR table updated successfully.");
                            }

                        }

                    }

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Regularization has been approved";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Regularization details not found";
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

        public ResultDTO GetNphProductivity_Agent(GetAgentNphProductivityDTO getAgentNphProductivityDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

                //var agent_nphprod = (from nph in _oMTDataContext.NPH_Productivity
                //                     join up1 in _oMTDataContext.UserProfile on nph.UserId equals up1.UserId
                //                     join up2 in _oMTDataContext.UserProfile on nph.TlUserId equals up2.UserId
                //                     where up1.IsActive && up2.IsActive 
                //                     && nph.Productivity_Date >= getAgentProdUtilDTO.FromDate 
                //                     && nph.Productivity_Date <= getAgentProdUtilDTO.ToDate
                //                     && up1.UserId == getAgentProdUtilDTO.UserId
                //                     orderby nph.Productivity_Date
                //                     group new { nph, up2 } by up1.UserId into g
                //select new GetAgentNphProductivityResponseDTO
                //{
                //    Datewisedata = g.Select(x => new GetNphProductivityResponseDTO
                //    {
                //        Productivity_Date = x.nph.Productivity_Date.ToString("dd-MM-yyyy"),
                //        Applied_Hours = x.nph.Applied_Hours,
                //        Productivity = x.nph.Productivity,
                //        Non_Productive_Productivity = x.nph.Non_Productive_Productivity,
                //        TlName = x.up2.FullName  
                //    }).ToList()
                //}).ToList();
                var pagination = getAgentNphProductivityDTO.Pagination;
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var agent_nphprod = (from nph in _oMTDataContext.NPH_Productivity
                                     join up1 in _oMTDataContext.UserProfile on nph.UserId equals up1.UserId
                                     join up2 in _oMTDataContext.UserProfile on nph.TlUserId equals up2.UserId
                                     where up1.IsActive && up2.IsActive
                                     && nph.Productivity_Date >= getAgentNphProductivityDTO.FromDate
                                     && nph.Productivity_Date <= getAgentNphProductivityDTO.ToDate
                                     && up1.UserId == getAgentNphProductivityDTO.UserId
                                     orderby nph.Productivity_Date
                                     select new
                                     {
                                         Tl_Name = up2.FirstName + " " + up2.LastName,
                                         Productivity_Date = nph.Productivity_Date.ToString("dd-MM-yyyy"),
                                         Applied_Hours = nph.Applied_Hours,
                                         Pending_Orders_Hours = nph.Pending_Orders_Hours,
                                         Productivity = nph.Productivity_Percentage,
                                         Nph_Productivity = nph.NPH_Productivity_Percentage,
                                         Total = nph.Productivity_Percentage + nph.NPH_Productivity_Percentage,
                                         Over_Utilization = nph.Productivity_Percentage + nph.NPH_Productivity_Percentage > 100 ? (nph.Productivity_Percentage + nph.NPH_Productivity_Percentage) - 100 : 0,
                                         Remarks = nph.Remarks
                                     }).ToList();


                if (agent_nphprod.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = agent_nphprod.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = agent_nphprod.Count;
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
                        resultDTO.Message = "List of Non productive productivities details";
                    }
                    else
                    {
                        resultDTO.Data = agent_nphprod;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "List of Non productive productivities details";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Non productive productivities not found";
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

        public ResultDTO GetNphProductivity_Team(GetTeamNphProductivityDTO getTeamNphProductivityDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var pagination = getTeamNphProductivityDTO.Pagination;
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                int? teamid = getTeamNphProductivityDTO.TeamId;
                int? roleid = 0;

                if (teamid == null)
                {
                    roleid = _oMTDataContext.UserProfile
                        .Where(x => x.UserId == userid && x.IsActive)
                        .Select(x => x.RoleId)
                        .FirstOrDefault();

                    if (roleid == 1)
                    {
                        teamid = _oMTDataContext.Teams
                            .Where(x => x.TL_Userid == userid && x.IsActive)
                            .Select(x => x.TeamId)
                            .FirstOrDefault();
                    }
                    else
                    {
                        teamid = 0;
                    }
                }

                bool hasTeam = teamid.HasValue && teamid.Value != 0;

                if (hasTeam || (!hasTeam && roleid != 1))
                {
                    var team_nphprod = (from nph in _oMTDataContext.NPH_Productivity
                                        join up1 in _oMTDataContext.UserProfile on nph.UserId equals up1.UserId
                                        join up2 in _oMTDataContext.UserProfile on nph.TlUserId equals up2.UserId
                                        join t in _oMTDataContext.Teams on nph.TlUserId equals t.TL_Userid
                                        where up1.IsActive && up2.IsActive
                                        && nph.Productivity_Date >= getTeamNphProductivityDTO.FromDate
                                        && nph.Productivity_Date <= getTeamNphProductivityDTO.ToDate
                                        && (!hasTeam || t.TeamId == teamid.Value)
                                        orderby up1.FirstName, nph.Productivity_Date
                                        select new
                                        {
                                            Tl_Name = up2.FirstName + " " + up2.LastName,
                                            UserName = up1.FirstName + " " + up1.LastName,
                                            Productivity_Date = nph.Productivity_Date.ToString("dd-MM-yyyy"),
                                            Applied_Hours = nph.Applied_Hours,
                                            Pending_Orders_Hours = nph.Pending_Orders_Hours,
                                            Productivity = nph.Productivity_Percentage,
                                            Nph_Productivity = nph.NPH_Productivity_Percentage,
                                            Total = nph.Productivity_Percentage + nph.NPH_Productivity_Percentage,
                                            Over_Utilization = nph.Productivity_Percentage + nph.NPH_Productivity_Percentage > 100 ? (nph.Productivity_Percentage + nph.NPH_Productivity_Percentage) - 100 : 0,
                                            Remarks = nph.Remarks
                                        }).Distinct().ToList();

                    if (team_nphprod.Count > 0)
                    {
                        if (pagination.IsPagination)
                        {
                            var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                            var paginatedData = team_nphprod.Skip(skip).Take(pagination.NoOfRecords).ToList();
                            var totalRecords = team_nphprod.Count;
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
                            resultDTO.Message = "List of Non productive productivities details";
                        }
                        else
                        {

                            resultDTO.Data = team_nphprod;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "List of Non productive productivities details";
                        }

                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Non productive productivities not found";
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
    }
}
