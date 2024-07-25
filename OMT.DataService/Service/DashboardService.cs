using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService
{
    public class DashboardService : IDashboardService
    {
        private readonly OMTDataContext _oMTDataContext;
        public DashboardService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO LiveStatusReport(LiveStatusReportDTO liveStatusReportDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();

                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId ==  liveStatusReportDTO.SkillSetId).FirstOrDefault();

                List<Dictionary<string, object>> complreport = new List<Dictionary<string, object>>();
                

                if (skillSet != null)
                {

                    if (liveStatusReportDTO.StartTime == null && liveStatusReportDTO.EndTime == null)
                    {
                        DateTime endDate = DateTime.Today;
                        DateTime startDate = DateTime.Today.AddDays(-1);

                        var starttime = _oMTDataContext.LiveReportTiming.Where(x => x.SystemOfRecordId == liveStatusReportDTO.SystemOfRecordId).Select(_ => _.StartTime).FirstOrDefault();
                        var endtime = _oMTDataContext.LiveReportTiming.Where(x => x.SystemOfRecordId == liveStatusReportDTO.SystemOfRecordId).Select(_ => _.EndTime).FirstOrDefault();

                        DateTime startDateTime = startDate.Add(starttime);
                        DateTime endDateTime = endDate.Add(endtime);

                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetLiveReportBySkillset"
                        };
                        command.Parameters.AddWithValue("@SkillSetName", skillSet.SkillSetName);
                        command.Parameters.AddWithValue("@STARTDateTime", startDateTime);
                        command.Parameters.AddWithValue("@EndDateTime", endDateTime);

                        SqlParameter returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);
                        command.ExecuteNonQuery();

                        int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while fetching the orders.");
                        }

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = command;

                        DataSet dataset = new DataSet();
                        
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        var querydt1 = datatable.AsEnumerable()
                                     .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                         column => column.ColumnName,
                                         column => row[column])).ToList();

                        complreport.AddRange(querydt1);

                        // get list of statusid for sorid

                        var query1 = (from ss in _oMTDataContext.SystemofRecord
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SystemofRecordId == liveStatusReportDTO.SystemOfRecordId
                                      select new
                                      {
                                          StatusId = ps.Id,
                                          Status = ps.Status,

                                      }).ToList();

                        List<StatusCountDTO> statusCounts = new List<StatusCountDTO>();

                        // get order counts for each status

                        foreach (var id in query1)
                        {
                            var filteredRows = datatable.AsEnumerable()
                                       .Where(row => row.Field<int>("statusid") == id.StatusId);

                            if (filteredRows.Any())
                            {
                                StatusCountDTO status = new StatusCountDTO
                                {
                                    StatusName = filteredRows.First().Field<string>("Status"),
                                    TotalCount = filteredRows.Count()
                                };

                                statusCounts.Add(status);
                            }
                        }

                        string que = $@"
                                      SELECT COUNT(t.OrderId) 
                                      FROM {skillSet.SkillSetName} t
                                      WHERE t.UserId IS NULL 
                                      AND CONVERT(DATE, CompletionDate) BETWEEN @StartDateTime AND @EndDateTime";

                        using (SqlCommand command2 = new SqlCommand(que, connection))
                        {
                            command2.Parameters.AddWithValue("@StartDateTime", startDateTime);
                            command2.Parameters.AddWithValue("@EndDateTime", endDateTime);

                            int totalCount = (int)command2.ExecuteScalar();

                            StatusCountDTO totalStatus = new StatusCountDTO
                            {
                                StatusName = "Not Assigned",
                                TotalCount = totalCount
                            };

                            statusCounts.Add(totalStatus);
                        }

                        // get list of distinct users to get their order counts

                        List<int> distinctUsers = datatable.AsEnumerable()
                                                  .Select(row => row.Field<int>("UserId"))
                                                  .Distinct()
                                                  .ToList();

                        List<AgentCompletionCountDTO> agentcompCounts = new List<AgentCompletionCountDTO>();

                        foreach (var user in distinctUsers)
                        {
                            var filteredRows = datatable.AsEnumerable()
                                       .Where(row => row.Field<int>("UserId") == user);

                            if (filteredRows.Any())
                            {
                                AgentCompletionCountDTO agentCompletionCountDTO = new AgentCompletionCountDTO
                                {
                                    UserName = filteredRows.First().Field<string>("UserName"),
                                    TotalCount = filteredRows.Count(),
                                };

                                agentcompCounts.Add(agentCompletionCountDTO);
                            }
                        }


                        LiveStatusReportResponseDTO liveStatusReportResponseDTO = new LiveStatusReportResponseDTO
                        {
                            StatusCount = statusCounts,
                            AgentCompletedOrdersCount = agentcompCounts,
                            CompletionReport = complreport

                        };

                        resultDTO.IsSuccess = true;
                        resultDTO.Data = liveStatusReportResponseDTO;
                        resultDTO.Message = "Live reports fetched successfully";

                    }
                    else
                    {
                        //using SqlCommand command = new()
                        //{
                        //    Connection = connection,
                        //    CommandType = CommandType.StoredProcedure,
                        //    CommandText = "GetLiveReportBySkillset"
                        //};
                        //command.Parameters.AddWithValue("@SkillSet", skillSet.SkillSetName);
                        //command.Parameters.AddWithValue("@STARTDateTime", liveStatusReportDTO.StartTime);
                        //command.Parameters.AddWithValue("@EndDateTime", liveStatusReportDTO.EndTime);

                        //SqlParameter returnValue = new()
                        //{
                        //    ParameterName = "@RETURN_VALUE",
                        //    Direction = ParameterDirection.ReturnValue
                        //};
                        //command.Parameters.Add(returnValue);


                        //int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                        //if (returnCode != 1)
                        //{
                        //    throw new InvalidOperationException("Something went wrong while fetching the completion report.");
                        //}
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
