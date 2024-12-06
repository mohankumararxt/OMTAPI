using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DataService.Settings;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OMT.DataService.Service
{
    public class DashboardSkillsetReportService:IDashboardSkillsetReportService
    {
        private readonly OMTDataContext _oMTDataContext;

        private readonly IOptions<TrdStatusSettings> _authSettings;
        private readonly IOptions<EmailDetailsSettings> _emailDetailsSettings;
        private readonly IConfiguration _configuration;
        public DashboardSkillsetReportService(OMTDataContext oMTDataContext, IOptions<TrdStatusSettings> authSettings, IOptions<EmailDetailsSettings> emailDetailsSettings, IConfiguration configuration)
        {
            _oMTDataContext = oMTDataContext;
            _authSettings = authSettings;
            _emailDetailsSettings = emailDetailsSettings;
            _configuration = configuration;
        }


        public ResultDTO DashboardReports(DateTime fromDate, DateTime toDate)
        {
            // Initialize the result object
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            int completecount = 0;
            try
            {
                // Get the database connection string
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                if (string.IsNullOrEmpty(connectionstring))
                {
                    throw new InvalidOperationException("Database connection string is not configured.");
                }

                using (var connection = new SqlConnection(connectionstring))
                {
                    // Open the connection
                    connection.Open();

                    // Prepare the stored procedure command
                    string query = "GetSkillSetData";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 180;

                        // Add parameters
                        command.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date) { Value = fromDate });
                        command.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.Date) { Value = toDate });

                        // Execute and load data into a DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Map data to a list of custom objects
                            List<SkillSetDataDTO> skillSetDataList = new List<SkillSetDataDTO>();
                            string lastSkillSetName = null; // Initialize to track the previous skill name
                            int ordercount = 0; // Initialize order count


                            foreach (DataRow row in dataTable.Rows)
                            {
                                SkillSetDataDTO data = new SkillSetDataDTO
                                {
                                    SystemOfRecordName = row["SystemOfRecordName"].ToString(),
                                    SkillSetName = row["SkillSetName"].ToString(),
                                    Completed_Count = Convert.ToInt32(row["Completed_Count"]),
                                    Pending_Count = Convert.ToInt32(row["Pending_Count"]),
                                    Reject_Count = Convert.ToInt32(row["Reject_Count"]),
                                    Threshold = Convert.ToInt32(row["Threshold"]),
                                    Total_WorkingDays = Convert.ToInt32(row["WorkingDays"]),
                                };

                                skillSetDataList.Add(data);
                            }


                            // Store the list in the result object
                            resultDTO.Data = skillSetDataList;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Handle SQL-specific errors
                resultDTO.IsSuccess = false;
                resultDTO.Message = $"Database error: {sqlEx.Message}";
                resultDTO.StatusCode = "500";
            }
            catch (Exception ex)
            {
                // Handle all other errors
                resultDTO.IsSuccess = false;
                resultDTO.Message = $"An unexpected error occurred: {ex.Message}";
                resultDTO.StatusCode = "500";
            }

            return resultDTO;
        }







        //public ResultDTO DashboardReports(DateTime fromDate, DateTime toDate)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

        //        // Dictionaries to store results
        //        var systemOfRecordData = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        //        var systemOfRecordData1 = new Dictionary<string, Dictionary<string, int>>();

        //        List<string> reportcol = new List<string>();

        //        var sorlist = _oMTDataContext.SystemofRecord.ToList();

        //        foreach (var sor in sorlist)
        //        {
        //            var skillSetNames = new List<string>();
        //            using (var connection = new SqlConnection(connectionstring))
        //            {
        //                connection.Open();
        //                var command = new SqlCommand(
        //                    "SELECT SkillSetName FROM SkillSet WHERE SystemofRecordId = @SystemofRecordId AND IsActive = 1",
        //                    connection);
        //                command.Parameters.AddWithValue("@SystemofRecordId", sor.SystemofRecordId);
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        skillSetNames.Add(reader["SkillSetName"].ToString());
        //                    }
        //                }
        //            }

        //            if (sor.SystemofRecordId != 3)
        //            {
        //                foreach (var skillsetName in skillSetNames)
        //                {
        //                    string tableName = skillsetName;

        //                    using (var connection = new SqlConnection(connectionstring))
        //                    {
        //                        connection.Open();
        //                        try
        //                        {
        //                            string query = $@"
        //SELECT so.SystemofRecordName, ss.SkillSetName, ps.Status, COUNT(ps.Status) AS Count, ss.Threshold
        //FROM SystemofRecord so
        //JOIN SkillSet ss ON so.SystemofRecordId = ss.SystemofRecordId
        //JOIN ProcessStatus ps ON so.SystemofRecordId = ps.SystemOfRecordId AND ss.SystemofRecordId = ps.SystemOfRecordId
        //JOIN {tableName} sst ON ps.Id = sst.Status AND ps.SystemofRecordId = sst.SystemOfRecordId AND ss.SkillSetId = sst.SkillSetId
        //WHERE sst.StartTime <= sst.EndTime AND so.SystemofRecordId = {sor.SystemofRecordId}
        //GROUP BY so.SystemofRecordName, ss.SkillSetName, ps.Status, ss.Threshold";

        //                            using (var command = new SqlCommand(query, connection))
        //                            {
        //                                using (var reader = command.ExecuteReader())
        //                                {
        //                                    if (reader.HasRows)
        //                                    {
        //                                        while (reader.Read())
        //                                        {
        //                                            string systemOfRecordName = reader["SystemofRecordName"].ToString();
        //                                            int threshold = Convert.ToInt32(reader["Threshold"]);
        //                                            string status = reader["Status"].ToString();
        //                                            int count = Convert.ToInt32(reader["Count"]);
        //                                            int resourceWorked = 2;

        //                                            float capacityPerDay = MathF.Round(threshold * resourceWorked, 2);

        //                                            int totalDaysExcludingSundays = 0;
        //                                            for (DateTime date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        //                                            {
        //                                                if (date.DayOfWeek != DayOfWeek.Sunday)
        //                                                {
        //                                                    totalDaysExcludingSundays++;
        //                                                }
        //                                            }

        //                                            int capacityPerTotalWorkingDays = (int)(capacityPerDay * totalDaysExcludingSundays);

        //                                            // Combine data into systemOfRecordData dictionary
        //                                            if (!systemOfRecordData.ContainsKey(systemOfRecordName))
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName] = new Dictionary<string, Dictionary<string, int>>();
        //                                            }

        //                                            string skillsetKey = $"{skillsetName.ToLower()}";
        //                                            if (!systemOfRecordData[systemOfRecordName].ContainsKey(skillsetKey))
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey] = new Dictionary<string, int>()
        //                                                {
        //                                                    { "completed", 0 },
        //                                                    { "pending", 0 },
        //                                                    { "reject", 0 },
        //                                                    { "ordersreceived", 0 },
        //                                                    { "Threshold", 0 },
        //                                                    { "resource_worked", 0 },
        //                                                    { "capacityperday", 0 },
        //                                                    { "capacitypertotalworkingdays", 0 },
        //                                                    { "shortfall", 0 },
        //                                                    { "utilization", 0 }
        //                                                };
        //                                            }

        //                                            // Increment the counts for the current status
        //                                            if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["completed"] += count;
        //                                            }
        //                                            else if (status.Equals("exception", StringComparison.OrdinalIgnoreCase) && sor.SystemofRecordId == 1)
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["completed"] += count;
        //                                            }
        //                                            else if (status.Equals("completed-nofind", StringComparison.OrdinalIgnoreCase) && sor.SystemofRecordId == 2)
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["completed"] += count;
        //                                            }
        //                                            else if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["pending"] += count;
        //                                            }
        //                                            else if (status.Equals("reject", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["reject"] += count;
        //                                            }

        //                                            int completed = systemOfRecordData[systemOfRecordName][skillsetKey]["completed"];
        //                                            int pending = systemOfRecordData[systemOfRecordName][skillsetKey]["pending"];
        //                                            int reject = systemOfRecordData[systemOfRecordName][skillsetKey]["reject"];

        //                                            // Calculate orders received
        //                                            systemOfRecordData[systemOfRecordName][skillsetKey]["ordersreceived"] = completed + pending + reject;
        //                                            systemOfRecordData[systemOfRecordName][skillsetKey]["Threshold"] = threshold;

        //                                            // Calculate the shortfall and utilization
        //                                            int shortfall = completed - capacityPerTotalWorkingDays;

        //                                            try
        //                                            {
        //                                                float utilization = (completed > 0 && capacityPerTotalWorkingDays > 0)
        //                                                    ? MathF.Round((float)completed / capacityPerTotalWorkingDays * 100) // Multiply by 100 after division
        //                                                    : 0;

        //                                                // Combine the values
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["resource_worked"] += resourceWorked;
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["capacityperday"] += (int)capacityPerDay;
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["capacitypertotalworkingdays"] += capacityPerTotalWorkingDays;
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["shortfall"] += shortfall;
        //                                                systemOfRecordData[systemOfRecordName][skillsetKey]["utilization"] += (int)utilization;
        //                                            }
        //                                            catch (Exception ex)
        //                                            {
        //                                                resultDTO.IsSuccess = false;
        //                                                resultDTO.Message = ex.Message;
        //                                                resultDTO.StatusCode = "500";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        resultDTO.IsSuccess = false;
        //                                        resultDTO.Message = "No data available.";
        //                                        continue;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            resultDTO.IsSuccess = false;
        //                            resultDTO.Message = ex.Message;
        //                            resultDTO.StatusCode = "500";
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                int loopCount = 0;
        //                foreach (var skillsetName in skillSetNames)
        //                {
        //                    string tableName = skillsetName;
        //                    using (var connection = new SqlConnection(connectionstring))
        //                    {
        //                        connection.Open();

        //                        try
        //                        {
        //                            string query = $@"
        //SELECT so.SystemofRecordName, ss.SkillSetName, ps.Status, COUNT(ps.Status) AS Count, ss.Threshold
        //FROM SystemofRecord so
        //JOIN SkillSet ss ON so.SystemofRecordId = ss.SystemofRecordId
        //JOIN ProcessStatus ps ON so.SystemofRecordId = ps.SystemOfRecordId AND ss.SystemofRecordId = ps.SystemOfRecordId
        //JOIN {tableName} sst ON ps.Id = sst.Status AND ps.SystemofRecordId = sst.SystemOfRecordId AND ss.SkillSetId = sst.SkillSetId
        //WHERE sst.StartTime <= sst.EndTime AND so.SystemofRecordId = {sor.SystemofRecordId}
        //GROUP BY so.SystemofRecordName, ss.SkillSetName, ps.Status, ss.Threshold";

        //                            using (var command = new SqlCommand(query, connection))
        //                            {
        //                                using (var reader = command.ExecuteReader())
        //                                {
        //                                    if (!reader.HasRows)
        //                                    {
        //                                        resultDTO.IsSuccess = false;
        //                                        resultDTO.Message = "No data available.";
        //                                        continue;
        //                                    }

        //                                    while (reader.Read())
        //                                    {
        //                                        string systemOfRecordName = reader["SystemofRecordName"].ToString();
        //                                        int threshold = Convert.ToInt32(reader["Threshold"]);
        //                                        string status = reader["Status"].ToString();
        //                                        int count = Convert.ToInt32(reader["Count"]);
        //                                        int resourceWorked = 2;

        //                                        float capacityPerDay = MathF.Round(threshold * resourceWorked, 2);

        //                                        int totalDaysExcludingSundays = 0;
        //                                        for (DateTime date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        //                                        {
        //                                            if (date.DayOfWeek != DayOfWeek.Sunday)
        //                                            {
        //                                                totalDaysExcludingSundays++;
        //                                            }
        //                                        }

        //                                        int capacityPerTotalWorkingDays = (int)(capacityPerDay * totalDaysExcludingSundays);

        //                                        // Combine data into systemOfRecordData1 dictionary
        //                                        if (!systemOfRecordData1.ContainsKey(systemOfRecordName))
        //                                        {
        //                                            systemOfRecordData1[systemOfRecordName] = new Dictionary<string, int>()
        //                                    {
        //                                        { "completed", 0 },
        //                                        { "pending", 0 },
        //                                        { "reject", 0 },
        //                                        { "ordersreceived", 0 },
        //                                        { "Threshold", 0 },
        //                                        { "resource_worked", 0 },
        //                                        { "capacityperday", 0 },
        //                                        { "capacitypertotalworkingdays", 0 },
        //                                        { "shortfall", 0 },
        //                                        { "utilization", 0 }
        //                                    };
        //                                        }

        //                                        // Increment the counts for the current status
        //                                        if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            systemOfRecordData1[systemOfRecordName]["completed"] += count;
        //                                        }
        //                                        if (status.Equals("completed-manual", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            systemOfRecordData1[systemOfRecordName]["completed"] += count;
        //                                        }
        //                                        else if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            systemOfRecordData1[systemOfRecordName]["pending"] += count;
        //                                        }
        //                                        else if (status.Equals("reject", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            systemOfRecordData1[systemOfRecordName]["reject"] += count;
        //                                        }

        //                                        int completed = systemOfRecordData1[systemOfRecordName]["completed"];
        //                                        int pending = systemOfRecordData1[systemOfRecordName]["pending"];
        //                                        int reject = systemOfRecordData1[systemOfRecordName]["reject"];

        //                                        // Calculate orders received
        //                                        systemOfRecordData1[systemOfRecordName]["ordersreceived"] = completed + pending + reject;
        //                                        systemOfRecordData1[systemOfRecordName]["Threshold"] = threshold;

        //                                        // Calculate the shortfall and utilization
        //                                        int shortfall = completed - capacityPerTotalWorkingDays;

        //                                        try
        //                                        {
        //                                            float utilization = (completed > 0 && capacityPerTotalWorkingDays > 0)
        //                                                    ? MathF.Round((float)completed / capacityPerTotalWorkingDays * 100) // Multiply by 100 after division
        //                                                    : 0;

        //                                            // Combine the values
        //                                            systemOfRecordData1[systemOfRecordName]["resource_worked"] += resourceWorked;
        //                                            systemOfRecordData1[systemOfRecordName]["capacityperday"] += (int)capacityPerDay;
        //                                            systemOfRecordData1[systemOfRecordName]["capacitypertotalworkingdays"] += capacityPerTotalWorkingDays;
        //                                            systemOfRecordData1[systemOfRecordName]["shortfall"] += shortfall;
        //                                            systemOfRecordData1[systemOfRecordName]["utilization"] +=(int)utilization;
        //                                        }
        //                                        catch (Exception ex)
        //                                        {
        //                                            resultDTO.IsSuccess = false;
        //                                            resultDTO.Message = ex.Message;
        //                                            resultDTO.StatusCode = "500";
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            resultDTO.IsSuccess = false;
        //                            resultDTO.Message = ex.Message;
        //                            resultDTO.StatusCode = "500";
        //                        }
        //                    }
        //                }
        //            }


        //        }
        //        var combinedDict = MergeDictionaries(systemOfRecordData, systemOfRecordData1);

        //        resultDTO.Data = combinedDict;
        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.Message = ex.Message;
        //        resultDTO.StatusCode = "500";
        //    }
        //    return resultDTO;
        //}


        // Helper function to merge both dictionaries
        //private Dictionary<string, Dictionary<string, Dictionary<string, int>>> MergeDictionaries(
        //    Dictionary<string, Dictionary<string, Dictionary<string, int>>> dict1,
        //    Dictionary<string, Dictionary<string, int>> dict2)
        //{
        //    var mergedDict = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        //    // Merge dict1 into mergedDict
        //    foreach (var entry in dict1)
        //    {
        //        if (!mergedDict.ContainsKey(entry.Key))
        //        {
        //            mergedDict[entry.Key] = new Dictionary<string, Dictionary<string, int>>();
        //        }
        //        foreach (var subEntry in entry.Value)
        //        {
        //            mergedDict[entry.Key][subEntry.Key] = new Dictionary<string, int>(subEntry.Value);
        //        }
        //    }

        //    // Merge dict2 into mergedDict (dict2 doesn't have the same nested structure)
        //    foreach (var entry in dict2)
        //    {
        //        if (!mergedDict.ContainsKey(entry.Key))
        //        {
        //            mergedDict[entry.Key] = new Dictionary<string, Dictionary<string, int>>();
        //        }
        //        mergedDict[entry.Key]["general"] = new Dictionary<string, int>(entry.Value);
        //    }

        //    return mergedDict;
        //}


    }
}
