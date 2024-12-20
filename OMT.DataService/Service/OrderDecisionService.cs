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
using System.Dynamic;
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

        private readonly ITemplateService _templateService;

        public OrderDecisionService(OMTDataContext oMTDataContext, ITemplateService templateService)
        {
            _oMTDataContext = oMTDataContext;
            _templateService = templateService;
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

                //check if user has any TRD skillsets to show the getpendingorders button

                List<string> trdskillsets = (from us in _oMTDataContext.UserSkillSet
                                             join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                             where us.UserId == userid && us.IsActive && ss.SystemofRecordId == 3
                                             && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                             select ss.SkillSetName).ToList();

                bool ispending = false;

                if (trdskillsets.Count > 0)
                {
                    ispending = true;
                }

                PendingOrdersResponseDTO pendingOrdersResponseDTO = new PendingOrdersResponseDTO();
                Dictionary<string, object> orderedRecords = new Dictionary<string, object>();

                // process pending orders if any for the user and send the details
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
                    orderedRecords = noStatusRecords
                         .OrderByDescending(record => bool.Parse(record["IsPriority"].ToString()))
                         .ThenBy(record => DateTime.Parse(record["StartTime"].ToString()))
                         .First();

                    orderedRecords.Remove("StartTime");

                    var dataToReturn = new List<Dictionary<string, object>> { orderedRecords };

                    //check if order is from trd pending
                    var istrd_pending = false;
                    var istrd = (int)orderedRecords["SystemOfRecordId"] == 3;
                    var tableid = (int)orderedRecords["Id"];

                    var tablename = orderedRecords["SkillSetName"].ToString();

                    if (istrd)
                    {
                        var pendingorder_query = $"SELECT IsPending FROM {tablename} where Id = {tableid}";

                        using SqlCommand trd_command = connection.CreateCommand();
                        trd_command.CommandText = pendingorder_query;

                        using SqlDataAdapter trd_dataAdapter = new SqlDataAdapter(trd_command);

                        DataSet trd_dataset = new DataSet();

                        trd_dataAdapter.Fill(trd_dataset);

                        DataTable trd_datatable = trd_dataset.Tables[0];

                        var trd_pnd = trd_datatable.AsEnumerable()
                                      .Select(row => trd_datatable.Columns.Cast<DataColumn>().ToDictionary(
                                       column => column.ColumnName,
                                       column => row[column]));

                        if (trd_pnd.Any())
                        {
                            istrd_pending = trd_pnd.Any(record =>
                                                record.ContainsKey("IsPending") && bool.TryParse(record["IsPending"]?.ToString(), out var trd_isPending) && trd_isPending);
                        }
                    }

                    // check if order is from TIQE 

                    var istiqe_order = orderedRecords.ContainsKey("SkillSetName") && orderedRecords["SkillSetName"].ToString() == "TIQE" && (int)orderedRecords["SystemOfRecordId"] == 2;


                    pendingOrdersResponseDTO = new PendingOrdersResponseDTO
                    {
                        IsPending = ispending,
                        PendingOrder = dataToReturn,
                        IsTiqe = istiqe_order,
                        IsTrdPending = istrd_pending
                    };
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "You have been assigned with an order by your TL,please finish this first";
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = pendingOrdersResponseDTO;
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
                    string po_uporder = string.Empty;

                    List<GetOrderCalculation> uss_cycle1 = userskillsetlist.Where(x => x.Utilized == false && x.IsCycle1).OrderBy(x => x.PriorityOrder).ThenByDescending(x => x.IsHardStateUser).ThenByDescending(x => x.Weightage).ToList();

                    if (uss_cycle1.Count == 0)
                    {
                        iscycle1 = false;

                        po_uporder = GetOrderByPo(userid, resultDTO, connection, iscycle1);

                        //if no priority orders then go by weightage 
                        if (string.IsNullOrEmpty(po_uporder))
                        {
                            // Get unutilized skill sets for cycle 2, ordered by priority
                            var uss_cycle2 = userskillsetlist.Where(x => x.Utilized == false && !x.IsCycle1).OrderBy(x => x.PriorityOrder).ThenByDescending(x => x.IsHardStateUser).ThenByDescending(x => x.Weightage).ToList();

                            // Process cycle 2 skill sets by weightage
                            uporder = GetOrderByCycle(uss_cycle2, iscycle1, userid, resultDTO, connection);
                        }

                    }
                    else
                    {
                        // get only priority orders from all the cycle1 skillsets 

                        po_uporder = GetOrderByPo(userid, resultDTO, connection, iscycle1);

                        //if no priority orders then go by weightage 
                        if (string.IsNullOrEmpty(po_uporder))
                        {
                            uporder = GetOrderByCycle(uss_cycle1, iscycle1, userid, resultDTO, connection);

                            // If no orders were assigned in cycle 1, proceed to cycle 2
                            if (string.IsNullOrWhiteSpace(uporder))
                            {
                                iscycle1 = false;

                                // get only priority orders from all the cycle1 skillsets 
                                po_uporder = GetOrderByPo(userid, resultDTO, connection, iscycle1);

                                if (string.IsNullOrEmpty(po_uporder))
                                {
                                    // Get unutilized skill sets for cycle 2, ordered by priority
                                    var uss_cycle2 = userskillsetlist.Where(x => x.Utilized == false && !x.IsCycle1).OrderBy(x => x.PriorityOrder).ThenByDescending(x => x.IsHardStateUser).ThenByDescending(x => x.Weightage).ToList();

                                    // Process cycle 2 skill sets
                                    uporder = GetOrderByCycle(uss_cycle2, iscycle1, userid, resultDTO, connection);

                                }

                            }
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
        public dynamic GetOrderByPo(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1)
        {
            string po_assigned;
            int userskillsetid;

            using SqlCommand command = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByPo_Threshold"
            };
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@IsCycle1", iscycle1);

            //output param to get the record
            SqlParameter outputParam_po = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam_po.Direction = ParameterDirection.Output;
            command.Parameters.Add(outputParam_po);

            SqlParameter outputParam_po_2 = new SqlParameter("@update_userskillsetid", SqlDbType.Int);
            outputParam_po_2.Direction = ParameterDirection.Output;
            command.Parameters.Add(outputParam_po_2);

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

            po_assigned = outputParam_po.Value.ToString();
            userskillsetid = (int)outputParam_po_2.Value;

            UpdateUtilized(userid, resultDTO, connection, iscycle1, po_assigned, userskillsetid);

            return po_assigned;

        }
        public string GetOrderByCycle(List<GetOrderCalculation> skillSets, bool iscycle1, int userid, ResultDTO resultDTO, SqlConnection connection)
        {
            foreach (var uss in skillSets)
            {
                string uporder = string.Empty;

                if (uss.IsHardStateUser)
                {
                    // Call method for hard state orders
                    uporder = callByHardstate(userid, resultDTO, connection, iscycle1, uss.SkillSetId, uss.UserSkillSetId);

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
            int userskillsetid_w;

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

            SqlParameter outputParam_po_2 = new SqlParameter("@update_userskillsetid", SqlDbType.Int);
            outputParam_po_2.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam_po_2);

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
            userskillsetid_w = (int)outputParam_po_2.Value;

            UpdateUtilized(userid, resultDTO, connection, iscycle1, updatedOrder, userskillsetid_w);

            return updatedOrder;
        }

        public dynamic callByHardstate(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, int skillsetid, int userskillsetid)
        {
            string uporder;
            //int userskillsetid_h;

            using SqlCommand command = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByHardstate_Threshold"
            };
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@IsCycle1", iscycle1);
            command.Parameters.AddWithValue("@SkillSetId", skillsetid);
            command.Parameters.AddWithValue("@UserSkillSetId", userskillsetid);

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

            UpdateUtilized(userid, resultDTO, connection, iscycle1, uporder, userskillsetid);

            return uporder;
        }

        private void UpdateUtilized(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, string updatedOrder, int userskillsetid)
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
                    var UssDetails = _oMTDataContext.GetOrderCalculation.Where(x => x.UserId == userid && x.IsActive && x.SkillSetId == ssid && x.IsCycle1 == iscycle1 && x.UserSkillSetId == userskillsetid).FirstOrDefault();

                    if (UssDetails.OrdersCompleted == UssDetails.TotalOrderstoComplete)
                    {
                        string UpdateGocTable = $"UPDATE GetOrderCalculation SET Utilized = 1 WHERE UserId = @UserId AND SkillSetId = @ssid AND IsCycle1 = @IsCycle1 AND UserSkillSetId = @UserSkillSetId AND IsActive = 1";

                        using SqlCommand UpdateGocTbl = connection.CreateCommand();
                        UpdateGocTbl.CommandText = UpdateGocTable;
                        UpdateGocTbl.Parameters.AddWithValue("@UserId", userid);
                        UpdateGocTbl.Parameters.AddWithValue("@ssid", ssid);
                        UpdateGocTbl.Parameters.AddWithValue("@IsCycle1", iscycle1);
                        UpdateGocTbl.Parameters.AddWithValue("@UserSkillSetId", userskillsetid);

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

                    List<GetOrderCalculation> trd_cycle1 = userskillsetlist.Where(x => x.Utilized == false && x.IsCycle1).OrderBy(x => x.PriorityOrder).ThenByDescending(x => x.IsHardStateUser).ThenByDescending(x => x.Weightage).ToList();

                    if (trd_cycle1.Count == 0)
                    {
                        iscycle1 = false;

                    }

                    // check if cycle 1 has no more orders , then send to cycle 2 ? or 

                    string po_uporder = string.Empty;
                    string updatedOrder;

                    // get only priority orders from all the cycle1 skillsets 

                    po_uporder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1, true);

                    //if no priority orders then go by weightage in cycle1
                    if (string.IsNullOrEmpty(po_uporder))
                    {
                        updatedOrder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1, false);

                        if (string.IsNullOrWhiteSpace(updatedOrder) && iscycle1)
                        {
                            iscycle1 = false;

                            po_uporder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1, true);

                            if (string.IsNullOrEmpty(po_uporder))
                            {
                                updatedOrder = GetTrdPendingOrder_Threshold(userid, resultDTO, connection, iscycle1, false);

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
                                    resultDTO.StatusCode = "200";
                                    resultDTO.Message = "Order assigned successfully";
                                }
                            }

                        }
                        else if (string.IsNullOrWhiteSpace(updatedOrder) && !iscycle1)
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
                            resultDTO.Message = "Order assigned successfully";
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

        public dynamic GetTrdPendingOrder_Threshold(int userid, ResultDTO resultDTO, SqlConnection connection, bool iscycle1, bool ispriority)
        {
            string updatedOrder;
            int userskillsetid_p;

            SqlCommand command1 = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetTrdPendingOrder_Threshold"
            };
            command1.Parameters.AddWithValue("@userid", userid);
            command1.Parameters.AddWithValue("@IsCycle1", iscycle1);
            command1.Parameters.AddWithValue("@IsPriority", ispriority);

            //output param to get the record
            SqlParameter outputParam = new SqlParameter("@updatedrecord", SqlDbType.NVarChar, -1);
            outputParam.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam);

            SqlParameter outputParam_po_2 = new SqlParameter("@update_userskillsetid", SqlDbType.Int);
            outputParam_po_2.Direction = ParameterDirection.Output;
            command1.Parameters.Add(outputParam_po_2);

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
            userskillsetid_p = (int)outputParam_po_2.Value;

            UpdateUtilized(userid, resultDTO, connection, iscycle1, updatedOrder, userskillsetid_p);

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
                                join sr in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sr.SystemofRecordId
                                where tc.SkillSetId == orderInfoDTO.SkillSetId && ss.IsActive
                                select new
                                {
                                    SkillSetName = ss.SkillSetName,
                                    SkillSetId = ss.SkillSetId,
                                    SystemofRecordId = ss.SystemofRecordId,
                                    SystemofRecordName = sr.SystemofRecordName,


                                }).FirstOrDefault();


                string completionDateField = "";

                if (orderInfoDTO.SystemofRecordId == 1)
                {
                    // Adjust CompletionDate field placement after Status
                    completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";
                }
                if (orderInfoDTO.SystemofRecordId != 1)
                {
                    completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                }
                if (skillset != null)
                {
                    var skillsetdetails = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillset.SkillSetName && x.IsActive).FirstOrDefault();

                    var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == orderInfoDTO.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillset.SystemofRecordId && rc.IsActive
                                     orderby rc.ColumnSequence
                                     select mrc.ReportColumnName
                                   ).ToList();

                    // Query to get column data types for the dynamic table
                    SqlCommand sqlCommand_columnTypeQuery;
                    SqlDataAdapter dataAdapter_columnTypeQuery;
                    List<Dictionary<string, object>> columnTypes;
                    _templateService.GetDataType(connection, skillsetdetails, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

                    // Extract valid columns based on column type
                    var validDateCols = reportcol
                                                 .Where(col => columnTypes.Any(ct =>
                                                     ct.ContainsKey("COLUMN_NAME") &&
                                                     ct.ContainsKey("DATA_TYPE") &&
                                                     ct["COLUMN_NAME"].ToString() == col &&
                                                     (ct["DATA_TYPE"].ToString() == "datetime" || ct["DATA_TYPE"].ToString() == "date")))
                                                 .ToList();


                    string sqlquery1 = $"SELECT ";

                    if (reportcol.Count > 0)
                    {
                        foreach (string col in reportcol)
                        {
                            if (validDateCols.Contains(col))
                            {
                                sqlquery1 += $@"
                                               CASE 
                                                   WHEN CAST(t.{col} AS DATETIME) = CAST(t.{col} AS DATE) 
                                                   THEN FORMAT(t.{col}, 'MM-dd-yyyy') 
                                                   ELSE FORMAT(t.{col}, 'MM-dd-yyyy HH:mm:ss') 
                                               END as {col}, ";
                            }
                            else
                            {
                                sqlquery1 += $"t.{col}, ";
                            }
                        }
                    }
                    string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName, ps.Status as Status, ps.Id as StatusId, ";

                    commonSqlPart += $"{completionDateField}, t.Remarks,t.Id, " +
                                     $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                     $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                     $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                     $"ss.SkillSetName as SkillSet,ss.SkillSetId,sr.SystemofRecordId,sr.SystemofRecordName " +
                                     $"FROM {skillset.SkillSetName} t " +
                                     $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                     $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                     $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                     $"INNER JOIN SystemOfRecord sr ON sr.SystemofRecordId = ss.SystemofRecordId " +
                                     $"WHERE t.OrderId=@OrderId AND t.Status IS NOT NULL AND t.Status <> '' AND t.UserId IS NOT NULL";



                    // Combine everything into the final query
                    string sqlquery = sqlquery1 + commonSqlPart;

                    using SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = sqlquery;

                    cmd.Parameters.AddWithValue("@OrderId", orderInfoDTO.OrderId);

                    using SqlDataAdapter dataAdapter = new(cmd);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);

                    //Duplicate Orders 
                    if (dataset != null && dataset.Tables.Count >= 1 && dataset.Tables[0].Rows.Count > 0)
                    {
                        DataTable datatable = dataset.Tables[0];

                        // Grouping orders 
                        var LatestOrders = datatable.AsEnumerable()
                            .GroupBy(row => row["OrderId"].ToString())
                            .Select(group =>
                            {
                                // finding row with the latest EndTime
                                var latestrow = group.OrderByDescending(row => row["EndTime"]).FirstOrDefault();

                                var UpdatedId = latestrow["Id"].ToString();

                                var orderDetails = group.Select(row =>
                                     datatable.Columns.Cast<DataColumn>()
                                         .ToDictionary(
                                             column => column.ColumnName,
                                             column => row[column] == DBNull.Value ? string.Empty : row[column]
                                         )
                                 ).ToList();

                                return new OrderdetailsDTO
                                {
                                    UpdatedId = UpdatedId,
                                    OrderDetails = orderDetails, // order details -> dynamic 
                                    
                                };
                            }).ToList();

                        resultDTO.Data = LatestOrders;
                        resultDTO.Message = "Order Details Fetched Successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Neither Order Exists Nor its Processed";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset Table Not Found";
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
        public ResultDTO UpdateOrderStatusByTL(int userid, UpdateOrderStatusByTLDTO updateOrderStatusByTLDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "400" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                var targettablename = "Order_Histroy";

                var skillset = (from tc in _oMTDataContext.TemplateColumns
                                join ss in _oMTDataContext.SkillSet on tc.SkillSetId equals ss.SkillSetId
                                where tc.SkillSetId == updateOrderStatusByTLDTO.SkillSetId && ss.IsActive
                                select new
                                {
                                    Tablename = ss.SkillSetName,

                                }).FirstOrDefault();

                // check if its already into invoice, if yes dont allow to update

                DateTime todayUtc = DateTime.UtcNow.Date; // Today at midnight in UTC
                DateTime endtime = todayUtc.AddHours(13); // Today 6:30 PM UTC
                DateTime starttime = endtime.AddDays(-1);

                var editable = false;
                var completiondate = DateTime.UtcNow;

                string[] ExcludedColumns = { "OrderId", "ProjectId", "SystemofRecordId", "SkillSetId", "UserId", "Status" };

                string DynamicColumns = string.Empty;

                if (skillset != null)
                {
                    string details = $@"SELECT * FROM {skillset.Tablename} WHERE OrderId = @OrderId";

                    using (SqlCommand detailscommand = new SqlCommand(details, connection))
                    {
                        detailscommand.Parameters.AddWithValue("@OrderId", updateOrderStatusByTLDTO.OrderId);

                        using SqlDataAdapter detailsAdapter = new SqlDataAdapter(detailscommand);

                        DataSet detailsDS = new DataSet();
                        detailsAdapter.Fill(detailsDS);

                        if (detailsDS.Tables[0].Rows.Count > 0)
                        {
                            DataRow detailsrow = detailsDS.Tables[0].Rows[0];
                            completiondate = (DateTime)detailsrow["CompletionDate"];
                        }

                        if ((completiondate <= endtime || completiondate >= starttime) && DateTime.UtcNow >= endtime)
                        {
                            editable = false;
                        }
                        else if (completiondate > endtime && DateTime.UtcNow >= endtime)
                        {
                            editable = true;
                        }
                        else if ((completiondate <= endtime && completiondate >= starttime) && DateTime.UtcNow < endtime)
                        {
                            editable = true;
                        }
                        else if (completiondate < starttime)
                        {
                            editable = false;
                        }
                    }

                    // Check if orders Exist in invoice 
                    string tableName = skillset.Tablename;

                    string inv_query = $"SELECT OrderId FROM InvoiceDump WHERE Skillset = '{tableName}' and OrderId = @OrderId";

                    using (SqlCommand command = new SqlCommand(inv_query, connection))
                    {

                        command.Parameters.AddWithValue("@OrderId", updateOrderStatusByTLDTO.OrderId);

                        using SqlDataAdapter Inv_dataAdapter = new SqlDataAdapter(command);

                        DataSet inDS = new DataSet();

                        Inv_dataAdapter.Fill(inDS);

                        DataTable inv_datatable = inDS.Tables[0];

                        var inv_dt = inv_datatable.AsEnumerable()
                                      .Select(row => inv_datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        // if yes dont allow to update
                        if (inv_dt.Count > 0)
                        {
                            editable = false;
                        }

                        // allow to edit only if completiondate <= endtime and >= startime and DateTime.UtcNow < endtime
                        if (editable)
                        {

                            DynamicColumns = GetDynamicColumns(connection, skillset.Tablename, ExcludedColumns, resultDTO);

                            string ordersql = $@"SELECT OrderId, ProjectId, SystemofRecordId, SkillSetId, UserId, Status,
                                        (  SELECT {DynamicColumns} FROM {tableName}
                                        WHERE OrderId = @OrderId
                                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER ) AS OrderDetailsJson
                                        FROM {tableName}  WHERE OrderId = @OrderId";


                            //Fetching Old Datas from the table
                            using (SqlCommand commands = new SqlCommand(ordersql, connection))
                            {
                                
                                commands.Parameters.AddWithValue("@OrderId", updateOrderStatusByTLDTO.OrderId);

                                using SqlDataAdapter dataAdapter = new(commands);

                                DataTable orderdetails = new DataTable();
                                dataAdapter.Fill(orderdetails);

                                if (orderdetails.Rows.Count > 0)
                                { 
                                    DataRow row = orderdetails.Rows[0];
                                    var oldUserId = row["UserId"];
                                    var oldOrderid = row["OrderId"];
                                    var oldSkillsetid = row["SkillSetId"];
                                    var oldProjectid = row["ProjectId"];
                                    var oldSystemofRecordid = row["SystemofRecordId"];
                                    var oldStatus = row["Status"];
                                    string oldOrderDetails = row["OrderDetailsJson"] as string;

                                    // Insert old details into Order_History table   
                                    string insertsql = @"INSERT INTO Order_History (Skillsetid, Orderid, UserId, Projectid, SystemofRecordid,Status,Orderdetails,UpdatedBy,UpdatedTime)  
                                                     VALUES (@Skillsetid, @Orderid, @UserId, @Projectid, @SystemofRecordid, @Status,@Orderdetails,@UpdatedBy,@UpdatedTime)";

                                    using (SqlCommand insertCommand = new SqlCommand(insertsql, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@Skillsetid", oldSkillsetid);
                                        insertCommand.Parameters.AddWithValue("@Orderid", oldOrderid);
                                        insertCommand.Parameters.AddWithValue("@UserId", oldUserId);
                                        insertCommand.Parameters.AddWithValue("@Projectid", oldProjectid);
                                        insertCommand.Parameters.AddWithValue("@SystemofRecordid", oldSystemofRecordid);
                                        insertCommand.Parameters.AddWithValue("@Status", oldStatus);
                                        insertCommand.Parameters.AddWithValue("@Orderdetails", oldOrderDetails);
                                        insertCommand.Parameters.AddWithValue("@UpdatedBy", userid);
                                        insertCommand.Parameters.AddWithValue("@UpdatedTime", DateTime.Now);

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    resultDTO.StatusCode = "404";
                                    resultDTO.IsSuccess = false;
                                    resultDTO.Message = "Order not found";
                                    return resultDTO;
                                }

                            }
                            //Updating Skillset table    
                            string updatesql = $@"UPDATE {tableName} SET Status = @Status,TLDescription = @TLDescription WHERE  OrderId = @OrderId and Id = @UpdatedId";


                            using (SqlCommand updateCommand = new SqlCommand(updatesql, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@OrderId", updateOrderStatusByTLDTO.OrderId);
                                updateCommand.Parameters.AddWithValue("@Status", updateOrderStatusByTLDTO.Status);
                                updateCommand.Parameters.AddWithValue("@TLDescription", updateOrderStatusByTLDTO.TL_Description);
                                updateCommand.Parameters.AddWithValue("@UpdatedId", updateOrderStatusByTLDTO.UpdatedId);

                                updateCommand.ExecuteNonQuery();
                            }
                            resultDTO.StatusCode = "200";
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Order Details Updated Successfully";
                        }
                        else
                        {
                            resultDTO.StatusCode = "404";
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "The order has already moved to invoice, you can't update the status anymore.";
                        }
                    }
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "No Skillset table found for the given SkillsetId";
                }
            }
            catch (Exception ex)
            {
                resultDTO.StatusCode = "500";
                resultDTO.IsSuccess = false;
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public string GetDynamicColumns(SqlConnection connection, string tableName, string[] ExcludedColumns, ResultDTO resultDTO)
        {
            string GetDynamicColumnssql = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@Tablename";
            using (SqlCommand Colcommand = new SqlCommand(GetDynamicColumnssql, connection))
            {
                Colcommand.Parameters.AddWithValue("@Tablename", tableName);
                var Columns = new List<string>();

                using SqlDataAdapter dataAdapter = new(Colcommand);
                DataSet dataset = new DataSet();
                dataAdapter.Fill(dataset);

                var dynamiclist = string.Empty;

                if (dataset != null && dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                {
                    DataTable datatable = dataset.Tables[0];

                    dynamiclist = string.Join(",", datatable.AsEnumerable()
                                   .Select(row => row["COLUMN_NAME"].ToString())
                                   .Where(columnName => !ExcludedColumns.Contains(columnName)));

                    resultDTO.Message = "List of Order Details";
                    resultDTO.IsSuccess = true;
                }
                return dynamiclist;
            }
        }
        public ResultDTO GetUnassignedOrderInfo(UnassignedOrderInfoDTO unassignedOrderInfoDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                var skillset = (from tc in _oMTDataContext.TemplateColumns
                                join ss in _oMTDataContext.SkillSet on tc.SkillSetId equals ss.SkillSetId
                                join sr in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sr.SystemofRecordId
                                where tc.SkillSetId == unassignedOrderInfoDTO.SkillSetId && ss.IsActive
                                select new
                                {
                                    SkillSetName = ss.SkillSetName,
                                    SkillSetId = ss.SkillSetId,
                                    SystemofRecordId = ss.SystemofRecordId,
                                    SystemofRecordName = sr.SystemofRecordName
                                }).FirstOrDefault();

                if (skillset != null)
                {

                    // pending orders 
                    var columns1 = (from dt in _oMTDataContext.TemplateColumns
                                    where dt.SkillSetId == unassignedOrderInfoDTO.SkillSetId && dt.IsGetOrderColumn == true
                                    select dt.ColumnAliasName).ToList();

                    var columns2 = (from dt in _oMTDataContext.DefaultTemplateColumns
                                    where dt.SystemOfRecordId == skillset.SystemofRecordId && dt.IsGetOrderColumn == true
                                    select dt.DefaultColumnName).ToList();

                    // get date type columns 
                    var datecol = (from ss in _oMTDataContext.SkillSet
                                   join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                   where ss.SkillSetName == skillset.SkillSetName && dt.IsGetOrderColumn && dt.DataType == "Date"
                                   select dt.DefaultColumnName).ToList();
                    //Datetime
                    var datetimecol = (from ss in _oMTDataContext.SkillSet
                                       join dt in _oMTDataContext.DefaultTemplateColumns on ss.SystemofRecordId equals dt.SystemOfRecordId
                                       where ss.SkillSetName == skillset.SkillSetName && dt.IsGetOrderColumn && dt.DataType == "DateTime"
                                       select dt.DefaultColumnName).ToList();

                    var columns = (columns1 ?? Enumerable.Empty<string>()).Concat(columns2 ?? Enumerable.Empty<string>());

                    string selectedColumns = $"t1.Id, {string.Join(", ", columns.Select(c => $"t1.{c}"))}";  //included Id's


                    string query = $@"
                                   SELECT 
                                        {selectedColumns}, 
                                        '{skillset.SkillSetName}' AS SkillSetName, 
                                        {skillset.SkillSetId} AS SkillSetId , 
                                        '{skillset.SystemofRecordName}' AS SystemOfRecordName,
                                        {skillset.SystemofRecordId} AS SystemOfRecordId
                                    FROM 
                                        {skillset.SkillSetName} AS t1
                                   WHERE 
                                        t1.UserId IS NULL 
                                        AND (t1.Status IS NULL OR t1.Status = '') 
                                    ORDER BY 
                                        t1.Id";

                    using SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = query;

                    using SqlDataAdapter dataAdapter = new(cmd);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);

                    if (dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                    {
                        DataTable datatable = dataset.Tables[0];

                        var orderlist = datatable.AsEnumerable()
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
                                          return dateValue.ToString("MM-dd-yyyy");  // Format as date string
                                      }
                                      if (datetimecol.Contains(column.ColumnName) && column.DataType == typeof(DateTime))
                                      {
                                          if (row[column] == DBNull.Value)
                                          {
                                              return "";
                                          }
                                          DateTime dateValue = (DateTime)row[column];
                                          return dateValue.ToString("MM-dd-yyyy HH:mm:ss");  // Format as date & Time string
                                      }
                                      return row[column] == DBNull.Value ? "" : row[column];
                                  })).ToList();

                        resultDTO.Data = orderlist;
                        resultDTO.Message = "List of Unassigned Orders";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Unassigned Orders Not Found";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset Table Not Found";
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
        public ResultDTO UpdateUnassignedOrder(int userid, UpdateUnassignedOrderDTO updateUnassignedOrderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                var SkillsetTable = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == updateUnassignedOrderDTO.SkillsetId && x.SystemofRecordId == updateUnassignedOrderDTO.SystemofRecordId)
                                    .Select(x => x.SkillSetName).FirstOrDefault();

                string IdList = string.Join(",", updateUnassignedOrderDTO.Id);

                var Datetime = DateTime.UtcNow;

                string query = $"UPDATE {SkillsetTable} SET Status=@Status,Remarks =@Remarks,UserId=@UserId," +
                               $"CompletionDate=@CompletionDate,StartTime=@StartTime,EndTime=@EndTime WHERE Id IN ({IdList})";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Status", updateUnassignedOrderDTO.Status);
                command.Parameters.AddWithValue("@Remarks", updateUnassignedOrderDTO.Remarks);
                command.Parameters.AddWithValue("@UserId", userid);
                command.Parameters.AddWithValue("@CompletionDate", Datetime);
                command.Parameters.AddWithValue("@StartTime", Datetime);
                command.Parameters.AddWithValue("@EndTime", Datetime);
                
                command.ExecuteNonQuery();

                resultDTO.Message = "Unassigned Order Status Updated Successfully";
                resultDTO.StatusCode = "200";
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