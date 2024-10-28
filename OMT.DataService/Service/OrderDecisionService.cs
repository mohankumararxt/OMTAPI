using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OMT.DataService.Service
{
    public class OrderDecisionService : IOrderDecisionService
    {
        private readonly OMTDataContext _oMTDataContext;

        public OrderDecisionService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetOrderForUser(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                //check if user has uncompleted orders in all of his skillsets. if any is there- dont assign orders,say- first complete pending orders
                List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                           join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                           where us.UserId == userid && us.IsActive
                                           && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                           select ss.SkillSetName).ToList();

                List<Dictionary<string, object>> noStatusRecords = new List<Dictionary<string, object>>();

                foreach (string tablename in tablenames)
                {
                    // check for pending orders and send them 
                    var columns1 = (from ss in _oMTDataContext.SkillSet
                                    join dt in _oMTDataContext.TemplateColumns on ss.SkillSetId equals dt.SkillSetId
                                    where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                    select dt.ColumnAliasName).ToList();

                    var columns2 = (from ss in _oMTDataContext.SkillSet
                                    join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                    where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                    select dt.DefaultColumnName).ToList();


                    // get date type columns 
                    var datecol = (from ss in _oMTDataContext.SkillSet
                                   join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                   where ss.SkillSetName == tablename && dt.IsGetOrderColumn && dt.DataType == "Date"
                                   select dt.DefaultColumnName).ToList();


                    var columns = (columns1 ?? Enumerable.Empty<string>()).Concat(columns2 ?? Enumerable.Empty<string>());
                    string selectedColumns = string.Join(", ", columns.Select(c => $"t1.{c}"));

                    string query = $@"
                                    SELECT 
                                        {selectedColumns},
                                        t2.SkillSetName AS SkillSetName, 
                                        t1.SkillSetId,
                                        t3.SystemOfRecordName AS SystemOfRecordName,
                                        t1.SystemOfRecordId,
                                        t1.Id,
                                        t1.StartTime
                                    FROM 
                                        [{tablename}] AS t1
                                    LEFT JOIN SkillSet AS t2 ON t1.SkillSetId = t2.SkillSetId
                                    LEFT JOIN SystemOfRecord AS t3 ON t1.SystemofRecordId = t3.SystemOfRecordId
                                    WHERE 
                                        UserId = @UserId 
                                        AND (Status IS NULL OR Status = '')
                                    ORDER BY 
	                                    t1.IsPriority DESC,t1.StartTime ASC;";


                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userid);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var querydt1 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                  column => column.ColumnName,
                                  column =>
                                  {
                                      if (datecol.Contains(column.ColumnName) && column.DataType == typeof(DateTime))
                                      {
                                          if (row[column] == DBNull.Value)
                                          {
                                              return "";
                                          }
                                          DateTime dateValue = (DateTime)row[column];
                                          return dateValue.ToString("yyyy-MM-dd");  // Format as date string
                                      }
                                      return row[column] == DBNull.Value ? "" : row[column];
                                  })).ToList();

                    noStatusRecords.AddRange(querydt1);

                }

                if (noStatusRecords.Count > 0)
                {
                    var orderedRecords = noStatusRecords
                         .OrderByDescending(record => bool.Parse(record["IsPriority"].ToString()))
                         .ThenBy(record => DateTime.Parse(record["StartTime"].ToString()))
                         .First();

                    orderedRecords.Remove("StartTime");

                    var dataToReturn = new List<Dictionary<string, object>> { orderedRecords };

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "You have been assigned with an order by your TL,please finish this first";
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = JsonConvert.SerializeObject(dataToReturn);
                }
                else
                {
                    List<GetOrderCalculation> userskillsetlist = (from uss in _oMTDataContext.UserSkillSet
                                                                  join goc in _oMTDataContext.GetOrderCalculation on uss.UserSkillSetId equals goc.UserSkillSetId
                                                                  join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                  join up in _oMTDataContext.UserProfile on uss.UserId equals up.UserId
                                                                  where uss.IsActive && goc.IsActive && ss.IsActive && up.IsActive && goc.UserId == userid
                                                                  orderby goc.PriorityOrder
                                                                  select new GetOrderCalculation
                                                                  {
                                                                      UserId = goc.UserId,
                                                                      UserSkillSetId = goc.UserSkillSetId,
                                                                      SkillSetId = goc.SkillSetId,
                                                                      TotalOrderstoComplete = goc.TotalOrderstoComplete,
                                                                      OrdersCompleted = goc.OrdersCompleted,
                                                                      Weightage = goc.Weightage,
                                                                      PriorityOrder = goc.PriorityOrder,
                                                                      IsActive = goc.IsActive,
                                                                      UpdatedDate = goc.UpdatedDate,
                                                                      IsCycle1 = goc.IsCycle1,
                                                                      IsHardStateUser = goc.IsHardStateUser,
                                                                      Utilized = goc.Utilized,
                                                                      HardStateUtilized = goc.HardStateUtilized,
                                                                  }).ToList();

                    bool iscycle1 = true;

                    string uporder = string.Empty;

                    List<GetOrderCalculation> uss_cycle1 = userskillsetlist.Where(x => x.Utilized == false && x.IsCycle1).OrderBy(x => x.PriorityOrder).ToList();

                    if (uss_cycle1.Count == 0)
                    {
                        iscycle1 = false;

                        // Get unutilized skill sets for cycle 2, ordered by priority
                        var uss_cycle2 = userskillsetlist
                            .Where(x => !x.Utilized && !x.IsCycle1)
                            .OrderBy(x => x.PriorityOrder)
                            .ToList();

                        // Process cycle 2 skill sets
                        uporder = GetOrderByCycle(uss_cycle2, iscycle1, userid, resultDTO, connection);

                    }
                    else
                    {
                        uporder = GetOrderByCycle(uss_cycle1, iscycle1, userid, resultDTO, connection);

                        // If no orders were assigned in cycle 1, proceed to cycle 2
                        if (string.IsNullOrWhiteSpace(uporder))
                        {
                            iscycle1 = false;

                            // Get unutilized skill sets for cycle 2, ordered by priority
                            var uss_cycle2 = userskillsetlist
                                .Where(x => !x.Utilized && !x.IsCycle1)
                                .OrderBy(x => x.PriorityOrder)
                                .ToList();

                            // Process cycle 2 skill sets
                            uporder = GetOrderByCycle(uss_cycle2, iscycle1, userid, resultDTO, connection);
                        }
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

        public string GetOrderByCycle(List<GetOrderCalculation> skillSets, bool iscycle1, int userid, ResultDTO resultDTO, SqlConnection connection)
        {
            foreach (var uss in skillSets)
            {
                string uporder = string.Empty;

                if (uss.IsHardStateUser)
                {
                    // Call method for hard state orders
                    uporder = callByHardstate(userid, resultDTO, connection, iscycle1, uss.SkillSetId);

                    if (!string.IsNullOrWhiteSpace(uporder))
                    {
                        // Order assigned successfully
                        resultDTO.Data = uporder;
                        resultDTO.IsSuccess = true;
                        resultDTO.StatusCode = "200";
                        resultDTO.Message = "Order assigned successfully";
                        return uporder;
                    }
                    //else
                    //{
                    //    // Mark as utilized if no order is assigned
                    //    uss.HardStateUtilized = true;
                    //    _oMTDataContext.GetOrderCalculation.Update(uss);
                    //    _oMTDataContext.SaveChanges();
                    //}
                }
                else
                {
                    // Call method for non-hard state orders
                    uporder = callSPbyWeightage(userid, resultDTO, connection, iscycle1, uss.SkillSetId);

                    if (!string.IsNullOrWhiteSpace(uporder))
                    {
                        // Order assigned successfully
                        resultDTO.Data = uporder;
                        resultDTO.IsSuccess = true;
                        resultDTO.StatusCode = "200";
                        resultDTO.Message = "Order assigned successfully";
                        return uporder;
                    }
                }
            }

            // No order was assigned, return an empty string
            return string.Empty;

        }
        public dynamic callSPbyWeightage(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, int skillsetid)
        {
            string updatedOrder;
            SqlCommand command1 = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByWeightage_Threshold"
            };
            command1.Parameters.AddWithValue("@userid", userid);
            command1.Parameters.AddWithValue("@IsCycle1", iscycle1);
            command1.Parameters.AddWithValue("@SkillSetId", skillsetid);
            //output param to get the record
            SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam);

            SqlParameter returnValue = new()
            {
                ParameterName = "@RETURN_VALUE",
                Direction = ParameterDirection.ReturnValue
            };
            command1.Parameters.Add(returnValue);

            command1.ExecuteNonQuery();

            int returnCode = (int)command1.Parameters["@RETURN_VALUE"].Value;

            if (returnCode != 1)
            {
                throw new InvalidOperationException("Stored Procedure call failed.");
            }

            updatedOrder = command1.Parameters["@updatedrecord"].Value.ToString();
            UpdateUtilized(userid, resultDTO, connection, iscycle1, updatedOrder);

            return updatedOrder;
        }

        public dynamic callByHardstate(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, int skillsetid)
        {
            string uporder;

            using SqlCommand command = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByHardstate_Threshold"
            };
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@IsCycle1", iscycle1);
            command.Parameters.AddWithValue("@SkillSetId", skillsetid);
            //output param to get the record
            SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(outputParam);

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
                throw new InvalidOperationException("Stored Procedure call failed.");
            }

            uporder = command.Parameters["@updatedrecord"].Value.ToString();

            UpdateUtilized(userid, resultDTO, connection, iscycle1, uporder);

            return uporder;
        }

        private void UpdateUtilized(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, string updatedOrder)
        {
            if (string.IsNullOrWhiteSpace(updatedOrder))
            {
                resultDTO.Data = "";
                resultDTO.StatusCode = "404";
                resultDTO.IsSuccess = false;
                resultDTO.Message = "No more orders for now, please come back again";
            }
            else
            {
                int? ssid = null;

                //update getordercal table-  check if toc == oc ,if yes make utilized = true,else false
                var jsonArray = JArray.Parse(updatedOrder);

                var firstItem = jsonArray[0] as JObject; // Cast to JObject to access properties
                if (firstItem != null)
                {
                    ssid = firstItem["SkillSetId"] != null ? (int)firstItem["SkillSetId"] : (int?)null;
                }

                if (ssid.HasValue)
                {
                    var UssDetails = _oMTDataContext.GetOrderCalculation.Where(x => x.UserId == userid && x.IsActive && x.SkillSetId == ssid && x.IsCycle1 == iscycle1).FirstOrDefault();

                    if (UssDetails.OrdersCompleted == UssDetails.TotalOrderstoComplete)
                    {
                        string UpdateGocTable = $"UPDATE GetOrderCalculation SET Utilized = 1 WHERE UserId = @UserId AND SkillSetId = @ssid";

                        using SqlCommand UpdateGocTbl = connection.CreateCommand();
                        UpdateGocTbl.CommandText = UpdateGocTable;
                        UpdateGocTbl.Parameters.AddWithValue("@UserId", userid);
                        UpdateGocTbl.Parameters.AddWithValue("@ssid", ssid);

                        UpdateGocTbl.ExecuteNonQuery();
                    }

                }

                resultDTO.Data = updatedOrder;
                resultDTO.IsSuccess = true;
                resultDTO.StatusCode = "200";
                resultDTO.Message = "Order assigned successfully";
            }
        }


        public ResultDTO GetTrdPendingOrderForUser(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                //check if user has uncompleted orders in all of his skillsets. if any is there- dont assign orders,say- first complete pending orders
                List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                           join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                           where us.UserId == userid && us.IsActive
                                           && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                           select ss.SkillSetName).ToList();

                List<Dictionary<string, object>> noStatusRecords = new List<Dictionary<string, object>>();

                foreach (string tablename in tablenames)
                {
                    // check for pending orders and send them 
                    var columns1 = (from ss in _oMTDataContext.SkillSet
                                    join dt in _oMTDataContext.TemplateColumns on ss.SkillSetId equals dt.SkillSetId
                                    where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                    select dt.ColumnAliasName).ToList();

                    var columns2 = (from ss in _oMTDataContext.SkillSet
                                    join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                    where ss.SkillSetName == tablename && dt.IsGetOrderColumn
                                    select dt.DefaultColumnName).ToList();


                    // get date type columns 
                    var datecol = (from ss in _oMTDataContext.SkillSet
                                   join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                   where ss.SkillSetName == tablename && dt.IsGetOrderColumn && dt.DataType == "Date"
                                   select dt.DefaultColumnName).ToList();


                    var columns = (columns1 ?? Enumerable.Empty<string>()).Concat(columns2 ?? Enumerable.Empty<string>());
                    string selectedColumns = string.Join(", ", columns.Select(c => $"t1.{c}"));

                    string query = $@"
                                    SELECT 
                                        {selectedColumns},
                                        t2.SkillSetName AS SkillSetName, 
                                        t1.SkillSetId,
                                        t3.SystemOfRecordName AS SystemOfRecordName,
                                        t1.SystemOfRecordId,
                                        t1.Id,
                                        t1.StartTime
                                    FROM 
                                        [{tablename}] AS t1
                                    LEFT JOIN SkillSet AS t2 ON t1.SkillSetId = t2.SkillSetId
                                    LEFT JOIN SystemOfRecord AS t3 ON t1.SystemofRecordId = t3.SystemOfRecordId
                                    WHERE 
                                        UserId = @UserId 
                                        AND (Status IS NULL OR Status = '')
                                    ORDER BY 
	                                    t1.IsPriority DESC,t1.StartTime ASC;";


                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userid);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var querydt1 = datatable.AsEnumerable()
                                 .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                 column => column.ColumnName,
                                 column =>
                                 {
                                     if (datecol.Contains(column.ColumnName) && column.DataType == typeof(DateTime))
                                     {
                                         if (row[column] == DBNull.Value)
                                         {
                                             return "";
                                         }
                                         DateTime dateValue = (DateTime)row[column];
                                         return dateValue.ToString("yyyy-MM-dd");  // Format as date string
                                     }
                                     return row[column] == DBNull.Value ? "" : row[column];
                                 })).ToList();

                    noStatusRecords.AddRange(querydt1);

                }
                if (noStatusRecords.Count > 0)
                {
                    var orderedRecords = noStatusRecords
                         .OrderByDescending(record => bool.Parse(record["IsPriority"].ToString()))
                         .ThenBy(record => DateTime.Parse(record["StartTime"].ToString()))
                         .First();

                    orderedRecords.Remove("StartTime");

                    var dataToReturn = new List<Dictionary<string, object>> { orderedRecords };

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "You have an order in your queue,please finish this first";
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = JsonConvert.SerializeObject(dataToReturn);
                }
                else
                {
                    List<GetOrderCalculation> userskillsetlist = (from uss in _oMTDataContext.UserSkillSet
                                                                  join goc in _oMTDataContext.GetOrderCalculation on uss.UserSkillSetId equals goc.UserSkillSetId
                                                                  join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                  join up in _oMTDataContext.UserProfile on uss.UserId equals up.UserId
                                                                  where uss.IsActive && goc.IsActive && ss.IsActive && up.IsActive && goc.UserId == userid && ss.SystemofRecordId == 3
                                                                  orderby goc.PriorityOrder // uss.IsHardStateUser descending, uss.Percentage descending
                                                                  select new GetOrderCalculation
                                                                  {
                                                                      UserId = goc.UserId,
                                                                      UserSkillSetId = goc.UserSkillSetId,
                                                                      SkillSetId = goc.SkillSetId,
                                                                      TotalOrderstoComplete = goc.TotalOrderstoComplete,
                                                                      OrdersCompleted = goc.OrdersCompleted,
                                                                      Weightage = goc.Weightage,
                                                                      PriorityOrder = goc.PriorityOrder,
                                                                      IsActive = goc.IsActive,
                                                                      UpdatedDate = goc.UpdatedDate,
                                                                      IsCycle1 = goc.IsCycle1,
                                                                      IsHardStateUser = goc.IsHardStateUser,
                                                                      Utilized = goc.Utilized,
                                                                      HardStateUtilized = goc.HardStateUtilized,
                                                                  }).ToList();

                    bool iscycle1 = true;
                    var cycle = new List<GetOrderCalculation>();

                    List<GetOrderCalculation> trd_cycle1 = userskillsetlist.Where(x => x.Utilized == false && x.IsCycle1).OrderBy(x => x.PriorityOrder).ToList();
                    List<GetOrderCalculation> trd_cycle2 = new();
                    if (trd_cycle1.Count == 0)
                    {
                        iscycle1 = false;
                        trd_cycle2 = userskillsetlist.Where(x => x.Utilized == false && x.IsCycle1 == false).OrderBy(x => x.PriorityOrder).ToList();
                    }

                    // check if cycle 1 has no more orders , then send to cycle 2 ? or 

                    string updatedOrder;

                    updatedOrder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1);
                    if (string.IsNullOrWhiteSpace(updatedOrder) && iscycle1)
                    {
                        iscycle1 = false;

                        updatedOrder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1);

                        if (string.IsNullOrWhiteSpace(updatedOrder))
                        {
                            resultDTO.Data = "";
                            resultDTO.StatusCode = "404";
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "No more orders for now, please come back again";
                        }
                        else
                        {
                            resultDTO.Data = updatedOrder;
                            resultDTO.IsSuccess = true;
                            resultDTO.StatusCode = "4200";
                            resultDTO.Message = "Order assigned successfully";
                        }

                    }
                    else
                    {
                        resultDTO.Data = updatedOrder;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Order assigned successfully";
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

        public dynamic GetTrdPendingOrder_Threshold(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1)
        {
            string updatedOrder;
            SqlCommand command1 = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetTrdPendingOrder_Threshold"
            };
            command1.Parameters.AddWithValue("@userid", userid);
            command1.Parameters.AddWithValue("@IsCycle1", iscycle1);
            //output param to get the record
            SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam);

            SqlParameter returnValue = new()
            {
                ParameterName = "@RETURN_VALUE",
                Direction = ParameterDirection.ReturnValue
            };
            command1.Parameters.Add(returnValue);

            command1.ExecuteNonQuery();

            int returnCode = (int)command1.Parameters["@RETURN_VALUE"].Value;

            if (returnCode != 1)
            {
                throw new InvalidOperationException("Stored Procedure call failed.");
            }

            updatedOrder = command1.Parameters["@updatedrecord"].Value.ToString();

            UpdateUtilized(userid, resultDTO, connection, iscycle1, updatedOrder);

            return updatedOrder;
        }
        public ResultDTO GetOrderInfo(OrderInfoDTO orderInfoDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                var skillset = (from tc in _oMTDataContext.TemplateColumns
                                join ss in _oMTDataContext.SkillSet on tc.SkillSetId equals ss.SkillSetId
                                where tc.SkillSetId == orderInfoDTO.SkillSetId && ss.IsActive
                                select new
                                {
                                    SkillSetName = ss.SkillSetName,
                                    SystemofRecordId=ss.SystemofRecordId,
                                }).FirstOrDefault();

                if (skillset != null)
                {
                    var reportCol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == orderInfoDTO.SkillSetId && rc.IsActive  && rc.SystemOfRecordId == skillset.SystemofRecordId && rc.IsActive
                                     select mrc.ReportColumnName).ToList();

                    string sqlquery1 = $"SELECT CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.OrderId,ss.SkillSetName as SkillSet,ps.Status as Status,t.Remarks,";

                    if (reportCol.Count > 0)
                    {
                        foreach (string col in reportCol)
                        {
                            if (col.Contains("Date", StringComparison.OrdinalIgnoreCase))
                            {
                                // Handle date formatting
                                sqlquery1 += $@"
                            CASE 
                                WHEN CAST(t.{col} AS DATETIME) = CAST(t.{col} AS DATE) 
                                THEN FORMAT(t.{col}, 'yyyy-MM-dd') 
                                ELSE FORMAT(t.{col}, 'yyyy-MM-dd HH:mm:ss') 
                            END AS {col}, ";
                            }
                            else
                            {
                                // Add other columns
                                sqlquery1 += $"t.{col}, ";
                            }
                        }
                    }
                    string commonSqlPart = $" CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime," +
                                           $" CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime," +
                                           $" CONVERT(VARCHAR(10), t.AllocationDate, 120) as CompletionDate, " +
                                           $" CONCAT((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600), ':'," +
                                           $" ((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60), ':', " +
                                           $" (DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60)) as TimeTaken" +
                                           $" FROM [{skillset.SkillSetName}] t" +
                                           $" INNER JOIN SkillSet ss ON ss.SkillSetId = t.SkillSetId" +
                                           $" INNER JOIN ProcessStatus ps ON ps.Id = t.Status" +
                                           $" INNER JOIN UserProfile up ON up.UserId = t.UserId" +
                                           $" WHERE  t.OrderId = @OrderId AND t.Status IS NOT NULL AND  t.Status <> '' AND  t.UserId IS NOT NULL";

                    // Combine everything into the final query
                    string sqlquery = sqlquery1 + commonSqlPart;

                    using SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = sqlquery;

                    cmd.Parameters.AddWithValue("@OrderId", orderInfoDTO.OrderId);

                    using SqlDataAdapter dataAdapter = new(cmd);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);

                    if (dataset != null && dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                    {
                        DataTable datatable = dataset.Tables[0];

                        var orderlist = datatable.AsEnumerable()
                            .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary
                            (column => column.ColumnName, column => row[column] == DBNull.Value ? string.Empty : row[column])).ToList();

                        resultDTO.Data = orderlist;
                        resultDTO.Message = "List of Order Details";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Neither Order Exists Nor its Processed";

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