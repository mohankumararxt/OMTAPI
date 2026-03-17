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
using System.Globalization;
using System.Net.Http.Json;

namespace OMT.DataService.Service
{
    public class BotService : IBotService
    {
        private readonly OMTDataContext _oMTDataContext;

        public BotService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetSkillSetListBySOR(string sorname)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                      join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                      join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup
                                      from hs in hsGroup.DefaultIfEmpty()
                                      where sor.IsActive && ss.IsActive && sor.SystemofRecordName == sorname // Filter by the specific SOR ID only Actives
                                      group new { ss, sor, hs } by new
                                      {
                                          ss.SkillSetId,
                                          ss.SkillSetName,
                                          ss.Threshold,
                                          sor.SystemofRecordName,
                                          ss.SystemofRecordId,
                                          ss.IsHardState,
                                          ss.GetOrderByProject
                                      } into grp
                                      select new SkillSetResponseDTO
                                      {
                                          SkillSetId = grp.Key.SkillSetId,
                                          SkillSetName = grp.Key.SkillSetName,
                                          Threshold = grp.Key.Threshold,
                                          SystemofRecordName = grp.Key.SystemofRecordName,
                                          SystemofRecordId = grp.Key.SystemofRecordId,
                                          IsHardState = grp.Any(x => x.hs != null && x.hs.IsActive), //null  Isactive 
                                          StateName = grp
                                                            .Where(x => x.hs != null && x.hs.IsActive)
                                                            .Select(x => x.hs.StateName)
                                                            .ToArray(),  //Isactive only
                                          GetOrderByProject = grp.Key.GetOrderByProject
                                      })
                                      .OrderBy(x => x.SystemofRecordId)
                                      .ThenBy(x => x.SkillSetName)
                                      .ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = skillSetGroups;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSkillSetListBySORId(int sorid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                      join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                      join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup
                                      from hs in hsGroup.DefaultIfEmpty()
                                      where sor.IsActive && ss.IsActive && sor.SystemofRecordId == sorid // Filter by the specific SOR ID only Actives
                                      group new { ss, sor, hs } by new
                                      {
                                          ss.SkillSetId,
                                          ss.SkillSetName,
                                          ss.Threshold,
                                          sor.SystemofRecordName,
                                          ss.SystemofRecordId,
                                          ss.IsHardState,
                                          ss.GetOrderByProject
                                      } into grp
                                      select new SkillSetResponseDTO
                                      {
                                          SkillSetId = grp.Key.SkillSetId,
                                          SkillSetName = grp.Key.SkillSetName,
                                          Threshold = grp.Key.Threshold,
                                          SystemofRecordName = grp.Key.SystemofRecordName,
                                          SystemofRecordId = grp.Key.SystemofRecordId,
                                          IsHardState = grp.Any(x => x.hs != null && x.hs.IsActive), //null  Isactive 
                                          StateName = grp
                                                            .Where(x => x.hs != null && x.hs.IsActive)
                                                            .Select(x => x.hs.StateName)
                                                            .ToArray(),  //Isactive only
                                          GetOrderByProject = grp.Key.GetOrderByProject
                                      })
                                      .OrderBy(x => x.SystemofRecordId)
                                      .ThenBy(x => x.SkillSetName)
                                      .ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = skillSetGroups;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSORList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<SystemofRecordResponseDTO> sorlist = _oMTDataContext.SystemofRecord.Where(x => x.IsActive).Select(_ => new SystemofRecordResponseDTO() { SystemofRecordId = _.SystemofRecordId, SystemofRecordName = _.SystemofRecordName }).OrderBy(x => x.SystemofRecordName).ToList();
                if (sorlist != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = sorlist;
                    resultDTO.Message = "System of Record List.";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = sorlist;
                    resultDTO.Message = "System of Record is empty";
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


        //private readonly HttpClient _httpClient;
        //private readonly IActivityLogger _logger;

        //public BotService(
        //    OMTDataContext oMTDataContext,
        //    HttpClient httpClient,
        //    IActivityLogger logger)
        //{
        //    _oMTDataContext = oMTDataContext;
        //    _httpClient = httpClient;
        //    _logger = logger;
        //}

        //public async Task<string> CallCandidateApiAsync(string name)
        //{
        //    // Log input before calling API
        //    _logger.Log($"BotService calling Candidate API with input: {name}");

        //    var response = await _httpClient.PostAsJsonAsync(
        //        "https://omtapidev.azurewebsites.net/api/candidate/create",
        //        name
        //    );

        //    var result = await response.Content.ReadAsStringAsync();

        //    _logger.Log($"BotService received response: {result}");

        //    return result;
        //}

        public ResultDTO Retrieve_SystemPending_Orders(RetrieveSystemPendingOrdersRequsetDTO retrieveSystemPendingOrdersRequsetDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                if (!string.Equals(retrieveSystemPendingOrdersRequsetDTO.SystemOfRecordName,"SCI",StringComparison.OrdinalIgnoreCase))
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Other system of records are not applicable.";
                    resultDTO.StatusCode = "404";
                }
                else
                {

                    var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == retrieveSystemPendingOrdersRequsetDTO.SkillSetName && x.IsActive).FirstOrDefault();
                    var statusid = _oMTDataContext.ProcessStatus.Where(x => x.SystemOfRecordId == skillset.SystemofRecordId && x.Status == "System-Pending" && x.IsActive).Select(x => x.Id).FirstOrDefault();

                    DateTime scheduledtime = retrieveSystemPendingOrdersRequsetDTO.Scheduled_Datetime.AddHours(-5).AddMinutes(-30);

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                    using SqlConnection connection = new(connectionstring);
                    connection.Open();

                    // check if orders went to system pending in skillset table with completiondate 

                    var checkquery = $@"SELECT COUNT(*) FROM {retrieveSystemPendingOrdersRequsetDTO.SkillSetName} WHERE CompletionDate >= @CompletionDate AND CompletionDate < DATEADD(MINUTE, 1, @CompletionDate) AND UserId IS NULL AND STATUS = @statusid AND CompletionDate IS NOT NULL";

                    using SqlCommand chq = connection.CreateCommand();
                    chq.CommandText = checkquery;
                    chq.Parameters.AddWithValue("@CompletionDate", scheduledtime);
                    chq.Parameters.AddWithValue("@statusid", statusid);

                    int ordercount = Convert.ToInt32(chq.ExecuteScalar());

                    if (ordercount > 0)
                    {
                        var updatequery = $@"UPDATE {retrieveSystemPendingOrdersRequsetDTO.SkillSetName} SET Status = NULL, CompletionDate = NULL WHERE UserId IS NULL AND STATUS = @statusid AND CompletionDate >= @CompletionDate AND CompletionDate < DATEADD(MINUTE, 1, @CompletionDate)";

                        SqlCommand updateToPN = new SqlCommand(updatequery, connection);
                        updateToPN.CommandType = CommandType.Text;

                        updateToPN.Parameters.AddWithValue("@statusid", statusid);
                        updateToPN.Parameters.AddWithValue("@CompletionDate", scheduledtime);

                        updateToPN.ExecuteNonQuery();

                        resultDTO.Message = "System Pending Orders have been successfully retrieved back to the queue";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.Message = "No orders have gone to System Pending in the given scheduled time, please check the details";
                        resultDTO.IsSuccess = false;
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

        //public ResultDTO Retrieve_SystemPending_Orders(RetrieveSystemPendingOrdersRequsetDTO retrieveSystemPendingOrdersRequsetDTO)
        //{
        //    _logger.Log("Retrieve_SystemPending_Orders API called");

        // Log full input object as JSON
        //string inputJson = JsonConvert.SerializeObject(retrieveSystemPendingOrdersRequsetDTO);
        //_logger.Log("Input Data: " + inputJson);

        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

        //    try
        //    {
        //        resultDTO.Message = "System Pending Orders have been successfully retrieved back to the queue";
        //        resultDTO.IsSuccess = true;

        //        _logger.Log("Retrieve_SystemPending_Orders executed successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log("Error: " + ex.Message);

        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = ex.Message;
        //    }

        //    return resultDTO;
        //}

        public ResultDTO Update_Checkindate_Bot(UpdateCheckindateBotRequestDTO updateCheckindateBotRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {


                // allow only TRD orders

                if (!string.Equals(updateCheckindateBotRequestDTO.SystemOfRecordName, "TRD", StringComparison.OrdinalIgnoreCase))
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Other system of records are not applicable.";
                    resultDTO.StatusCode = "404";
                }

                else
                {
                    // check if order is available in skillset table
                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                    using SqlConnection connection = new(connectionstring);
                    connection.Open();


                    var skillsetid = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == updateCheckindateBotRequestDTO.SkillSetName && x.IsActive).FirstOrDefault();

                    DateTime endtime = updateCheckindateBotRequestDTO.EndTime.AddHours(-5).AddMinutes(-30);

                    var checkquery = $@"SELECT * FROM {updateCheckindateBotRequestDTO.SkillSetName} WHERE OrderId = @OrderId AND DATEDIFF(SECOND, EndTime, @EndTime) = 0 AND UserId IS NOT NULL AND STATUS IS NOT NULL";

                    using SqlCommand chq = connection.CreateCommand();
                    chq.CommandText = checkquery;
                    chq.Parameters.AddWithValue("@OrderId", updateCheckindateBotRequestDTO.OrderId);
                    chq.Parameters.AddWithValue("@EndTime", endtime);

                    using SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(chq);
                    DataSet dataset = new DataSet();
                    sqlDataAdapter.Fill(dataset);

                    if (dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                    {
                        DataTable datatable = dataset.Tables[0];

                        DateTime? checkinDate = datatable.AsEnumerable()
                            .Select(row => row.Field<DateTime?>("checkin_date"))
                            .FirstOrDefault();

                        // if order found update the checkindate in skillset table
                        if (checkinDate.HasValue)
                        {
                            var updatecheckin = $@"UPDATE {updateCheckindateBotRequestDTO.SkillSetName} SET CheckIn_date = @CheckIn_date WHERE OrderId = @OrderId AND DATEDIFF(SECOND, EndTime, @EndTime) = 0 AND UserId IS NOT NULL AND STATUS IS NOT NULL";

                            using SqlCommand upd = connection.CreateCommand();
                            upd.CommandText = updatecheckin;
                            upd.Parameters.AddWithValue("@OrderId", updateCheckindateBotRequestDTO.OrderId);
                            upd.Parameters.AddWithValue("EndTime", updateCheckindateBotRequestDTO.EndTime.AddHours(-5).AddMinutes(-30));  //convert to utc
                            upd.Parameters.AddWithValue("@CheckIn_date", updateCheckindateBotRequestDTO.CheckIn_date);

                            upd.ExecuteNonQuery();

                            // get current datetime and check if its > 6.30 pm, if yes check if order went to invoice with 

                            DateTime currentdatetime = DateTime.UtcNow;
                            DateTime todayUtc = DateTime.UtcNow.Date; // Today at midnight in UTC
                            DateTime invtime = todayUtc.AddHours(13); // Today 6:30 PM UTC
                            DateTime yesterday_checkindate = todayUtc.AddDays(-1);


                            // check if endtime is lesser than invoice run time , if yes no need to do anything in invoice

                            //if (endtime < invtime && currentdatetime < invtime )
                            //{
                            string inv_query = $@"SELECT InvoiceDeleteDate,InvoiceDumpId FROM InvoiceDump WHERE Skillset = '{updateCheckindateBotRequestDTO.SkillSetName}' and OrderId = @OrderId and DATEDIFF(SECOND, EndTime, @EndTime) = 0";

                            using SqlCommand inv = connection.CreateCommand();
                            inv.CommandText = inv_query;
                            inv.Parameters.AddWithValue("@OrderId", updateCheckindateBotRequestDTO.OrderId);
                            inv.Parameters.AddWithValue("EndTime", updateCheckindateBotRequestDTO.EndTime.AddHours(-5).AddMinutes(-30));  //convert to utc


                            using SqlDataAdapter sqlDataAdapter2 = new SqlDataAdapter(inv);
                            DataSet dataset2 = new DataSet();
                            sqlDataAdapter2.Fill(dataset2);

                            if (dataset2.Tables.Count > 0 && dataset2.Tables[0].Rows.Count > 0)
                            {
                                DataTable datatable2 = dataset2.Tables[0];

                                DateTime? InvoiceDeleteDate = datatable2.AsEnumerable().Select(row => row.Field<DateTime?>("InvoiceDeleteDate")).FirstOrDefault();

                                int InvoiceDumpId = datatable2.AsEnumerable().Select(row => row.Field<int>("InvoiceDumpId")).FirstOrDefault();


                                if (InvoiceDeleteDate.HasValue)
                                {
                                    // if new checkindate >  yesterday_checkindate - delete from invoice

                                    if (updateCheckindateBotRequestDTO.CheckIn_date > yesterday_checkindate)
                                    {
                                        var deletequery = $@"DELETE FROM InvoiceDump WHERE InvoiceDumpId = @InvoiceDumpId";

                                        using SqlCommand del = connection.CreateCommand();
                                        del.CommandText = deletequery;
                                        del.Parameters.AddWithValue("@InvoiceDumpId", InvoiceDumpId);

                                        del.ExecuteNonQuery();

                                    }

                                    // if new checkindate < yesterday_checkindate - update completion date in invoice

                                    else if (updateCheckindateBotRequestDTO.CheckIn_date < yesterday_checkindate)
                                    {
                                        var updinvquery = $@"UPDATE InvoiceDump SET CompletionDate = @CheckIn_date WHERE InvoiceDumpId = @InvoiceDumpId";

                                        using SqlCommand upinv = connection.CreateCommand();
                                        upinv.CommandText = updinvquery;
                                        upinv.Parameters.AddWithValue("@InvoiceDumpId", InvoiceDumpId);
                                        upinv.Parameters.AddWithValue("@CheckIn_date", updateCheckindateBotRequestDTO.CheckIn_date);

                                        upinv.ExecuteNonQuery();
                                    }

                                    resultDTO.Message = "Checkin_Date has been changed for the given order.";

                                }
                            }
                            else
                            {
                                // if new checkin date = yesterday_checkindate and endtime < invtime and currentdatetime < invtime - no change
                                //if new checkin date > todayUtc - no change

                                // if new checkindate < yesterday_checkindate  - insert into invoice or if new checkin date = yesterday_checkindate and endtime < invtime and currentdatetime > invtime - insert into invoice

                                if ((updateCheckindateBotRequestDTO.CheckIn_date < yesterday_checkindate) || (updateCheckindateBotRequestDTO.CheckIn_date == yesterday_checkindate && endtime < invtime && currentdatetime > invtime))
                                {
                                    using SqlCommand insertbysp = new()
                                    {
                                        Connection = connection,
                                        CommandType = CommandType.StoredProcedure,
                                        CommandText = "Insert_Missing_InvoiceDump"
                                    };
                                    insertbysp.Parameters.AddWithValue("@YesterdayDate", updateCheckindateBotRequestDTO.CheckIn_date);
                                    insertbysp.Parameters.AddWithValue("@SkillSetId", skillsetid.SkillSetId);
                                    insertbysp.Parameters.AddWithValue("@SkillSetName", updateCheckindateBotRequestDTO.SkillSetName);
                                    insertbysp.Parameters.AddWithValue("@EndTime1", endtime);
                                    insertbysp.Parameters.AddWithValue("@EndTime2", endtime);
                                    SqlParameter returnValue = new()
                                    {
                                        ParameterName = "@RETURN_VALUE",
                                        Direction = ParameterDirection.ReturnValue
                                    };
                                    insertbysp.Parameters.Add(returnValue);

                                    insertbysp.ExecuteNonQuery();

                                    int returnCode = (int)insertbysp.Parameters["@RETURN_VALUE"].Value;

                                    if (returnCode != 1)
                                    {
                                        throw new InvalidOperationException("Error encountered while inserting in invoicedump table,please try again");
                                    }
                                }

                                resultDTO.Message = "Checkin_Date has been changed for the given order.";

                            }

                        }
                        else
                        {
                            resultDTO.Message = "Checkin date not found for the given order, please check the order details";
                            resultDTO.IsSuccess = false;
                            resultDTO.StatusCode = "404";
                        }

                    }
                    else
                    {
                        resultDTO.Message = "Order not found, please check the order details";
                        resultDTO.IsSuccess = false;
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
