using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class SciExceptionService : ISciExceptionService
    {

        private readonly OMTDataContext _oMTDataContext;
        public SciExceptionService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO GetSciExceptionReport(GetSciExceptionReportDTO getSciExceptionReportDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                if (getSciExceptionReportDTO.FromDate != null && getSciExceptionReportDTO.ToDate != null)
                {
                    var SciExceptionOrders = _oMTDataContext.SciException.Where(x => EF.Functions.DateDiffDay(getSciExceptionReportDTO.FromDate, x.Date_Created) >= 0
                                                                                     && EF.Functions.DateDiffDay(getSciExceptionReportDTO.ToDate, x.Date_Created) <= 0)
                                                                         .Select(x => new SciExceptionReportResponseDTO
                                                                         {
                                                                             Id = x.Id,
                                                                             Project = x.Project,
                                                                             Loan = x.Loan,
                                                                             Valid_Invalid = x.Valid_Invalid,
                                                                             Status = x.Status,
                                                                             Question = x.Question,
                                                                             Code = x.Code,
                                                                             CodeName = x.CodeName,
                                                                             Description = x.Description,
                                                                             Comments = x.Comments,
                                                                             Date_Created = x.Date_Created.ToString("MM-dd-yyyy hh:mm:ss tt"),
                                                                         }).ToList();

                    if (SciExceptionOrders.Any())
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Sci Exception Report fetched successfully";
                        resultDTO.Data = SciExceptionOrders;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "No details found.";

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

        public ResultDTO UploadSciExceptionReport(UploadSciExceptionReportDTO uploadSciExceptionReportDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string tabelname = "SciException";

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                using SqlCommand command = new()
                {
                    Connection = connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertSciExceptionReport"
                };

                command.Parameters.AddWithValue("@Tablename", tabelname);
                command.Parameters.AddWithValue("@jsonData", uploadSciExceptionReportDTO.JsonData);

                SqlParameter returnValue = new()
                {
                    ParameterName = "@RETURN_VALUE",
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnValue);

                connection.Open();
                command.ExecuteNonQuery();

                int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                if (returnCode != 1)
                {
                    throw new InvalidOperationException("Something went wrong while uploading the orders,please check the order details.");
                }

                resultDTO.IsSuccess = true;
                resultDTO.Message = "SCI-Trailing Docs-Exception report uploaded successfully";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }


        public ResultDTO GetTatStatus()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var tatstatus = _oMTDataContext.TatStatus.Where(x => x.IsActive).ToList();

                if (tatstatus.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Tat Status fetched successfully";
                    resultDTO.Data = tatstatus;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No details found.";
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

        public ResultDTO GetSciPendingStatusSkillsetsList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var SciPendingStatusSkillsets = (from sps in _oMTDataContext.SciPendingStatusSkillsets
                                                 join ss in _oMTDataContext.SkillSet on sps.SkillSetId equals ss.SkillSetId
                                                 join ts in _oMTDataContext.TatStatus on sps.IsActive equals ts.TatStatus_Value
                                                 select new
                                                 {
                                                     Id = sps.Id,
                                                     Skillsetid = sps.SkillSetId,
                                                     Skillsetname = ss.SkillSetName,
                                                     IsActive = sps.IsActive,
                                                     Status = ts.TatStatus_Name,
                                                     StatusId = ts.Id,
                                                     Scheduled_Time = sps.Scheduled_Time,
                                                     Scheduled_Days = sps.Scheduled_Days,
                                                 }
                                                 ).ToList();

                if (SciPendingStatusSkillsets.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Sci Pending Status Skillsets fetched successfully";
                    resultDTO.Data = SciPendingStatusSkillsets;
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No details found.";
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

        public ResultDTO UpdateTat(UpdateTatDTO updateTatDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var tatstatus = _oMTDataContext.TatStatus.Where(x => x.Id == updateTatDTO.TatStatusId).FirstOrDefault();
                var scipendingskillset = _oMTDataContext.SciPendingStatusSkillsets.Where(x => x.Id == updateTatDTO.SciPendingStatusSkillsetsId).FirstOrDefault();


                if (tatstatus.TatStatus_Value == false)
                {
                    // check if it is already disabled 

                    var tathistory = _oMTDataContext.Tat_History.Where(x => x.SciPendingStatusSkillsetsId == updateTatDTO.SciPendingStatusSkillsetsId && x.DisabledTime != null && x.EnabledTime == null).FirstOrDefault();

                    if (tathistory != null)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "It is already diasbled, you can't disable it again.";
                    }

                    else
                    {

                        scipendingskillset.IsActive = false;

                        _oMTDataContext.SciPendingStatusSkillsets.Update(scipendingskillset);
                        _oMTDataContext.SaveChanges();

                        //capture the user who modified tat and extended time

                        Tat_History tat_History = new Tat_History()
                        {
                            SciPendingStatusSkillsetsId = updateTatDTO.SciPendingStatusSkillsetsId,
                            TatDate = DateTime.UtcNow.Date,
                            DisabledBy = userid,
                            DisabledTime = DateTime.UtcNow,
                        };

                        _oMTDataContext.Tat_History.Add(tat_History);
                        _oMTDataContext.SaveChanges();

                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "TAT has been disabled successfully";
                    }

                }
                else if (tatstatus.TatStatus_Value == true)
                {

                    // check if it is already enabled 

                    var tathistory = _oMTDataContext.Tat_History.Where(x => x.SciPendingStatusSkillsetsId == updateTatDTO.SciPendingStatusSkillsetsId).OrderByDescending(x => x.Tat_HistoryId).FirstOrDefault();


                    if (tathistory != null)
                    {
                        if (tathistory.EnabledTime != null)
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.StatusCode = "404";
                            resultDTO.Message = "It is already enabled, you can't enable it again.";
                        }


                        else
                        {

                            scipendingskillset.IsActive = true;

                            _oMTDataContext.SciPendingStatusSkillsets.Update(scipendingskillset);
                            _oMTDataContext.SaveChanges();

                            // check those skillset tables for un assigned orders

                            var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == scipendingskillset.SkillSetId && x.IsActive).FirstOrDefault();

                            string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                            using SqlConnection connection = new(connectionstring);

                            connection.Open();

                            var checkquery = $@"SELECT COUNT(*) FROM {skillset.SkillSetName} WHERE UserId IS NULL AND Status IS NULL";

                            using SqlCommand countCmd = new()
                            {
                                Connection = connection,
                                CommandType = CommandType.Text,
                                CommandText = checkquery
                            };

                            int ordercount = Convert.ToInt32(countCmd.ExecuteScalar());

                            // insert into Daily_system_pending_Count table

                            string updatedspc = $@"INSERT INTO Daily_system_pending_Count (SystemofRecordId,SkillSetId,Date,Count) VALUES (@SystemofRecordId,@SkillSetId,@Date,@Count)";

                            SqlCommand updateToSPN = new SqlCommand(updatedspc, connection);
                            updateToSPN.CommandType = CommandType.Text;

                            updateToSPN.Parameters.AddWithValue("@SystemofRecordId", skillset.SystemofRecordId);
                            updateToSPN.Parameters.AddWithValue("@SkillSetId", skillset.SkillSetId);
                            updateToSPN.Parameters.AddWithValue("@Date", DateTime.Now.Date);
                            updateToSPN.Parameters.AddWithValue("@Count", ordercount);

                            updateToSPN.ExecuteNonQuery();

                            // move to system pending

                            if (ordercount > 0)
                            {
                                var updatequery = $@"UPDATE {skillset.SkillSetName} SET Status = @statusid, CompletionDate = @CompletionDate WHERE UserId IS NULL AND Status IS NULL";

                                var statusid = _oMTDataContext.ProcessStatus.Where(x => x.SystemOfRecordId == skillset.SystemofRecordId && x.Status == "System-Pending" && x.IsActive).Select(x => x.Id).FirstOrDefault();
                                DateTime dateTime = DateTime.UtcNow;

                                SqlCommand updateToPN = new SqlCommand(updatequery, connection);
                                updateToPN.CommandType = CommandType.Text;

                                updateToPN.Parameters.AddWithValue("@statusid", statusid);
                                updateToPN.Parameters.AddWithValue("@CompletionDate", dateTime);

                                updateToPN.ExecuteNonQuery();
                            }


                            //capture the user who modified tat and extended time

                            var tat_history = _oMTDataContext.Tat_History.Where(x => x.SciPendingStatusSkillsetsId == updateTatDTO.SciPendingStatusSkillsetsId && x.EnabledTime == null && x.DisabledTime != null).FirstOrDefault();

                            if (tat_history != null)
                            {
                                tat_history.EnabledBy = userid;
                                tat_history.EnabledTime = DateTime.UtcNow;

                                _oMTDataContext.Tat_History.Update(tat_history);
                                _oMTDataContext.SaveChanges();

                                resultDTO.IsSuccess = true;
                                resultDTO.Message = "TAT has been enabled successfully";
                            }
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Please disable it once and then try to enable again.";
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
