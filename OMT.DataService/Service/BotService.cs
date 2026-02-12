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
                resultDTO.Message = "System Pending Orders have been successfully retrieved back to the queue";
                resultDTO.IsSuccess = true;
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
                // check if order is available in skillset table
                //string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                //using SqlConnection connection = new(connectionstring);

                //var checkquery = $@"SELECT * FROM @SkillSetName WHERE OrderId = @OrderId AND EndTime = @EndTime";

                //using SqlCommand chq = connection.CreateCommand();
                //chq.CommandText = checkquery;
                //chq.Parameters.AddWithValue("@OrderId", updateCheckindateBotRequestDTO.OrderId);
                //chq.Parameters.AddWithValue("EndTime", updateCheckindateBotRequestDTO.EndTime);  //convert to utc
                //chq.Parameters.AddWithValue("@SkillSetName", updateCheckindateBotRequestDTO.SkillSetName);

                //using SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                //DataSet dataset = new DataSet();
                //sqlDataAdapter.Fill(dataset);

                //DataTable datatable = dataset.Tables[0];

                //var querydt = datatable.AsEnumerable()
                //                     .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                //                         column => column.ColumnName,
                //                         column => row[column] == DBNull.Value ? "" : row[column])).ToList();



                // if order found update the checkindate in skillset table

                // get current datetime
                // check if given checkin dates invoice has run or not and if that order is not available in invoice for checkindate = invoicedeletedate



                resultDTO.Message = "Checkin date has beenn successfully updated";
                resultDTO.IsSuccess = true;
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
