﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public ResultDTO UpdateGetOrderCalculation()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                //check getorder table for any records , if yes put it into backup table, truncate getorder table and call the below method 
                var tablename = "GetOrderCalculation";

                var existingdetails = _oMTDataContext.GetOrderCalculation
                                                     .Select(x => new Utilization
                                                     {
                                                         UserId = x.UserId,
                                                         UserSkillSetId = x.SkillSetId,
                                                         SkillSetId = x.SkillSetId,
                                                         TotalOrderstoComplete = x.TotalOrderstoComplete,
                                                         OrdersCompleted = x.OrdersCompleted,
                                                         Weightage = x.Weightage,
                                                         PriorityOrder = x.PriorityOrder,
                                                         Utilized = x.Utilized,
                                                         IsActive = x.IsActive,
                                                         UpdatedDate = x.UpdatedDate,
                                                         IsCycle1 = x.IsCycle1,
                                                         IsHardStateUser = x.IsHardStateUser,
                                                         HardStateUtilized = x.HardStateUtilized,
                                                     }).ToList();

                if (existingdetails.Count > 0)
                {
                    _oMTDataContext.Utilization.AddRange(existingdetails);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand truncatecmd = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = "Truncate table " + tablename,
                    };
                    connection.Open();
                    truncatecmd.ExecuteNonQuery();

                    InsertGetOrderCalculation(resultDTO, 0);
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

        public void InsertGetOrderCalculation(ResultDTO resultDTO, int userid)
        {
            string? connectionstring = _oMTDataContext.Database.GetConnectionString();
            using SqlConnection connection = new(connectionstring);
            connection.Open();

            try
            {
                List<int> users = new List<int>();

                if (userid == 0)
                {
                    users = _oMTDataContext.UserProfile.Where(x => x.IsActive).Select(_ => _.UserId).ToList();
                }
                else
                {
                    users.Add(userid);
                }

                foreach (var user in users)
                {
                    var PriorityOrder = 1;

                    var userskillsets = (from up in _oMTDataContext.UserProfile
                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                         where up.UserId == user && up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                         orderby uss.IsCycle1 descending, uss.Percentage descending
                                         select new UserSkillSet
                                         {
                                             UserId = uss.UserId,
                                             UserSkillSetId = uss.UserSkillSetId,
                                             SkillSetId = uss.SkillSetId,
                                             Percentage = uss.Percentage,
                                             IsActive = uss.IsActive,
                                             IsHardStateUser = uss.IsHardStateUser,
                                             IsCycle1 = uss.IsCycle1,
                                             HardStateName = uss.HardStateName,
                                         }).ToList();


                    foreach (var userkillset in userskillsets)
                    {
                        var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userkillset.SkillSetId && x.IsActive).FirstOrDefault();

                        double totalorders = ((double)userkillset.Percentage / 100) * skillset.Threshold;

                        int roundedtotalorders = (int)Math.Round(totalorders);

                        GetOrderCalculation getOrderCalculation = new GetOrderCalculation
                        {
                            UserId = userkillset.UserId,
                            UserSkillSetId = userkillset.UserSkillSetId,
                            SkillSetId = userkillset.SkillSetId,
                            TotalOrderstoComplete = roundedtotalorders,
                            OrdersCompleted = 0,
                            Weightage = userkillset.Percentage,
                            PriorityOrder = PriorityOrder++,
                            IsActive = true,
                            UpdatedDate = DateTime.Now,
                            IsCycle1 = userkillset.IsCycle1,
                            IsHardStateUser = userkillset.IsHardStateUser,
                            Utilized = false,
                            HardStateUtilized = false,
                        };

                        _oMTDataContext.GetOrderCalculation.Add(getOrderCalculation);
                        _oMTDataContext.SaveChanges();

                    }

                }

                // update priorityorder in goc table based on priority orders in skillset tables

                Update_by_priorityOrder(resultDTO, connection, userid);

                resultDTO.IsSuccess = true;
                resultDTO.Message = "GetOrderCalculation table updated successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
        }

        public void Update_by_priorityOrder(ResultDTO resultDTO, SqlConnection connection, int userid)
        {
            try
            {
                List<SkillSet> skillsets = new List<SkillSet>();

                if (userid == 0)
                {
                    skillsets = (from ss in _oMTDataContext.SkillSet
                                 join goc in _oMTDataContext.GetOrderCalculation on ss.SkillSetId equals goc.SkillSetId
                                 where ss.IsActive && goc.IsActive && goc.IsCycle1
                                 select new SkillSet
                                 {
                                     SkillSetId = ss.SkillSetId,
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId
                                 }).Distinct().ToList();
                }
                else
                {
                    skillsets = (from ss in _oMTDataContext.SkillSet
                                 join goc in _oMTDataContext.GetOrderCalculation on ss.SkillSetId equals goc.SkillSetId
                                 where ss.IsActive && goc.IsActive && goc.IsCycle1 && goc.UserId == userid
                                 select new SkillSet
                                 {
                                     SkillSetId = ss.SkillSetId,
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId
                                 }).Distinct().ToList();
                }

                foreach (var ss in skillsets)
                {
                    string tableName = ss.SkillSetName;

                    string query = $@"
                                    SELECT COUNT(*)
                                    FROM {tableName} 
                                    WHERE IsPriority = 1 AND UserId IS NULL AND Status IS NULL";

                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = query
                    };

                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        using SqlCommand priority = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "UpdateGoc_PriorityOrder"
                        };
                        priority.Parameters.AddWithValue("@SkillSetId", ss.SkillSetId);
                        priority.Parameters.AddWithValue("@SystemOfRecordId", ss.SystemofRecordId);
                        priority.Parameters.AddWithValue("@UserId", userid);

                        SqlParameter priority_returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE_Po",
                            Direction = ParameterDirection.ReturnValue
                        };
                        priority.Parameters.Add(priority_returnValue);

                        priority.ExecuteNonQuery();

                        int priority_returnCode = (int)priority.Parameters["@RETURN_VALUE_Po"].Value;

                        if (priority_returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while updating GetOrderCalculation table.");
                        }
                    }

                }
                resultDTO.IsSuccess = true;
                resultDTO.Message = "GetOrderCalculation table updated successfully.";
            }
            catch (Exception ex)
            {

                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
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
    }
}
