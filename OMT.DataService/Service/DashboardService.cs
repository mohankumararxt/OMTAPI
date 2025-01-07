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

                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == liveStatusReportDTO.SkillSetId).FirstOrDefault();

                List<Dictionary<string, object>> notassignedreport = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> complreport = new List<Dictionary<string, object>>();
                List<StatusCountDTO> statusCounts = new List<StatusCountDTO>();
                List<AgentCompletionCountDTO> agentcompCounts = new List<AgentCompletionCountDTO>();

                DateTime startDateTime = DateTime.UtcNow;
                DateTime endDateTime = DateTime.UtcNow;

                if (skillSet != null)
                {
                    var templatePresent = _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == liveStatusReportDTO.SkillSetId);

                    if (templatePresent)
                    {

                        if (liveStatusReportDTO.StartTime == null && liveStatusReportDTO.EndTime == null)
                        {
                            DateTime endDate = DateTime.Today;
                            DateTime startDate = DateTime.Today.AddDays(-1);

                            var starttime = _oMTDataContext.LiveReportTiming.Where(x => x.SystemOfRecordId == liveStatusReportDTO.SystemOfRecordId).Select(_ => _.StartTime).FirstOrDefault();
                            var endtime = _oMTDataContext.LiveReportTiming.Where(x => x.SystemOfRecordId == liveStatusReportDTO.SystemOfRecordId).Select(_ => _.EndTime).FirstOrDefault();

                            startDateTime = startDate.Add(starttime);
                            endDateTime = endDate.Add(endtime);

                        }
                        else
                        {
                            startDateTime = Convert.ToDateTime(liveStatusReportDTO.StartTime);
                            endDateTime = Convert.ToDateTime(liveStatusReportDTO.EndTime);
                        }

                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetLiveReportBySkillset"
                        };
                        command.Parameters.AddWithValue("@SkillSetId", skillSet.SkillSetId);
                        command.Parameters.AddWithValue("@SkillSetName", skillSet.SkillSetName);
                        command.Parameters.AddWithValue("@STARTDateTime", startDateTime);
                        command.Parameters.AddWithValue("@EndDateTime", endDateTime);
                        command.Parameters.AddWithValue("@SystemOfRecordId", liveStatusReportDTO.SystemOfRecordId);

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

                        // get unassigned order counts

                        if (dataset.Tables.Count > 1)
                        {
                            int unasssignedcount = Convert.ToInt32(dataset.Tables[1].Rows.Count);     //get the no of rows

                            StatusCountDTO totalStatus = new StatusCountDTO
                            {
                                StatusName = "Not Assigned",
                                TotalCount = unasssignedcount
                            };

                            statusCounts.Add(totalStatus);

                            DataTable unasgnorderDT = dataset.Tables[1];

                            var listofunassignedorders = unasgnorderDT.AsEnumerable()
                                                                      .Select(row => unasgnorderDT.Columns.Cast<DataColumn>()
                                                                      .ToDictionary(
                                                                          column => column.ColumnName,
                                                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                            notassignedreport.AddRange(listofunassignedorders);
                        }

                        if (dataset.Tables.Count > 0)
                        {
                            DataTable datatable = dataset.Tables[0];

                            var listofcomporders = datatable.AsEnumerable()
                                                             .Where(row => row["status"].ToString() != "Complex")
                                                             .Select(row => datatable.Columns.Cast<DataColumn>()
                                                                 .ToDictionary(
                                                                     column => column.ColumnName,
                                                                     column => {
                                                                         var value = row[column];
                                                                         if (value == DBNull.Value)
                                                                         {
                                                                             return "";
                                                                         }

                                                                         if (column.DataType == typeof(DateTime))
                                                                         {
                                                                             DateTime dateValue = (DateTime)value;
                                                                             return dateValue.TimeOfDay == TimeSpan.Zero
                                                                                 ? (object)dateValue.ToString("yyyy-MM-dd")
                                                                                 : (object)dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                                                                         }

                                                                         return (object)value.ToString();
                                                                     }))
                                                             .ToList();

                            List<string> coltoremove = new List<string> { "UserId", "statusid" };

                            foreach (var dictionary in listofcomporders)
                            {
                                foreach (var key in coltoremove)
                                {
                                    dictionary.Remove(key);
                                }
                                complreport.Add(dictionary);
                            }

                            // get count of priority orders

                            var filteredPriorityRows = datatable.AsEnumerable().Where(row => row.Field<bool>("IsPriority") == true);

                            StatusCountDTO statusdt = new StatusCountDTO
                            {
                                StatusName = "Rush Order",
                                TotalCount = filteredPriorityRows.Count()
                            };

                            statusCounts.Add(statusdt);

                            // get list of statusid for sorid

                            var ListofStatus = (from ss in _oMTDataContext.SystemofRecord
                                                join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                                where ss.SystemofRecordId == liveStatusReportDTO.SystemOfRecordId && ps.IsActive
                                                select new
                                                {
                                                    StatusId = ps.Id,
                                                    Status = ps.Status,

                                                }).ToList();

                            ListofStatus.RemoveAll(item => item.Status == "System-Pending");

                            // get order counts for each status

                            foreach (var status in ListofStatus)
                            {
                                var filteredRows = datatable.AsEnumerable().Where(row => row.Field<int>("statusid") == status.StatusId);

                                if (filteredRows.Any())
                                {
                                    StatusCountDTO statusdto = new StatusCountDTO
                                    {
                                        StatusName = status.Status,
                                        TotalCount = filteredRows.Count()
                                    };

                                    statusCounts.Add(statusdto);
                                }
                                else
                                {
                                    StatusCountDTO statusdto = new StatusCountDTO
                                    {
                                        StatusName = status.Status,
                                        TotalCount = filteredRows.Count()
                                    };

                                    statusCounts.Add(statusdto);
                                }
                            }

                            // get list of distinct users to get their order counts

                            List<int> distinctUsers = datatable.AsEnumerable()
                                                      .Select(row => row.Field<int>("UserId"))
                                                      .Distinct()
                                                      .ToList();

                            foreach (var user in distinctUsers)
                            {
                                var filteredRows = datatable.AsEnumerable()
                                             .Where(row => row["status"].ToString().ToLower().Trim() != "complex" &&
                                              row.Field<int>("UserId") == user);

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
                                CompletionReport = complreport,
                                NotAssignedReport = notassignedreport

                            };

                            resultDTO.IsSuccess = true;
                            resultDTO.Data = liveStatusReportResponseDTO;
                            resultDTO.Message = "Live reports fetched successfully";
                        }

                    }
                    else
                    {
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = $"Sorry, the template '{skillSet.SkillSetName}' doesnt exist.";
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

        public ResultDTO AgentCompletionCount(AgentDashDTO agentDashDTO,int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                           join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                           where us.UserId == userid && us.IsActive
                                           && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                           select ss.SkillSetName).Distinct().ToList();

                Dictionary<string, List<string>> statuses = new Dictionary<string, List<string>>();

                foreach (string tableName in tablenames)
                {
                    var skillset = _oMTDataContext.SkillSet.Where(x =>x.SkillSetName == tableName && x.IsActive).FirstOrDefault();

                    var statusnames = _oMTDataContext.ProcessStatus.Where(x => x.SystemOfRecordId == skillset.SystemofRecordId && x.IsActive).Select(_ => _.Status).ToList();

                    statusnames.Remove("System-Pending");
                    statuses[skillset.SkillSetName] = statusnames;
                }

                DateTime startDateTime = DateTime.UtcNow;
                DateTime endDateTime = DateTime.UtcNow;

                startDateTime = Convert.ToDateTime(agentDashDTO.StartTime);
                endDateTime = Convert.ToDateTime(agentDashDTO.EndTime);

                using SqlCommand command = new()
                {
                    Connection = connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "GetAgentCompletionCount"
                };

                command.Parameters.AddWithValue("@SkillSetNames", string.Join(",", tablenames));
                command.Parameters.AddWithValue("@FromDate", startDateTime);
                command.Parameters.AddWithValue("@ToDate", endDateTime);
                command.Parameters.AddWithValue("@UserId", userid);

                SqlParameter returnValue = new SqlParameter
                {
                    ParameterName = "@RETURN_VALUE",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnValue);

                command.ExecuteNonQuery();

                using SqlDataAdapter dataAdapter_acc = new SqlDataAdapter(command);
                DataTable resultTable = new DataTable();

                dataAdapter_acc.Fill(resultTable);

                // Create resultList using the statuses dictionary
                var resultList = resultTable.AsEnumerable()
                    .Select(row =>
                    {
                        // Extract the SkillSetName from the current row
                        string skillSetName = row["SkillSet"]?.ToString();

                        // Normalize the column names for comparison (replace underscores and hyphens with spaces)
                        var normalizedStatuses = statuses
                            .ToDictionary(
                                kvp => kvp.Key.ToLowerInvariant(),
                                kvp => kvp.Value.Select(status => status.Replace("_", " ").Replace("-", " ").ToLowerInvariant()).ToList()
                            );

                        // Check if the SkillSetName exists in the normalized dictionary
                        if (!string.IsNullOrEmpty(skillSetName) && normalizedStatuses.ContainsKey(skillSetName.ToLowerInvariant()))
                        {
                            // Get the valid statuses for the current skill set
                            var validStatuses = normalizedStatuses[skillSetName.ToLowerInvariant()];

                            // Transform the row into a dictionary
                            return resultTable.Columns.Cast<DataColumn>()
                                .ToDictionary(
                                    column => column.ColumnName,
                                    column =>
                                    {
                                        // Normalize the column name (replace underscores and hyphens with spaces)
                                        string normalizedColumnName = column.ColumnName.Replace("_", " ").Replace("-", " ").ToLowerInvariant();

                                        // If the column is a valid status, keep the count or 0
                                        if (validStatuses.Contains(normalizedColumnName))
                                        {
                                            return row[column] == DBNull.Value ? 0 : row[column]; // Keep count or 0 for valid statuses
                                        }
                                        else
                                        {
                                            // Set all other status columns to null, except for SystemOfRecord and SkillSet
                                            return (column.ColumnName == "SystemOfRecord" || column.ColumnName == "SkillSet")
                                                ? row[column] // Retain these columns
                                                : "";       
                                        }
                                    });
                        }
                        else
                        {
                            // If the SkillSetName is not found, set all status columns to null, except for SystemOfRecord and SkillSet
                            return resultTable.Columns.Cast<DataColumn>()
                                .ToDictionary(
                                    column => column.ColumnName,
                                    column =>
                                        (column.ColumnName == "SystemOfRecord" || column.ColumnName == "SkillSet")
                                            ? row[column]  // Retain values for these columns
                                            : null         // Set null for all other status columns
                                );
                        }
                    })
                    .ToList();


                // Check the return value to ensure success
                int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                if (returnCode != 1)
                {
                    throw new InvalidOperationException("Something went wrong while fetching the completion data.");
                }

                if (resultList.Count > 0)
                {
                    resultDTO.Data = resultList;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Live reports fetched successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = $"No details found.";
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
