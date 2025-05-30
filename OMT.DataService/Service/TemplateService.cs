﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using OMT.DataService.Settings;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;


namespace OMT.DataService.Service
{
    public class TemplateService : ITemplateService
    {
        private readonly OMTDataContext _oMTDataContext;

        private readonly IOptions<HastatusSettings> _hastatusSettings;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        private readonly IOptions<TrdStatusSettings> _authSettings;
        private readonly IOptions<EmailDetailsSettings> _emailDetailsSettings;
        private readonly IConfiguration _configuration;
        public TemplateService(OMTDataContext oMTDataContext, IOptions<TrdStatusSettings> authSettings, IOptions<EmailDetailsSettings> emailDetailsSettings, IConfiguration configuration, IOptions<HastatusSettings> hastatusSettings)
        {
            _oMTDataContext = oMTDataContext;
            _authSettings = authSettings;
            _emailDetailsSettings = emailDetailsSettings;
            _configuration = configuration;
            _hastatusSettings = hastatusSettings;
        }
        public ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            //using (var dbContextTransaction = _oMTDataContext.Database.BeginTransaction())
            //{
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == createTemplateDTO.SkillsetId && x.SystemofRecordId == createTemplateDTO.SystemofRecordId && x.IsActive).FirstOrDefault();

                List<string> defcol = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == createTemplateDTO.SystemofRecordId).Select(x => x.DefaultColumnName).ToList();

                string dcol = string.Join(", ", defcol);

                if (skillSet != null)
                {
                    TemplateColumns template = _oMTDataContext.TemplateColumns.Where(x => x.SystemOfRecordId == createTemplateDTO.SystemofRecordId && x.SkillSetId == createTemplateDTO.SkillsetId).FirstOrDefault();
                    if (template != null)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Template Name already exists.";
                    }
                    else
                    {
                        //check if col name has any keywords

                        var keywordlist = _oMTDataContext.Keywordstable.Where(x => x.IsActive).Select(_ => _.Keywordname).ToList();

                        var notallowed = createTemplateDTO.TemplateColumns.Any(x => keywordlist.Contains(x.ColumnName, StringComparer.OrdinalIgnoreCase));

                        var matching = createTemplateDTO.TemplateColumns.Where(x => keywordlist.Contains(x.ColumnName, StringComparer.OrdinalIgnoreCase)).Select(x => x.ColumnName).ToList();

                        // check if user has added default columns while creating template

                        var twiceadd = createTemplateDTO.TemplateColumns.Any(x => defcol.Contains(x.ColumnName, StringComparer.OrdinalIgnoreCase));

                        var matchingdefcol = createTemplateDTO.TemplateColumns.Where(x => defcol.Contains(x.ColumnName, StringComparer.OrdinalIgnoreCase)).Select(x => x.ColumnName).ToList();

                        if (notallowed && twiceadd)
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Template creation doesn't allow certain keywords such as "
                                                + string.Join(", ", matching)
                                                + " to be set as column name. Use different column name(s). \n"
                                                + "Also "
                                                + string.Join(", ", matchingdefcol)
                                                + " can't be added twice. It's already set as default column(s).";
                        }

                        else if (!notallowed && twiceadd)
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = string.Join(", ", matchingdefcol) + " can't be added twice. It's already set as default column(s).";
                        }

                        else if (notallowed && !twiceadd)
                        {
                            resultDTO.Message = "Template creation doesn't allow certain keywords such as "
                                               + string.Join(", ", matching)
                                               + " to be set as column name. Use different column name(s).";
                        }

                        else
                        {
                            if (createTemplateDTO.TemplateColumns.Any())
                            {
                                foreach (TemplateColumnDTO templateColumns in createTemplateDTO.TemplateColumns)
                                {
                                    TemplateColumns newtemplateColumns = new TemplateColumns()
                                    {
                                        SkillSetId = createTemplateDTO.SkillsetId,
                                        SystemOfRecordId = createTemplateDTO.SystemofRecordId,
                                        ColumnAliasName = templateColumns.ColumnName.Replace(" ", "_"),
                                        ColumnName = templateColumns.ColumnName,
                                        ColumnDataType = templateColumns.ColumnDataType,
                                        IsDuplicateCheck = templateColumns.IsDuplicateCheck,
                                        IsGetOrderColumn = templateColumns.IsGetOrderColumn,
                                    };
                                    _oMTDataContext.TemplateColumns.Add(newtemplateColumns);
                                }
                                _oMTDataContext.SaveChanges();
                            }

                            string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                            using SqlConnection connection = new(connectionstring);
                            using SqlCommand command = new()
                            {
                                Connection = connection,
                                CommandType = CommandType.StoredProcedure,
                                CommandText = "CreateTemplate"
                            };
                            command.Parameters.AddWithValue("@SkillsetId", createTemplateDTO.SkillsetId);
                            command.Parameters.AddWithValue("@SystemofRecordId", createTemplateDTO.SystemofRecordId);
                            SqlParameter returnValue = new()
                            {
                                ParameterName = "@RETURN_VALUE",
                                Direction = ParameterDirection.ReturnValue
                            };
                            command.Parameters.Add(returnValue);
                            connection.Open();
                            command.ExecuteNonQuery();

                            int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;
                            resultDTO.Message = "Template Added Successfully.";
                            if (returnCode != 1)
                            {
                                throw new InvalidOperationException("Error encountered while creating template,please try again");
                            }
                            // Commit transaction
                            // dbContextTransaction.Commit();

                        }
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset not exists.";
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction
                // dbContextTransaction.Rollback();
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            //}
            return resultDTO;
        }
        public ResultDTO DeleteTemplate(int SkillSetId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == SkillSetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> columns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();
                    _oMTDataContext.TemplateColumns.RemoveRange(columns);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = "Drop table " + skillSet.SkillSetName
                    };
                    connection.Open();
                    command.ExecuteNonQuery();


                    var reportcolumns = _oMTDataContext.ReportColumns.Where(x => x.SkillSetId == SkillSetId).ToList();

                    if (reportcolumns.Count > 0)
                    {
                        using SqlConnection repcol = new(connectionstring);
                        using SqlCommand repcolcmd = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.Text,
                            CommandText = "Delete from ReportColumns where skillsetid = @SkillSetId"
                        };

                        repcolcmd.Parameters.AddWithValue("@SkillSetId", SkillSetId);
                        repcolcmd.ExecuteNonQuery();
                    }
                    resultDTO.Message = "Template Deleted Successfully";
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
        public ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);

                var url = _emailDetailsSettings.Value.SendEmailURL;

                var insertedJsonobject = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(uploadTemplateDTO.JsonData);
                var records = insertedJsonobject["Records"];
                var NoOfOrders = records.Count;
                var HasPriorityOrder = records.Any(record => record.IsPriority == 1); //check for priority orders presence

                string uploadedDate = DateTime.UtcNow.ToString("MM-dd-yyyy HH:mm:ss");

                for (int i = 0; i < records.Count; i++)
                {
                    // Cast each item to JObject so we can add new properties
                    var item = records[i] as JObject;
                    if (item != null)
                    {
                        item["UploadedBy"] = userid;
                        item["UploadedDate"] = uploadedDate;
                    }
                }

                string updatedJsonData = JsonConvert.SerializeObject(insertedJsonobject);

                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    TemplateColumns template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == uploadTemplateDTO.SkillsetId).FirstOrDefault();
                    if (template != null)
                    {
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "InsertData"
                        };
                        command.Parameters.AddWithValue("@SkillSetId", uploadTemplateDTO.SkillsetId);
                        command.Parameters.AddWithValue("@jsonData", updatedJsonData);

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

                        // send details about uploaded orders via mail

                        DateTime uploadedate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                        string firstname = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.FirstName).FirstOrDefault();
                        string lastname = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.LastName).FirstOrDefault();
                        string username = string.Join(' ', firstname, lastname);


                        IConfigurationSection toEmailId = _configuration.GetSection("EmailConfig:UploadorderAPIdetails:ToEmailId");

                        List<string> toEmailIds1 = toEmailId.AsEnumerable()
                                                                  .Where(c => !string.IsNullOrEmpty(c.Value))
                                                                  .Select(c => c.Value)
                                                                  .ToList();

                        var uploaddetails = $"{username} has uploaded {NoOfOrders} orders in {skillSet.SkillSetName} at {uploadedate}";
                        // var uploaddetails = $"<b>{username}</b> has uploaded <b>{NoOfOrders}</b> orders in \"<b>{skillSet.SkillSetName}</b>\" at <b>{uploadedate}</b>";


                        SendEmailDTO sendEmailDTO1 = new SendEmailDTO
                        {
                            ToEmailIds = toEmailIds1,
                            Subject = "Orders uploaded",
                            Body = uploaddetails,
                        };
                        try
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO1);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");

                                var webApiUrl = new Uri(url);
                                var response = client.PostAsync(webApiUrl, content).Result;

                                if (response.IsSuccessStatusCode)
                                {
                                    var responseData = response.Content.ReadAsStringAsync().Result;

                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }



                        if (skillSet.SystemofRecordId == 2 || skillSet.SystemofRecordId == 4)
                        {
                            var message = "";
                            List<int> ExistingResWareProductDescriptionIds = new List<int>();
                            List<string> ExistingResWareProductDescriptionName = new List<string>();

                            var distinctResWareProductDescriptions = records.Select(r => (string)r.ResWareProductDescriptions).Distinct().ToList();

                            var NewResWareProductDescriptions = distinctResWareProductDescriptions.Where(rpd => !_oMTDataContext.ResWareProductDescriptions
                                                                                                  .Any(t1 => t1.ResWareProductDescriptionName == rpd)).ToList();

                            foreach (var kvp in distinctResWareProductDescriptions)
                            {
                                var rpdid = _oMTDataContext.ResWareProductDescriptions.Where(x => x.ResWareProductDescriptionName == kvp).FirstOrDefault();

                                if (rpdid != null)
                                {
                                    ExistingResWareProductDescriptionIds.Add(rpdid.ResWareProductDescriptionId);
                                    ExistingResWareProductDescriptionName.Add(rpdid.ResWareProductDescriptionName);
                                }
                            }

                            var NotMappedToSkillset = (from erpd in ExistingResWareProductDescriptionName
                                                       join rpd in _oMTDataContext.ResWareProductDescriptions on erpd equals rpd.ResWareProductDescriptionName
                                                       where !_oMTDataContext.ResWareProductDescriptionMap
                                                           .Any(rpdm => rpdm.ResWareProductDescriptionId == rpd.ResWareProductDescriptionId
                                                                        && rpdm.SkillSetId == skillSet.SkillSetId)
                                                       select erpd).Distinct().ToList();



                            if (NewResWareProductDescriptions.Count > 0 && NotMappedToSkillset.Count == 0)
                            {
                                if (skillSet.SystemofRecordId == 2)
                                {
                                    message = $"Please add the following new ResWare Product Descriptions: {string.Join(", ", NewResWareProductDescriptions)}, and map them to the skillset \"{skillSet.SkillSetName}\" in OMT.";
                                }

                                else if (skillSet.SystemofRecordId == 4)
                                {
                                    message = $"Please add the following new Tiqe Product Descriptions: {string.Join(", ", NewResWareProductDescriptions)}, and map them to the skillset \"{skillSet.SkillSetName}\" in OMT.";
                                }
                            }

                            else if (NewResWareProductDescriptions.Count == 0 && NotMappedToSkillset.Count > 0)
                            {
                                if (skillSet.SystemofRecordId == 2)
                                {
                                    message = $"Please map the following ResWare Product Descriptions: {string.Join(", ", NotMappedToSkillset)} to the skillset \"{skillSet.SkillSetName}\" in OMT.";
                                }
                                else if (skillSet.SystemofRecordId == 4)
                                {
                                    message = $"Please map the following Tiqe Product Descriptions: {string.Join(", ", NotMappedToSkillset)} to the skillset \"{skillSet.SkillSetName}\" in OMT.";

                                }
                            }

                            else if (NewResWareProductDescriptions.Count > 0 && NotMappedToSkillset.Count > 0)
                            {
                                if (skillSet.SystemofRecordId == 2)
                                {
                                    message = $"Please add the following new ResWare Product Descriptions: {string.Join(", ", NewResWareProductDescriptions)}, and map the following: {string.Join(", ", NotMappedToSkillset)}, {string.Join(", ", NewResWareProductDescriptions)} to the skillset \"{skillSet.SkillSetName}\" in OMT.";
                                }

                                else if (skillSet.SystemofRecordId == 4)
                                {
                                    message = $"Please add the following new Tiqe Product Descriptions: {string.Join(", ", NewResWareProductDescriptions)}, and map the following: {string.Join(", ", NotMappedToSkillset)}, {string.Join(", ", NewResWareProductDescriptions)} to the skillset \"{skillSet.SkillSetName}\" in OMT.";
                                }
                            }

                            if (!string.IsNullOrEmpty(message))
                            {
                                string subject = "";

                                if (skillSet.SystemofRecordId == 2)
                                {
                                    subject = "Invoice - resware product description";
                                }
                                else if (skillSet.SystemofRecordId == 4)
                                {
                                    subject = "Invoice - tiqe product description";
                                }
                                //  var url = _emailDetailsSettings.Value.SendEmailURL;
                                IConfigurationSection toEmailIdSection = _configuration.GetSection("EmailConfig:UploadorderAPI:ToEmailId");

                                List<string> toEmailIds = toEmailIdSection.AsEnumerable()
                                                                          .Where(c => !string.IsNullOrEmpty(c.Value))
                                                                          .Select(c => c.Value)
                                                                          .ToList();


                                SendEmailDTO sendEmailDTO = new SendEmailDTO
                                {
                                    ToEmailIds = toEmailIds,
                                    Subject = subject,
                                    Body = message,
                                };

                                try
                                {
                                    using (HttpClient client = new HttpClient())
                                    {
                                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO);
                                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                                        var webApiUrl = new Uri(url);
                                        var response = client.PostAsync(webApiUrl, content).Result;

                                        if (response.IsSuccessStatusCode)
                                        {
                                            var responseData = response.Content.ReadAsStringAsync().Result;

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw;
                                }
                            }
                        }

                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Order uploaded successfully";

                        //if (skillSet.SkillSetName.Equals("RIC", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    HaStatusDTO haStatusDTO = new HaStatusDTO()
                        //    {
                        //        isauto = 0
                        //    };

                        //    CallAsyncHaStatusAPI(haStatusDTO);
                        //}
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Template doesnt exist for the given skillsetid,please create one to upload";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
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

        public void CallAsyncHaStatusAPI(HaStatusDTO haStatusDTO)
        {
            // Fire-and-forget, ensuring the main thread is not blocked
            _ = SendPostRequest_RIC_Async(haStatusDTO);
        }

        private async Task SendPostRequest_RIC_Async(HaStatusDTO haStatusDTO)
        {
            var url = _hastatusSettings.Value.HaUrl;

            try
            {

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(haStatusDTO);

                var content = new StringContent(json, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                //if (response.IsSuccessStatusCode)
                //{
                //    var responseData = await response.Content.ReadAsStringAsync();

                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling API: " + ex.Message);
            }
        }
        public ResultDTO ValidateOrders(ValidateOrderDTO validateorderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == validateorderDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == validateorderDTO.SkillsetId).ToList();
                    if (template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;

                        List<string> listofcolumns1 = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsDuplicateCheck).Select(_ => _.DefaultColumnName).ToList();
                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                        List<string> combinedList = listofcolumns1.Concat(listofColumns).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(validateorderDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        //sql query
                        string isDuplicateColumns1 = listofcolumns1 != null ? string.Join(",", listofcolumns1) : "";
                        string isDuplicateColumns = listofColumns != null ? string.Join(",", listofColumns) : "";

                        // Combine the strings, ensuring that if any of them is null, it's selected without a comma
                        string combinedString = (isDuplicateColumns1 != "" && isDuplicateColumns != "") ? isDuplicateColumns1 + "," + isDuplicateColumns : isDuplicateColumns1 + isDuplicateColumns;

                        string sql = $"SELECT CASE WHEN t.UserId IS NULL THEN '' ELSE CONCAT(up.FirstName, ' ', up.LastName, '(',up.EmployeeId, ')') END AS UserName, {combinedString}, " +
                                     $"ISNULL(ps.Status, '') AS Status, " +
                                     $"ISNULL(CONVERT(VARCHAR(10), t.CompletionDate, 120), '') AS CompletionDate, " +
                                     $"ISNULL(CONVERT(VARCHAR(19), t.StartTime, 120), '') AS StartTime, " +
                                     $"ISNULL(CONVERT(VARCHAR(19), t.EndTime, 120), '') AS EndTime " +
                                     $"FROM {tablename} t " +
                                     $"LEFT JOIN UserProfile up ON t.UserId = up.UserId " +
                                     $"LEFT JOIN ProcessStatus ps ON t.Status = ps.Id AND t.SystemOfRecordId = ps.SystemOfRecordId WHERE ";

                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in combinedList)
                            {
                                string columndata = records.Value<string>(columnname);

                                query += $"[{columnname}] = '{columndata}' AND ";
                            }

                            query = query.Substring(0, query.Length - 5);
                            query += ") OR ";

                            sql += query;
                        }

                        sql = sql.Substring(0, sql.Length - 4);

                        //execute sql query to fetch records from table
                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);

                        DataSet dataset = new DataSet();
                        connection.Open();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        if (querydt.Count > 0)
                        {
                            ValidationResponseDTO validationResponseDTO = new ValidationResponseDTO()
                            {
                                DuplicateCheckColumns = combinedString,
                                DuplicateOrders = querydt
                            };
                            resultDTO.IsSuccess = true;
                            resultDTO.Data = validationResponseDTO;
                            resultDTO.Message = "Duplicate records found,please verify before uploading";
                        }
                        else
                        {
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "No dulpicate records found, proceed to upload";
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Template doesn't exist for the given skillsetid,please create one to upload";
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
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
        public ResultDTO GetTemplateList()
        {

            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TemplateListDTO> templateList = new List<TemplateListDTO>();

                List<SkillSet> skillSets = (from ss in _oMTDataContext.SkillSet
                                            join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                            where ss.IsActive == true
                                            select ss).Distinct().OrderBy(ss => ss.SkillSetName).ToList();

                List<TemplateColumns> templatecolumns = _oMTDataContext.TemplateColumns.ToList();

                if (skillSets.Count > 0)
                {
                    foreach (SkillSet skillset in skillSets)
                    {
                        TemplateListDTO templateListDTO = new TemplateListDTO();
                        templateListDTO.SkillsetId = skillset.SkillSetId;
                        templateListDTO.SkillSetName = skillset.SkillSetName;
                        templateListDTO.SystemofRecordId = skillset.SystemofRecordId;
                        List<TemplateColumnDTO> TemplateColumns = templatecolumns.Where(x => x.SkillSetId == skillset.SkillSetId).
                                                            Select(_ => new TemplateColumnDTO() { ColumnDataType = _.ColumnDataType, ColumnName = _.ColumnAliasName, IsDuplicateCheck = _.IsDuplicateCheck }).ToList();

                        List<DefaultTemplateColumnlistDTO> defaultTemplateColumns = _oMTDataContext.DefaultTemplateColumns
                                                                                    .Where(x => x.SystemOfRecordId == skillset.SystemofRecordId && x.IsDefOnlyColumn)
                                                                                    .Select(_ => new DefaultTemplateColumnlistDTO()
                                                                                    {
                                                                                        DataType = _.DefaultColumnName == "IsPriority" ? "bit" : _.DataType,
                                                                                        DefaultColumnName = _.DefaultColumnName,
                                                                                        IsDuplicateCheck = _.IsDuplicateCheck
                                                                                    }).ToList();

                        var finalcolumns = TemplateColumns.
                                            Concat(defaultTemplateColumns.
                                            Select(dc => new TemplateColumnDTO
                                            {
                                                ColumnDataType = dc.DataType,
                                                ColumnName = dc.DefaultColumnName,
                                                IsDuplicateCheck = dc.IsDuplicateCheck
                                            })).ToList();

                        templateListDTO.TemplateColumns = finalcolumns;
                        templateList.Add(templateListDTO);
                    }
                    resultDTO.Data = templateList;
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

        public ResultDTO GetOrders(int userid)
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

                    //var querydt1 = datatable.AsEnumerable()
                    //                .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                    //                    column => column.ColumnName,
                    //                    column => row[column])).ToList();

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
                    List<UserSkillSet> userskillsetlist = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsActive).ToList();
                    List<UserSkillSet> hardstateid = userskillsetlist.Where(x => x.IsHardStateUser && x.IsPrimary).ToList();
                    if (hardstateid.Count > 0)
                    {
                        string uporder;
                        using SqlCommand command = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetOrderByHardstate"
                        };
                        command.Parameters.AddWithValue("@userid", userid);
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
                        if (!string.IsNullOrWhiteSpace(uporder))
                        {
                            resultDTO.Data = uporder;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Order assigned successfully";
                        }
                        else
                        {
                            callSPbyPrimary(userid, resultDTO, connection);
                        }
                    }
                    else
                    {
                        callSPbyPrimary(userid, resultDTO, connection);

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

        private static void callSPbyPrimary(int userid, ResultDTO resultDTO, SqlConnection connection)
        {
            string updatedOrder;
            SqlCommand command1 = new()
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "GetOrderByIsPRorPerc"
            };
            command1.Parameters.AddWithValue("@userid", userid);
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
                resultDTO.Message = "Order assigned successfully";
            }

            //return updatedOrder;
        }

        public ResultDTO UpdateOrderStatus(UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                // check if user has trd skillsets
                PendingOrdersResponseDTO pendingOrdersResponseDTO = new PendingOrdersResponseDTO();

                List<string> trdskillsets = (from us in _oMTDataContext.UserSkillSet
                                             join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                             where us.UserId == updateOrderStatusDTO.UserId && us.IsActive && ss.SystemofRecordId == 3
                                             && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                             select ss.SkillSetName).ToList();

                bool ispending = false;

                if (trdskillsets.Count > 0)
                {
                    ispending = true;
                }

                pendingOrdersResponseDTO = new PendingOrdersResponseDTO
                {
                    IsPending = ispending,
                    PendingOrder = null
                };

                // check if the skillset has template 

                var table = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == updateOrderStatusDTO.SkillSetId).Select(_ => new { _.SkillSetName, _.SystemofRecordId }).FirstOrDefault();

                var exist = (from tc in _oMTDataContext.TemplateColumns
                             join ss in _oMTDataContext.SkillSet on tc.SkillSetId equals ss.SkillSetId
                             where _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId) && tc.SkillSetId == updateOrderStatusDTO.SkillSetId && ss.IsActive
                             select new
                             {
                                 SkillSetName = ss.SkillSetName
                             }).FirstOrDefault();

                string timetaken = "";

                if (exist != null)
                {
                    string sql = $"SELECT * FROM {exist.SkillSetName} WHERE Id = @Id";

                    using SqlCommand cmd = connection.CreateCommand();

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@Id", updateOrderStatusDTO.Id);
                    cmd.ExecuteNonQuery();

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var querydt1 = datatable.AsEnumerable()
                                    .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                        column => column.ColumnName,
                                        column => row[column] == DBNull.Value ? string.Empty : row[column])).ToList();

                    if (querydt1.Count > 0)
                    {
                        DateTime dateTime = DateTime.UtcNow;
                        var starttime = querydt1[0]["StartTime"]?.ToString();

                        if (!string.IsNullOrEmpty(starttime))
                        {
                            DateTime startDateTime;
                            if (DateTime.TryParse(starttime, out startDateTime))
                            {
                                // Calculate the difference
                                TimeSpan difference = dateTime - startDateTime;
                                timetaken = difference.ToString(@"hh\:mm\:ss");

                            }
                        }

                        string sql1 = "";


                        using (SqlCommand command = connection.CreateCommand())
                        {
                            if (table.SystemofRecordId == 3 && updateOrderStatusDTO.ImageID != null)
                            {
                                sql1 = $"UPDATE {exist.SkillSetName} SET Status = @Status, Remarks = @Remarks, CompletionDate = @CompletionDate, EndTime = @EndTime,TimeTaken = @TimeTaken, ImageId = @ImageId WHERE Id = @ID";
                                command.Parameters.AddWithValue("@ImageId", updateOrderStatusDTO.ImageID);
                            }
                            else if (table.SystemofRecordId == 4 && updateOrderStatusDTO.Number_Of_Manual_Splits != null && updateOrderStatusDTO.Number_Of_Documents != null)
                            {
                                sql1 = $"UPDATE {exist.SkillSetName} SET Status = @Status, Remarks = @Remarks, CompletionDate = @CompletionDate, EndTime = @EndTime, TimeTaken = @TimeTaken, Number_Of_Documents = @Number_Of_Documents, Number_Of_Manual_Splits = @Number_Of_Manual_Splits WHERE Id = @ID";
                                command.Parameters.AddWithValue("@Number_Of_Documents", updateOrderStatusDTO.Number_Of_Documents);
                                command.Parameters.AddWithValue("@Number_Of_Manual_Splits", updateOrderStatusDTO.Number_Of_Manual_Splits);
                            }
                            else
                            {
                                sql1 = $"UPDATE {exist.SkillSetName} SET Status = @Status, Remarks = @Remarks, CompletionDate = @CompletionDate, EndTime = @EndTime, TimeTaken = @TimeTaken WHERE Id = @ID";
                            }

                            command.CommandText = sql1;

                            // Add common parameters
                            command.Parameters.AddWithValue("@Status", updateOrderStatusDTO.StatusId);
                            command.Parameters.AddWithValue("@Remarks", updateOrderStatusDTO.Remarks);
                            command.Parameters.AddWithValue("@Id", updateOrderStatusDTO.Id);
                            command.Parameters.AddWithValue("@EndTime", dateTime);
                            command.Parameters.AddWithValue("@CompletionDate", dateTime);
                            command.Parameters.AddWithValue("@TimeTaken", timetaken);

                            // Execute the query
                            command.ExecuteNonQuery();
                        }

                        resultDTO.Message = "Order status has been updated successfully";
                        resultDTO.IsSuccess = true;
                        resultDTO.Data = pendingOrdersResponseDTO;
                    }
                    else
                    {
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Sorry, this order doesnt exist, you can't update the status anymore.";
                        resultDTO.Data = pendingOrdersResponseDTO;
                    }
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = $"Sorry, the template '{table.SkillSetName}' doesnt exist, you can't update the status for this order anymore.";
                    resultDTO.Data = pendingOrdersResponseDTO;
                }

                if (table.SystemofRecordId == 3 && (updateOrderStatusDTO.StatusId == _authSettings.Value.TRDcompletedManualStatusID || updateOrderStatusDTO.StatusId == _authSettings.Value.TRDpendingStatusID || (!string.IsNullOrEmpty(updateOrderStatusDTO.TrdStatus) && (updateOrderStatusDTO.TrdStatus.ToString().ToLower().Trim() == "manual"))))
                {
                    string manualstatus = string.Empty;
                    int statusid = 0;

                    if (updateOrderStatusDTO.StatusId == _authSettings.Value.TRDcompletedManualStatusID)
                    {
                        manualstatus = $@"SELECT ir.*, dt.DocTypeID 
                                             FROM {exist.SkillSetName} ir
                                             INNER JOIN DocType dt ON ir.DocType = dt.DocumentName
                                             WHERE ir.Id = @Id AND ir.Status = @statusid";

                        statusid = 2;
                    }
                    else if (updateOrderStatusDTO.StatusId == _authSettings.Value.TRDpendingStatusID)
                    {
                        manualstatus = $@"SELECT ir.*, dt.DocTypeID
                                             FROM {exist.SkillSetName} ir
                                             INNER JOIN DocType dt ON ir.DocType = dt.DocumentName
                                             WHERE ir.Id = @Id AND ir.Status = @statusid";

                        statusid = 1;
                    }
                    else if ((!string.IsNullOrEmpty(updateOrderStatusDTO.TrdStatus) && (updateOrderStatusDTO.TrdStatus.ToString().ToLower().Trim() == "manual")))
                    {
                        manualstatus = $@"SELECT ir.*, dt.DocTypeID 
                                             FROM {exist.SkillSetName} ir
                                             INNER JOIN DocType dt ON ir.DocType = dt.DocumentName
                                             WHERE ir.Id = @Id AND ir.HaStatus = 'Manual'";

                        statusid = 2;
                    }


                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = manualstatus;
                    command.Parameters.AddWithValue("@Id", updateOrderStatusDTO.Id);
                    command.Parameters.AddWithValue("@statusid", updateOrderStatusDTO.StatusId);
                    command.ExecuteNonQuery();

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);
                    DataTable ms = dataset.Tables[0];

                    if (ms.Rows.Count > 0)
                    {
                        DataRow row = ms.Rows[0];
                        TrdStatusDTO trdStatusDTO1 = new TrdStatusDTO()
                        {
                            processid = row.Field<int>("SystemOfRecordId"),
                            projectid = row.Field<string>("ProjectID"),
                            referenceid = row.Field<string>("OrderID"),
                            doctypeid = row.Field<int>("DocTypeID"),
                            value = statusid
                        };

                        // Call the REST API
                        CallRestApiAsync(trdStatusDTO1);
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

        private void CallRestApiAsync(TrdStatusDTO trdStatusDTO)
        {
            var url = _authSettings.Value.TrdURL;

            // Serialize the DTO to JSON
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Add("Accept", "application/json");

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(trdStatusDTO);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var webApiUrl = new Uri(url);
                    var response = client.PutAsync(webApiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = response.Content.ReadAsStringAsync().Result;

                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ResultDTO AgentCompletedOrders(AgentCompletedOrdersDTO agentCompletedOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var pagination = agentCompletedOrdersDTO.Pagination;

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();

                if (agentCompletedOrdersDTO.SystemOfRecordId == null && agentCompletedOrdersDTO.SkillSetId == null)
                {
                    List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where us.UserId == agentCompletedOrdersDTO.UserId //&& us.IsActive
                                               && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                               select ss.SkillSetName).Distinct().ToList();

                    foreach (string tablename in tablenames)
                    {
                        //new changes
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SkillSetName == tablename && ps.Status == "Complex"
                                      select new
                                      {
                                          SystemOfRecordId = ps.SystemOfRecordId,
                                          Id = ps.Id
                                      }).FirstOrDefault();

                        SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == tablename).FirstOrDefault();

                        var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                         join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                         where rc.SkillSetId == skillSet.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillSet.SystemofRecordId
                                         orderby rc.ColumnSequence
                                         select mrc.ReportColumnName
                                        ).ToList();


                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";

                        DateTime fromDate = agentCompletedOrdersDTO.FromDate;
                        DateTime toDate = agentCompletedOrdersDTO.ToDate;

                        if (skillSet.SystemofRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";
                            dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                         ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                         : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";
                            if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                        }

                        if (skillSet.SystemofRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }

                            dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                        }

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillSet.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.UserId = @userid AND t.Status IS NOT NULL AND t.Status <> '' ";

                        if (query1 != null)
                        {
                            commonSqlPart += $"AND t.Status <> {query1.Id} ";
                        }

                        // Combine everything into the final query
                        string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;
                        using SqlCommand command = connection.CreateCommand();

                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@userid", agentCompletedOrdersDTO.UserId);
                        command.Parameters.AddWithValue("@FromDate", fromDate);
                        command.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }

                }

                else if (agentCompletedOrdersDTO.SystemOfRecordId != null && agentCompletedOrdersDTO.SkillSetId != null)
                {
                    var query1 = (from ss in _oMTDataContext.SkillSet
                                  join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                  where ss.SkillSetId == agentCompletedOrdersDTO.SkillSetId && ps.Status == "Complex"
                                  select new
                                  {
                                      SystemOfRecordId = ps.SystemOfRecordId,
                                      Id = ps.Id
                                  }).FirstOrDefault();

                    SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == agentCompletedOrdersDTO.SkillSetId).FirstOrDefault();

                    var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == agentCompletedOrdersDTO.SkillSetId && rc.IsActive && rc.SystemOfRecordId == agentCompletedOrdersDTO.SystemOfRecordId
                                     orderby rc.ColumnSequence
                                     select mrc.ReportColumnName
                                    ).ToList();

                    // Query to get column data types for the dynamic table
                    SqlCommand sqlCommand_columnTypeQuery;
                    SqlDataAdapter dataAdapter_columnTypeQuery;
                    List<Dictionary<string, object>> columnTypes;
                    GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                    string commonSqlPart = $"ps.Status as Status, ";

                    string completionDateField = "";
                    string dateFilterCondition = "";

                    DateTime fromDate = agentCompletedOrdersDTO.FromDate;
                    DateTime toDate = agentCompletedOrdersDTO.ToDate;

                    if (skillSet.SystemofRecordId == 1)
                    {
                        // Adjust CompletionDate field placement after Status
                        completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                        dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                     ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                     : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                        if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                    }

                    if (skillSet.SystemofRecordId != 1)
                    {
                        completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                        if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            // Convert IST to UTC
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                        else
                        {
                            fromDate = fromDate.AddHours(8);
                            toDate = toDate.AddDays(1).AddHours(8);
                        }

                        dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                 ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                 : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                    }

                    commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                     $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                     $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                     $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                     $"ss.SkillSetName as SkillSet " +
                                     $"FROM {skillSet.SkillSetName} t " +
                                     $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                     $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                     $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                     $"WHERE t.UserId = @userid AND t.Status IS NOT NULL AND t.Status <> '' ";


                    if (query1 != null)
                    {
                        commonSqlPart += $"AND t.Status <> {query1.Id} ";
                    }

                    // Combine everything into the final query
                    string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;


                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sqlquery;
                    sqlCommand.Parameters.AddWithValue("@userid", agentCompletedOrdersDTO.UserId);
                    sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                    allCompletedRecords.AddRange(querydt2);

                }

                else if (agentCompletedOrdersDTO.SystemOfRecordId != null && agentCompletedOrdersDTO.SkillSetId == null)
                {
                    List<string> tablenames = (from us in _oMTDataContext.UserSkillSet
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where us.UserId == agentCompletedOrdersDTO.UserId && ss.SystemofRecordId == agentCompletedOrdersDTO.SystemOfRecordId //&& us.IsActive
                                               && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                               select ss.SkillSetName).Distinct().ToList();

                    foreach (string tablename in tablenames)
                    {
                        //new changes
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SkillSetName == tablename && ps.Status == "Complex"
                                      select new
                                      {
                                          SystemOfRecordId = ps.SystemOfRecordId,
                                          Id = ps.Id
                                      }).FirstOrDefault();

                        SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == tablename).FirstOrDefault();

                        var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                         join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                         where rc.SkillSetId == skillSet.SkillSetId && rc.IsActive && rc.SystemOfRecordId == agentCompletedOrdersDTO.SystemOfRecordId
                                         orderby rc.ColumnSequence
                                         select mrc.ReportColumnName
                                        ).ToList();

                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";

                        DateTime fromDate = agentCompletedOrdersDTO.FromDate;
                        DateTime toDate = agentCompletedOrdersDTO.ToDate;

                        if (skillSet.SystemofRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                            dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                         ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                         : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                            if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                        }

                        if (skillSet.SystemofRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }

                            dateFilterCondition = agentCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                        }

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillSet.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.UserId = @userid AND t.Status IS NOT NULL AND t.Status <> '' ";


                        if (query1 != null)
                        {
                            commonSqlPart += $"AND t.Status <> {query1.Id} ";
                        }

                        // Combine everything into the final query
                        string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@userid", agentCompletedOrdersDTO.UserId);
                        command.Parameters.AddWithValue("@FromDate", fromDate);
                        command.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }

                }

                if (allCompletedRecords.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = allCompletedRecords.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = allCompletedRecords.Count;
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
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }
                    else
                    {

                        resultDTO.IsSuccess = true;
                        resultDTO.Data = allCompletedRecords;
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Completed orders not found";
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

        public ResultDTO TeamCompletedOrders(TeamCompletedOrdersDTO teamCompletedOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var pagination = teamCompletedOrdersDTO.Pagination;

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();

                if (teamCompletedOrdersDTO.SystemOfRecordId == null && teamCompletedOrdersDTO.SkillSetId == null)
                {
                    List<string> tablenames = (from ta in _oMTDataContext.TeamAssociation
                                               join us in _oMTDataContext.UserSkillSet on ta.UserId equals us.UserId
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId) && ta.TeamId == teamCompletedOrdersDTO.TeamId //&& us.IsActive 
                                               select ss.SkillSetName).Distinct().ToList();

                    foreach (string tablename in tablenames)
                    {
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SkillSetName == tablename && ps.Status == "Complex"
                                      select new
                                      {
                                          SystemofRecordId = ps.SystemOfRecordId,
                                          Id = ps.Id
                                      }).FirstOrDefault();

                        SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == tablename).FirstOrDefault();

                        var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                         join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                         where rc.SkillSetId == skillSet.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillSet.SystemofRecordId
                                         orderby rc.ColumnSequence
                                         select mrc.ReportColumnName
                                        ).ToList();

                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName,ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";

                        DateTime fromDate = teamCompletedOrdersDTO.FromDate;
                        DateTime toDate = teamCompletedOrdersDTO.ToDate;

                        if (skillSet.SystemofRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                            dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                         ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                         : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                            if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                        }

                        if (skillSet.SystemofRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }
                            dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                        }

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillSet.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.TeamLeadId = @Teamid AND t.Status IS NOT NULL AND t.Status <> '' ";

                        if (query1 != null)
                        {
                            commonSqlPart += $"AND t.Status <> {query1.Id} ";
                        }

                        // Combine everything into the final query
                        string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@Teamid", teamCompletedOrdersDTO.TeamId);
                        command.Parameters.AddWithValue("@FromDate", fromDate);
                        command.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }

                }

                else if (teamCompletedOrdersDTO.SystemOfRecordId != null && teamCompletedOrdersDTO.SkillSetId != null)
                {
                    var query1 = (from ss in _oMTDataContext.SkillSet
                                  join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                  where ss.SkillSetId == teamCompletedOrdersDTO.SkillSetId && ps.Status == "Complex"
                                  select new
                                  {
                                      Id = ps.Id
                                  }).FirstOrDefault();

                    SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == teamCompletedOrdersDTO.SkillSetId).FirstOrDefault();

                    var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == teamCompletedOrdersDTO.SkillSetId && rc.IsActive && rc.SystemOfRecordId == teamCompletedOrdersDTO.SystemOfRecordId
                                     orderby rc.ColumnSequence
                                     select mrc.ReportColumnName
                                    ).ToList();

                    // Query to get column data types for the dynamic table
                    SqlCommand sqlCommand_columnTypeQuery;
                    SqlDataAdapter dataAdapter_columnTypeQuery;
                    List<Dictionary<string, object>> columnTypes;
                    GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                    string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName,ps.Status as Status, ";

                    string completionDateField = "";
                    string dateFilterCondition = "";

                    DateTime fromDate = teamCompletedOrdersDTO.FromDate;
                    DateTime toDate = teamCompletedOrdersDTO.ToDate;

                    if (skillSet.SystemofRecordId == 1)
                    {
                        // Adjust CompletionDate field placement after Status
                        completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                        dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                     ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                     : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                        if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                    }

                    if (skillSet.SystemofRecordId != 1)
                    {
                        completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                        if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            // Convert IST to UTC
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                        else
                        {
                            fromDate = fromDate.AddHours(8);
                            toDate = toDate.AddDays(1).AddHours(8);
                        }
                        dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                 ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                 : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                    }

                    commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                     $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                     $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                     $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                     $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                     $"ss.SkillSetName as SkillSet " +
                                     $"FROM {skillSet.SkillSetName} t " +
                                     $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                     $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                     $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                     $"WHERE t.TeamLeadId = @Teamid AND t.Status IS NOT NULL AND t.Status <> '' ";

                    if (query1 != null)
                    {
                        commonSqlPart += $"AND t.Status <> {query1.Id} ";
                    }

                    // Combine everything into the final query
                    string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sqlquery;
                    sqlCommand.Parameters.AddWithValue("@Teamid", teamCompletedOrdersDTO.TeamId);
                    sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                    allCompletedRecords.AddRange(querydt2);

                }

                else if (teamCompletedOrdersDTO.SystemOfRecordId != null && teamCompletedOrdersDTO.SkillSetId == null)
                {
                    List<string> tablenames = (from ta in _oMTDataContext.TeamAssociation
                                               join us in _oMTDataContext.UserSkillSet on ta.UserId equals us.UserId
                                               join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                               where _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId) && ta.TeamId == teamCompletedOrdersDTO.TeamId && ss.SystemofRecordId == teamCompletedOrdersDTO.SystemOfRecordId //&& us.IsActive
                                               select ss.SkillSetName).Distinct().ToList();

                    foreach (string tablename in tablenames)
                    {
                        var query1 = (from ss in _oMTDataContext.SkillSet
                                      join ps in _oMTDataContext.ProcessStatus on ss.SystemofRecordId equals ps.SystemOfRecordId
                                      where ss.SkillSetName == tablename && ps.Status == "Complex"
                                      select new
                                      {
                                          SystemofRecordId = ps.SystemOfRecordId,
                                          Id = ps.Id
                                      }).FirstOrDefault();

                        SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == tablename).FirstOrDefault();

                        var reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                         join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                         where rc.SkillSetId == skillSet.SkillSetId && rc.IsActive && rc.SystemOfRecordId == teamCompletedOrdersDTO.SystemOfRecordId
                                         orderby rc.ColumnSequence
                                         select mrc.ReportColumnName
                                        ).ToList();

                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillSet, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName,ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";

                        DateTime fromDate = teamCompletedOrdersDTO.FromDate;
                        DateTime toDate = teamCompletedOrdersDTO.ToDate;

                        if (skillSet.SystemofRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                            dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                         ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                         : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                            if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                        }

                        if (skillSet.SystemofRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }
                            dateFilterCondition = teamCompletedOrdersDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                        }

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19),  DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillSet.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.TeamLeadId = @Teamid AND t.Status IS NOT NULL AND t.Status <> '' ";

                        if (query1 != null)
                        {
                            commonSqlPart += $"AND t.Status <> {query1.Id} ";
                        }

                        // Combine everything into the final query
                        string sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;
                        command.Parameters.AddWithValue("@Teamid", teamCompletedOrdersDTO.TeamId);
                        command.Parameters.AddWithValue("@FromDate", fromDate);
                        command.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt1);

                    }

                }

                if (allCompletedRecords.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = allCompletedRecords.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = allCompletedRecords.Count;
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
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }
                    else
                    {

                        resultDTO.IsSuccess = true;
                        resultDTO.Data = allCompletedRecords;
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Completed orders not found";
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

        public ResultDTO GetDefaultColumnNames(int systemofrecordid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<DefaultTemplateColumnsDTO> defaultTemplateColumns = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == systemofrecordid && x.IsDefOnlyColumn).Select(_ => new DefaultTemplateColumnsDTO() { DefaultColumnName = _.DefaultColumnName }).ToList();

                if (defaultTemplateColumns.Count > 0)
                {
                    resultDTO.Data = defaultTemplateColumns;
                    resultDTO.Message = "Default columns are successfully fetched";
                    resultDTO.IsSuccess = true;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Default columns doesnt exist for the specified systemofrecordid";
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

        public ResultDTO GetPendingOrderDetails(int userid)
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
	                                     t1.ispriority DESC,t1.StartTime ASC;";

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@UserId", userid);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //var querydt1 = datatable.AsEnumerable()
                    //                .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                    //                    column => column.ColumnName,
                    //                    column => row[column])).ToList();

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

                    var istiqe_order = orderedRecords.ContainsKey("SkillSetName") && orderedRecords["SkillSetName"].ToString() == "TIQELoanMod" && (int)orderedRecords["SystemOfRecordId"] == 4;


                    pendingOrdersResponseDTO = new PendingOrdersResponseDTO
                    {
                        IsPending = ispending,
                        PendingOrder = new List<Dictionary<string, object>> { orderedRecords },
                        IsTiqe = istiqe_order,
                        IsTrdPending = istrd_pending
                    };

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Please update the status of this order";
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = pendingOrdersResponseDTO;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "No more orders to update status";

                    pendingOrdersResponseDTO = new PendingOrdersResponseDTO
                    {
                        IsPending = ispending,
                        PendingOrder = null,
                        IsTiqe = false,
                        IsTrdPending = false
                    };
                    resultDTO.Data = pendingOrdersResponseDTO;
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

        public ResultDTO GetComplexOrdersDetails(ComplexOrdersRequestDTO complexOrdersRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> ComplexRecords = new List<Dictionary<string, object>>();

                if (complexOrdersRequestDTO.SkillsetId == null && complexOrdersRequestDTO.UserId == null)
                {
                    List<SkillSet> tablenames = (from ss in _oMTDataContext.SkillSet
                                                 join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                                 join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                                 where ss.IsActive == true
                                                 select new SkillSet
                                                 {
                                                     SkillSetName = ss.SkillSetName,
                                                     SystemofRecordId = ss.SystemofRecordId
                                                 }
                                            ).Distinct().ToList();

                    foreach (SkillSet tablename in tablenames)
                    {
                        string sqlquery = "";

                        if (tablename.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 5";
                        }

                        else if (tablename.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 9";

                        }
                        else if (tablename.SystemofRecordId == 3)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,t.DocType,CONVERT(VARCHAR(10), t.DocImageDate, 120) as DocImageDate,t.HaStatus,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 15";
                        }
                        else if (tablename.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {tablename.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 21";
                        }

                        if (sqlquery != null)
                        {
                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            DataSet dataset = new DataSet();

                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            //query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                          .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                              column => column.ColumnName,
                                              column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);

                        }

                    }

                    if (ComplexRecords.Count > 0)
                    {
                        resultDTO.Data = ComplexRecords;
                        resultDTO.Message = "Complex orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Complex orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }
                else if (complexOrdersRequestDTO.SkillsetId != null && complexOrdersRequestDTO.UserId == null)
                {
                    SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == complexOrdersRequestDTO.SkillsetId && x.IsActive).FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 5";

                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 9";
                        }
                        else if (skillset.SystemofRecordId == 3)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,t.DocType,CONVERT(VARCHAR(10), t.DocImageDate, 120) as DocImageDate,t.HaStatus,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 15";
                        }
                        else if (skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 21";
                        }

                        if (sqlquery != null)
                        {

                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                            DataSet dataset = new DataSet();

                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            //query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                          .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                              column => column.ColumnName,
                                              column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);


                            if (ComplexRecords.Count > 0)
                            {
                                resultDTO.Data = ComplexRecords;
                                resultDTO.Message = "Complex orders fetched successfully";
                                resultDTO.IsSuccess = true;
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Complex orders not found";
                                resultDTO.StatusCode = "404";
                            }
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "This Skillset is not associated with complex status";
                            resultDTO.StatusCode = "404";
                        }
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Skillset not found";
                        resultDTO.StatusCode = "404";
                    }
                }

                else if (complexOrdersRequestDTO.SkillsetId == null && complexOrdersRequestDTO.UserId != null)
                {
                    List<SkillSet> skillset2 = (from us in _oMTDataContext.UserSkillSet
                                                join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                                where us.UserId == complexOrdersRequestDTO.UserId && us.IsActive
                                                && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                select new SkillSet
                                                {
                                                    SkillSetName = ss.SkillSetName,
                                                    SystemofRecordId = ss.SystemofRecordId
                                                }).Distinct().ToList();

                    foreach (SkillSet skillset in skillset2)
                    {
                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, 
                                        ps.Status as Status, t.Remarks, CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 5 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, 
                                        ps.Status as Status, t.Remarks, CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 9 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 3)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,t.DocType,CONVERT(VARCHAR(10), t.DocImageDate, 120) as DocImageDate,t.HaStatus,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 15 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,
                                        ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, 
                                        ps.Status as Status, t.Remarks, CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 21 and t.UserId = @UserId";
                        }

                        if (sqlquery != null)
                        {
                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;
                            command.Parameters.AddWithValue("@UserId", complexOrdersRequestDTO.UserId);

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                            DataSet dataset = new DataSet();
                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            // Query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName,
                                    column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);
                        }
                    }

                    if (ComplexRecords.Count > 0)
                    {
                        resultDTO.Data = ComplexRecords;
                        resultDTO.Message = "Complex orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Complex orders not found";
                        resultDTO.StatusCode = "404";
                    }
                }


                else if (complexOrdersRequestDTO.SkillsetId != null && complexOrdersRequestDTO.UserId != null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet
                                        .Where(x => x.SkillSetId == complexOrdersRequestDTO.SkillsetId && x.IsActive)
                                        .FirstOrDefault();

                    if (skillset != null)
                    {
                        string sqlquery = "";

                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, ps.Status as Status, t.Remarks,
                                        CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                        INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN UserProfile up on up.UserId = t.UserId
                                        WHERE t.Status = 5 AND t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 2)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, ps.Status as Status, t.Remarks,
                                         CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                         FROM {skillset.SkillSetName} t 
                                         INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                         INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                         INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                         INNER JOIN UserProfile up on up.UserId = t.UserId
                                         WHERE t.Status = 9 AND t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 3)
                        {
                            sqlquery = $@"SELECT t.Id,t.ProjectId,t.OrderId,t.DocType,CONVERT(VARCHAR(10), t.DocImageDate, 120) as DocImageDate,t.HaStatus,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord, ps.Status as Status,t.Remarks,
                                            CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                            FROM {skillset.SkillSetName} t 
                                            INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                            INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                            INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                            INNER JOIN UserProfile up on up.UserId = t.UserId
                                            WHERE t.Status = 15 and t.UserId = @UserId";
                        }
                        else if (skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord, ps.Status as Status, t.Remarks,
                                         CONVERT(VARCHAR(10), t.CompletionDate, 120) as CompletionDate
                                         FROM {skillset.SkillSetName} t 
                                         INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId
                                         INNER JOIN ProcessStatus ps on ps.Id = t.Status
                                         INNER JOIN SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                         INNER JOIN UserProfile up on up.UserId = t.UserId
                                         WHERE t.Status = 21 AND t.UserId = @UserId";
                        }

                        if (sqlquery != null)
                        {

                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;
                            command.Parameters.AddWithValue("@UserId", complexOrdersRequestDTO.UserId);

                            using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                            DataSet dataset = new DataSet();
                            dataAdapter.Fill(dataset);

                            DataTable datatable = dataset.Tables[0];

                            // Query dt to get records
                            var querydt1 = datatable.AsEnumerable()
                                .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                    column => column.ColumnName,
                                    column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                            ComplexRecords.AddRange(querydt1);

                            if (ComplexRecords.Count > 0)
                            {
                                resultDTO.Data = ComplexRecords;
                                resultDTO.Message = "Complex orders fetched successfully";
                                resultDTO.IsSuccess = true;
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Complex orders not found";
                                resultDTO.StatusCode = "404";
                            }
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "This skillset is not associated with complex status";
                            resultDTO.StatusCode = "404";
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

        public ResultDTO ReleaseOrder(ReleaseOrderDTO releaseOrderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                foreach (var order in releaseOrderDTO.Orders)
                {
                    if (order.TryGetValue("skillset", out var skillSetName))
                    {
                        // Fetch the corresponding SkillSet based on SkillSetName
                        SkillSet skillSet = _oMTDataContext.SkillSet.FirstOrDefault(x => x.SkillSetName == skillSetName.ToString());

                        if (skillSet != null)
                        {   //changes here
                            if (order.TryGetValue("OrderId", out var orderId) && order.TryGetValue("UserId", out var userId) && order.TryGetValue("Id", out var id))
                            {
                                string sqlquery = $@"
                                UPDATE {skillSet.SkillSetName} 
                                SET UserId = NULL, 
                                Status = NULL, 
                                Remarks = NULL, 
                                CompletionDate = NULL, 
                                StartTime = NULL, 
                                EndTime = NULL, 
                                TeamLeadId = NULL, 
                                SkillSetId = NULL, 
                                SystemOfRecordId = NULL
                                WHERE OrderId = @OrderId and UserId = @UserId and Id = @Id";

                                using SqlCommand command = connection.CreateCommand();
                                command.CommandText = sqlquery;
                                command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                                command.Parameters.AddWithValue("@UserId", userId.ToString());
                                command.Parameters.AddWithValue("@Id", id.ToString());

                                command.ExecuteNonQuery();
                                resultDTO.Message = "Order has been released back to queue successfully";
                                resultDTO.IsSuccess = true;
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Something went wrong";
                                resultDTO.StatusCode = "404";
                            }

                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Skillset not found";
                            resultDTO.StatusCode = "404";
                        }

                    }
                    else
                    {
                        throw new Exception("Skillset key not found in order.");
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

        public ResultDTO TimeExceededOrders(TimeExceededOrdersDTO timeExceededOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> TExceededRecords = new List<Dictionary<string, object>>();

                if (timeExceededOrdersDTO.SkillsetId == null && timeExceededOrdersDTO.UserId == null)
                {
                    List<SkillSet> tablenames = (from ss in _oMTDataContext.SkillSet
                                                 join tc in _oMTDataContext.TemplateColumns on ss.SkillSetId equals tc.SkillSetId
                                                 join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                                 where ss.IsActive == true
                                                 select new SkillSet
                                                 {
                                                     SkillSetName = ss.SkillSetName,
                                                     SkillSetId = ss.SkillSetId,
                                                     SystemofRecordId = ss.SystemofRecordId,
                                                 }
                                                 ).Distinct().ToList();

                    foreach (SkillSet tablename in tablenames)
                    {
                        List<string> hs = _oMTDataContext.Timeline.Where(x => x.SkillSetId == tablename.SkillSetId && x.IsHardState && x.IsActive).Select(_ => _.Hardstatename).ToList();

                        string sqlquery = "";

                        if (tablename.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {tablename.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                          AND (
                                                (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";
                        }

                        else if (tablename.SystemofRecordId == 2 || tablename.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {tablename.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                          AND (
                                               (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";

                        }

                        else
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {tablename.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                                AND  (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.IsHardState = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime))";

                        }

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        if (tablename.SystemofRecordId == 1 || tablename.SystemofRecordId == 2 || tablename.SystemofRecordId == 4)
                        {
                            command.Parameters.AddWithValue("@hardstates", string.Join(",", hs));
                        }


                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);

                    }

                    if (TExceededRecords.Count > 0)
                    {
                        resultDTO.Data = TExceededRecords;
                        resultDTO.Message = "Time Exceeded orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Time Exceeded orders not found";
                        resultDTO.StatusCode = "404";
                    }

                }

                else if (timeExceededOrdersDTO.SkillsetId == null && timeExceededOrdersDTO.UserId != null)
                {
                    List<SkillSet> skillset2 = (from us in _oMTDataContext.UserSkillSet
                                                join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                                where us.UserId == timeExceededOrdersDTO.UserId && us.IsActive
                                                && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                select new SkillSet
                                                {
                                                    SkillSetName = ss.SkillSetName,
                                                    SkillSetId = ss.SkillSetId,
                                                    SystemofRecordId = ss.SystemofRecordId,
                                                }).Distinct().ToList();

                    foreach (SkillSet skillset in skillset2)
                    {
                        List<string> hs = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillset.SkillSetId && x.IsHardState && x.IsActive).Select(_ => _.Hardstatename).ToList();

                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken,
                                                ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN 
                                                 UserProfile up on up.UserId = t.UserId
                                        INNER JOIN 
                                                 Timeline tl on ss.SkillSetId = tl.SkillSetId
                                        WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                        AND (
                                            (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";

                        }
                        else if (skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken,
                                                ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN 
                                                 SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN 
                                                  UserProfile up on up.UserId = t.UserId
                                        INNER JOIN 
                                                  Timeline tl on ss.SkillSetId = tl.SkillSetId
                                        WHERE 
                                                 t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                        AND (
                                             (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";

                        }
                        else
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken,
                                                ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord
                                        FROM {skillset.SkillSetName} t 
                                        INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                        INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                        INNER JOIN 
                                                 UserProfile up on up.UserId = t.UserId
                                        INNER JOIN 
                                                 Timeline tl on ss.SkillSetId = tl.SkillSetId
                                        WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                        AND  (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.IsHardState = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime))";

                        }
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        if (skillset.SystemofRecordId == 1 || skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);
                            command.Parameters.AddWithValue("@hardstates", string.Join(",", hs));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);
                        }

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);

                    }

                    if (TExceededRecords.Count > 0)
                    {
                        resultDTO.Data = TExceededRecords;
                        resultDTO.Message = "Time Exceeded orders fetched successfully";
                        resultDTO.IsSuccess = true;
                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Time Exceeded orders not found";
                        resultDTO.StatusCode = "404";
                    }


                }

                else if (timeExceededOrdersDTO.SkillsetId != null && timeExceededOrdersDTO.UserId == null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == timeExceededOrdersDTO.SkillsetId && x.IsActive).FirstOrDefault();

                    if (skillset != null)
                    {
                        List<string> hs = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillset.SkillSetId && x.IsHardState && x.IsActive).Select(_ => _.Hardstatename).ToList();

                        string sqlquery = "";
                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                          AND (
                                                (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";


                        }
                        else if (skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                          AND (
                                                (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";
                        }
                        else
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId,t.OrderId,CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId,ss.SkillSetName as skillset,sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL 
                                          AND  (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.IsHardState = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime))";

                        }


                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        if (skillset.SystemofRecordId == 1 || skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            command.Parameters.AddWithValue("@hardstates", string.Join(",", hs));
                        }

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);


                        if (TExceededRecords.Count > 0)
                        {
                            resultDTO.Data = TExceededRecords;
                            resultDTO.Message = "Time Exceeded orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Time Exceeded orders not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                }
                else if (timeExceededOrdersDTO.SkillsetId != null && timeExceededOrdersDTO.UserId != null)
                {
                    SkillSet? skillset = _oMTDataContext.SkillSet
                                        .Where(x => x.SkillSetId == timeExceededOrdersDTO.SkillsetId && x.IsActive)
                                        .FirstOrDefault();

                    if (skillset != null)
                    {
                        List<string> hs = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillset.SkillSetId && x.IsHardState && x.IsActive).Select(_ => _.Hardstatename).ToList();

                        string sqlquery = "";

                        if (skillset.SystemofRecordId == 1)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                          AND (
                                               (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";

                        }
                        else if (skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                          AND (
                                                (EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND ((tl.HardStateName <> '' AND tl.IsHardState = 1 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')) AND t.PropertyState = tl.Hardstatename)
                                                     OR (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime AND t.PropertyState NOT IN (SELECT value FROM STRING_SPLIT(@hardstates, ',')))))
                                                OR (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.ishardstate = 1 AND tl_sub.IsActive = 1) 
                                                AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime)))";

                        }
                        else
                        {
                            sqlquery = $@"SELECT 
                                                t.Id,t.ProjectId, t.OrderId, CONCAT(up.FirstName, ' ', up.LastName) as UserName,t.UserId, ss.SkillSetName as skillset, sor.SystemofRecordName as SystemofRecord,CONVERT(VARCHAR(19), t.StartTime, 120) as StartTime,
                                                CONCAT((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 3600),':',((DATEDIFF(SECOND, t.StartTime, GETDATE()) / 60) % 60), ':',(DATEDIFF(SECOND, t.StartTime, GETDATE()) % 60)) as TimeTaken
                                          FROM {skillset.SkillSetName} t 
                                          INNER JOIN 
                                                SkillSet ss on ss.SkillSetId = t.SkillSetId
                                          INNER JOIN 
                                                SystemOfRecord sor on sor.SystemOfRecordId = ss.SystemOfRecordId
                                          INNER JOIN 
                                                UserProfile up on up.UserId = t.UserId
                                          INNER JOIN 
                                                Timeline tl on ss.SkillSetId = tl.SkillSetId
                                          WHERE 
                                                t.Status IS NULL and t.Completiondate IS NULL and t.UserId = @UserId 
                                          AND  (NOT EXISTS (SELECT 1 FROM Timeline tl_sub WHERE tl_sub.SkillSetId = ss.SkillSetId AND tl_sub.IsHardState = 1 AND tl_sub.IsActive = 1) 
                                               AND (tl.HardStateName = '' AND tl.IsHardState = 0 AND DATEDIFF(MINUTE, t.StartTime, GETDATE()) > tl.ExceedTime))";

                        }

                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = sqlquery;

                        if (skillset.SystemofRecordId == 1 || skillset.SystemofRecordId == 2 || skillset.SystemofRecordId == 4)
                        {
                            command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);
                            command.Parameters.AddWithValue("@hardstates", string.Join(",", hs));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@UserId", timeExceededOrdersDTO.UserId);
                        }

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataSet dataset = new DataSet();
                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        // Query dt to get records
                        var querydt1 = datatable.AsEnumerable()
                            .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                column => column.ColumnName,
                                column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        TExceededRecords.AddRange(querydt1);

                        if (TExceededRecords.Count > 0)
                        {
                            resultDTO.Data = TExceededRecords;
                            resultDTO.Message = "Time Exceeded orders fetched successfully";
                            resultDTO.IsSuccess = true;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Time Exceeded orders not found";
                            resultDTO.StatusCode = "404";
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

        public ResultDTO GetTemplateColumns(int skillsetId)
        {

            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TemplateListDTO> templateList = new List<TemplateListDTO>();


                List<string> templatecolumns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillsetId).Select(_ => _.ColumnAliasName).ToList();

                List<string> defaultTemplateColumns = (from dt in _oMTDataContext.DefaultTemplateColumns
                                                       join ss in _oMTDataContext.SkillSet on dt.SystemOfRecordId equals ss.SystemofRecordId
                                                       where ss.SkillSetId == skillsetId && dt.IsDefOnlyColumn
                                                       select dt.DefaultColumnName).ToList();

                var finalcolumns = templatecolumns.
                                    Concat(defaultTemplateColumns).ToList();
                resultDTO.Data = finalcolumns;

            }

            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO ReplaceOrders(ReplaceOrdersDTO replaceOrdersDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == replaceOrdersDTO.SkillsetId && x.IsActive).FirstOrDefault();

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == replaceOrdersDTO.SkillsetId).ToList();
                    if (template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;

                        List<string> listofcolumns1 = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsDuplicateCheck).Select(_ => _.DefaultColumnName).ToList();
                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                        List<string> combinedList = listofcolumns1.Concat(listofColumns).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(replaceOrdersDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        string isDuplicateColumns1 = listofcolumns1 != null ? string.Join(",", listofcolumns1) : "";
                        string isDuplicateColumns = listofColumns != null ? string.Join(",", listofColumns) : "";

                        // Combine the strings, ensuring that if any of them is null, it's selected without a comma
                        string combinedString = (isDuplicateColumns1 != "" && isDuplicateColumns != "") ? isDuplicateColumns1 + "," + isDuplicateColumns : isDuplicateColumns1 + isDuplicateColumns;

                        // update ispriority = 1 only if order is replaced as ispriority = 1

                        JArray filteredRecords = new JArray(
                                                 recordsarray.Where(record => (int?)record["IsPriority"] == 1));

                        if (filteredRecords.Count > 0)
                        {
                            // update the orders which are already assigned with IsPriority = 1
                            string priorityUpdateQuery = $"UPDATE  " +
                                       $"{tablename} " +
                                       $"SET IsPriority = 1 " +
                                       $"WHERE UserID IS NOT NULL AND (";

                            foreach (JObject records in filteredRecords)
                            {
                                string query = "(";
                                foreach (string columnname in combinedList)
                                {
                                    string columndata = records.Value<string>(columnname);

                                    query += $"[{columnname}] = '{columndata}' AND ";
                                }

                                query = query.Substring(0, query.Length - 5);
                                query += ") OR ";

                                priorityUpdateQuery += query;
                            }

                            priorityUpdateQuery = priorityUpdateQuery.Substring(0, priorityUpdateQuery.Length - 4);
                            priorityUpdateQuery += ")";


                            SqlCommand updatePriority = new SqlCommand(priorityUpdateQuery, connection);
                            updatePriority.ExecuteNonQuery();

                        }
                        // replace orders

                        string sql = $"SELECT * " +
                                     $"FROM {tablename} t " +
                                     $"WHERE UserID IS NULL AND (";

                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in combinedList)
                            {
                                string columndata = records.Value<string>(columnname);

                                query += $"[{columnname}] = '{columndata}' AND ";
                            }

                            query = query.Substring(0, query.Length - 5);
                            query += ") OR ";

                            sql += query;
                        }

                        sql = sql.Substring(0, sql.Length - 4);
                        sql += ")";

                        //execute sql query to fetch records from table

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        List<JObject> recordsToInsert = new List<JObject>();

                        // List<JObject> recordsNotInserted = new List<JObject>();

                        if (querydt.Count > 0)
                        {
                            string deleteSql = $"DELETE FROM {tablename} WHERE UserId IS NULL AND (";

                            foreach (var record in querydt)
                            {
                                string deleteQuery = "(";
                                foreach (string columnname in combinedList)
                                {
                                    string columndata = record[columnname].ToString();

                                    deleteQuery += $"[{columnname}] = '{columndata}' AND ";
                                }

                                deleteQuery = deleteQuery.Substring(0, deleteQuery.Length - 5);
                                deleteQuery += ") OR ";

                                deleteSql += deleteQuery;
                            }

                            deleteSql = deleteSql.Substring(0, deleteSql.Length - 4);
                            deleteSql += ")";

                            using SqlCommand deleteCommand = new SqlCommand(deleteSql, connection);
                            deleteCommand.ExecuteNonQuery();

                            foreach (JObject records in recordsarray)
                            {
                                bool recordExists = querydt.Any(existingRecord =>
                                    combinedList.All(column => records.Value<string>(column) == existingRecord[column].ToString()));

                                if (recordExists)
                                {
                                    recordsToInsert.Add(records);
                                }

                            }

                            if (recordsToInsert.Count > 0)
                            {
                                // Create the new JSON object with "Records" key
                                JObject recordsObject = new JObject();
                                recordsObject["Records"] = JArray.FromObject(recordsToInsert);

                                // Serialize the records to a JSON string
                                string jsonToInsert = recordsObject.ToString();

                                using SqlCommand cmd = new SqlCommand("InsertData", connection);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@SkillSetId", replaceOrdersDTO.SkillsetId);
                                cmd.Parameters.AddWithValue("@jsonData", jsonToInsert);

                                SqlParameter returnValue = new()
                                {
                                    ParameterName = "@RETURN_VALUE",
                                    Direction = ParameterDirection.ReturnValue
                                };

                                cmd.Parameters.Add(returnValue);

                                cmd.ExecuteNonQuery();

                                int returnCode = (int)cmd.Parameters["@RETURN_VALUE"].Value;

                                if (returnCode != 1)
                                {
                                    throw new InvalidOperationException("Something went wrong while replacing the orders,please check the order details.");
                                }

                                resultDTO.IsSuccess = true;
                                resultDTO.Message = "Orders replaced successfully";
                                resultDTO.StatusCode = "200";
                            }

                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Order(s) couldn't be replaced because it's under process or completed";
                            resultDTO.StatusCode = "404";
                        }

                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
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

        public ResultDTO RejectOrder(RejectOrderDTO rejectOrderDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<string> successfulupdate = new List<string>();
                List<Dictionary<string, object>> failedupdates = new List<Dictionary<string, object>>();

                foreach (var order in rejectOrderDTO.Orders)
                {
                    if (order.TryGetValue("skillset", out var skillSetName) && order.TryGetValue("UserId", out var userid) && order.TryGetValue("UserName", out var username) && order.TryGetValue("OrderId", out var orderid) && order.TryGetValue("Id", out var id))
                    {

                        SkillSet skillSet1 = _oMTDataContext.SkillSet.Where(x => x.IsActive && x.SkillSetName == skillSetName.ToString()).FirstOrDefault();

                        string Userid = userid.ToString();
                        int userIdInt = int.Parse(Userid);

                        UserSkillSet userSkillSet = _oMTDataContext.UserSkillSet.FirstOrDefault(x => x.UserId == userIdInt && x.IsActive && x.SkillSetId == skillSet1.SkillSetId);

                        if (userSkillSet != null)
                        {
                            string sqlquery = $@"
                                               UPDATE {skillSet1.SkillSetName} 
                                               SET Status = NULL, 
                                               Remarks = NULL, 
                                               CompletionDate = NULL, 
                                               StartTime = getdate(), 
                                               EndTime = NULL
                                               WHERE OrderId = @OrderId and UserId = @UserId and Id = @Id";

                            using SqlCommand command = connection.CreateCommand();
                            command.CommandText = sqlquery;
                            command.Parameters.AddWithValue("@OrderId", orderid.ToString());
                            command.Parameters.AddWithValue("@UserId", userid.ToString());
                            command.Parameters.AddWithValue("@Id", id.ToString());

                            command.ExecuteNonQuery();

                            successfulupdate.Add(orderid.ToString());

                        }
                        else
                        {
                            var updateInfo = new Dictionary<string, object>
                                {
                                    { "Username", username },
                                    { "SkillSetName", skillSetName },
                                    { "OrderId", orderid }
                                };

                            failedupdates.Add(updateInfo);

                        }

                    }
                    else
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "Invalid order data";
                        resultDTO.StatusCode = "400";
                    }
                }
                if (successfulupdate.Count > 0 && failedupdates.Count > 0)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = $"No of orders rejected successfully : {successfulupdate.Count}. \nThe following orders could not be rejected:\n";
                    foreach (var updateInfo in failedupdates)
                    {
                        resultDTO.Message += $"- OrderId {updateInfo["OrderId"]} for {updateInfo["Username"]} could not be rejected because {updateInfo["SkillSetName"]} is no longer associated.\n";
                    }
                    resultDTO.StatusCode = "200";
                }
                else if (successfulupdate.Count <= 0 && failedupdates.Count > 0)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Rejection failed. \nThe following orders could not be rejected:\n";
                    foreach (var updateInfo in failedupdates)
                    {
                        resultDTO.Message += $"- OrderId {updateInfo["OrderId"]} for {updateInfo["Username"]} could not be rejected because {updateInfo["SkillSetName"]} is no longer associated.\n";
                    }
                    resultDTO.StatusCode = "404";
                }
                else if (successfulupdate.Count > 0 && failedupdates.Count <= 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Orders rejected successfully";
                    resultDTO.StatusCode = "200";
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

        public ResultDTO AssignOrderToUser(AssignOrderToUserDTO assignOrderToUserDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<string> skillsetnames = (from us in _oMTDataContext.UserSkillSet
                                              join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                              where us.UserId == assignOrderToUserDTO.UserId && us.IsActive && ss.IsActive
                                              select ss.SkillSetName).ToList();

                // get skillsets from orders
                List<string> orderskillsetnames = new List<string>();

                foreach (var order in assignOrderToUserDTO.Orders)
                {
                    if (order.TryGetValue("skillset", out var skillSetName))
                    {
                        orderskillsetnames.Add(skillSetName.ToString());
                    }
                }

                // get the skillsets not associated to user
                List<string> notAssociatedSkillsets = new List<string>();

                foreach (string orderSkillset in orderskillsetnames)
                {
                    if (!skillsetnames.Contains(orderSkillset))
                    {
                        notAssociatedSkillsets.Add(orderSkillset);
                    }
                }

                //check if user has all the required skillsets
                bool commonSkillSets = orderskillsetnames.All(skillsetnames.Contains);

                if (commonSkillSets)
                {
                    foreach (var order in assignOrderToUserDTO.Orders)
                    {
                        if (order.TryGetValue("skillset", out var skillSetName))
                        {
                            SkillSet skillSet = _oMTDataContext.SkillSet.FirstOrDefault(x => x.SkillSetName == skillSetName.ToString());
                            TeamAssociation teamAssociation = _oMTDataContext.TeamAssociation.Where(x => x.UserId == assignOrderToUserDTO.UserId).FirstOrDefault();

                            if (skillSet != null)
                            {
                                if (order.TryGetValue("OrderId", out var orderId) && order.TryGetValue("UserId", out var userId) && order.TryGetValue("Id", out var id))
                                {
                                    UserProfile userProfile = _oMTDataContext.UserProfile.Where(x => x.UserId == assignOrderToUserDTO.UserId).FirstOrDefault();

                                    string sqlquery = $@"
                                                       UPDATE {skillSet.SkillSetName} 
                                                       SET UserId = {assignOrderToUserDTO.UserId}, 
                                                       Status = NULL, 
                                                       Remarks = NULL, 
                                                       CompletionDate = NULL, 
                                                       StartTime = getdate(), 
                                                       EndTime = NULL, 
                                                       TeamLeadId = {teamAssociation.TeamId},
                                                       SkillSetId = {skillSet.SkillSetId}, 
                                                       SystemOfRecordId = {skillSet.SystemofRecordId}
                                                       WHERE OrderId = @OrderId and UserId = @UserId and Id = @Id";

                                    using SqlCommand command = connection.CreateCommand();
                                    command.CommandText = sqlquery;
                                    command.Parameters.AddWithValue("@OrderId", orderId.ToString());
                                    command.Parameters.AddWithValue("@UserId", userId.ToString());
                                    command.Parameters.AddWithValue("@Id", id.ToString());

                                    command.ExecuteNonQuery();
                                    resultDTO.Message = "Order has been assigned to the selected user successfully";
                                    resultDTO.IsSuccess = true;
                                }
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Something went wrong";
                                resultDTO.StatusCode = "404";
                            }
                        }
                        else
                        {
                            throw new Exception("Skillset key not found in order.");
                        }
                    }
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Selected user is not associated with " + string.Join(", ", notAssociatedSkillsets) + ". Please associate the skillsets to the user and try again.";
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

        public ResultDTO DeleteOrders(DeleteOrderDTO deleteOrderDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var url = _emailDetailsSettings.Value.SendEmailURL;

                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == deleteOrderDTO.SkillsetId && x.IsActive).FirstOrDefault();
                if (skillSet != null)
                {
                    List<TemplateColumns> template = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == deleteOrderDTO.SkillsetId).ToList();

                    if (template.Count > 0)
                    {
                        string tablename = skillSet.SkillSetName;

                        List<string> listofcolumns1 = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsDuplicateCheck).Select(_ => _.DefaultColumnName).ToList();
                        List<string> listofColumns = template.Where(x => x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                        List<string> combinedList = listofcolumns1.Concat(listofColumns).ToList();

                        //parse json data
                        JObject jsondata = JObject.Parse(deleteOrderDTO.JsonData);
                        JArray recordsarray = jsondata.Value<JArray>("Records");

                        string isDuplicateColumns1 = listofcolumns1 != null ? string.Join(",", listofcolumns1) : "";
                        string isDuplicateColumns = listofColumns != null ? string.Join(",", listofColumns) : "";

                        // Combine the strings, ensuring that if any of them is null, it's selected without a comma
                        string combinedString = (isDuplicateColumns1 != "" && isDuplicateColumns != "") ? isDuplicateColumns1 + "," + isDuplicateColumns : isDuplicateColumns1 + isDuplicateColumns;

                        string sql = $"SELECT * " +
                                     $"FROM {tablename} t " +
                                     $"WHERE ";

                        foreach (JObject records in recordsarray)
                        {
                            string query = "(";
                            foreach (string columnname in combinedList)
                            {
                                string columndata = records.Value<string>(columnname);

                                query += $"[{columnname}] = '{columndata}' AND ";
                            }

                            query = query.Substring(0, query.Length - 5);
                            query += ") OR ";

                            sql += query;
                        }

                        sql = sql.Substring(0, sql.Length - 4);

                        //execute sql query to fetch records from table
                        string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                        using SqlConnection connection = new(connectionstring);
                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);

                        DataSet dataset = new DataSet();
                        connection.Open();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        // Check if orders already in invoice 

                        var orderids_inv = new List<string>();

                        foreach (DataRow row_inv in datatable.Rows)
                        {
                            if (datatable.Columns.Contains("OrderId") && row_inv["OrderId"] != DBNull.Value)
                            {
                                var oid = row_inv["OrderId"].ToString();
                                orderids_inv.Add(oid);
                            }
                        }

                        var oid_check = string.Join(",", orderids_inv.Select(oid => $"'{oid}'"));

                        string inv_query = $"SELECT OrderId FROM InvoiceDump WHERE Skillset = '{tablename}' and OrderId in ({oid_check})";

                        using SqlDataAdapter Inv_dataAdapter = new SqlDataAdapter(inv_query, connection);

                        DataSet inDS = new DataSet();

                        Inv_dataAdapter.Fill(inDS);

                        DataTable inv_datatable = inDS.Tables[0];

                        var inv_dt = inv_datatable.AsEnumerable()
                                      .Select(row => inv_datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        // if yes dont allow to delete
                        if (inv_dt.Count > 0)
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.StatusCode = "404";
                            resultDTO.Message = "The following orders are already in invoice and can't be deleted: " + string.Join(", ", inv_dt.Select(dict => dict["OrderId"].ToString()));
                        }
                        else
                        {
                            var NoOfOrders = querydt.Count;

                            if (querydt.Count > 0)
                            {
                                string deleteSql = $"DELETE FROM {tablename} WHERE ";

                                foreach (var record in querydt)
                                {
                                    string deleteQuery = "(";
                                    foreach (string columnname in combinedList)
                                    {
                                        string columndata = record[columnname].ToString();

                                        deleteQuery += $"[{columnname}] = '{columndata}' AND ";
                                    }

                                    deleteQuery = deleteQuery.Substring(0, deleteQuery.Length - 5);
                                    deleteQuery += ") OR ";

                                    deleteSql += deleteQuery;
                                }

                                deleteSql = deleteSql.Substring(0, deleteSql.Length - 4);

                                using SqlCommand deleteCommand = new SqlCommand(deleteSql, connection);
                                deleteCommand.ExecuteNonQuery();

                                // send details about deleted orders via mail 

                                DateTime deletedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                                string firstname = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.FirstName).FirstOrDefault();
                                string lastname = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.LastName).FirstOrDefault();
                                string username = string.Join(' ', firstname, lastname);

                                IConfigurationSection toEmailId = _configuration.GetSection("EmailConfig:UploadorderAPIdetails:ToEmailId");

                                List<string> toEmailIds1 = toEmailId.AsEnumerable()
                                                                          .Where(c => !string.IsNullOrEmpty(c.Value))
                                                                          .Select(c => c.Value)
                                                                          .ToList();

                                var deletedetails = $"{username} has deleted {NoOfOrders} orders in {skillSet.SkillSetName} at {deletedDate}";

                                SendEmailDTO sendEmailDTO1 = new SendEmailDTO
                                {
                                    ToEmailIds = toEmailIds1,
                                    Subject = "Orders deleted",
                                    Body = deletedetails,
                                };

                                try
                                {
                                    using (HttpClient client = new HttpClient())
                                    {
                                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO1);
                                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                                        var webApiUrl = new Uri(url);
                                        var response = client.PostAsync(webApiUrl, content).Result;

                                        if (response.IsSuccessStatusCode)
                                        {
                                            var responseData = response.Content.ReadAsStringAsync().Result;

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw;
                                }


                                resultDTO.IsSuccess = true;
                                resultDTO.Message = "Orders deleted from template successfully";
                            }
                        }

                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "Skillset does not exist.";
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

        public ResultDTO SkillsetWiseReports(SkillsetWiseReportsDTO skillsetWiseReportsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var pagination = skillsetWiseReportsDTO.Pagination;

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                connection.Open();

                List<Dictionary<string, object>> allCompletedRecords = new List<Dictionary<string, object>>();

                List<string> reportcol = new List<string>();

                if (skillsetWiseReportsDTO.SkillSetId == null && skillsetWiseReportsDTO.StatusId == null)
                {
                    List<string> skillsetnames = (from ss in _oMTDataContext.SkillSet
                                                  where ss.SystemofRecordId == skillsetWiseReportsDTO.SystemOfRecordId && ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                  select ss.SkillSetName).ToList();


                    foreach (string skillsetname in skillsetnames)
                    {
                        var skillsetdetails = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillsetname && x.IsActive).FirstOrDefault();

                        reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == skillsetdetails.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillsetWiseReportsDTO.SystemOfRecordId
                                     orderby rc.ColumnSequence
                                     select mrc.ReportColumnName
                                    ).ToList();

                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillsetdetails, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName, ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";
                        string sqlquery = "";

                        DateTime fromDate = skillsetWiseReportsDTO.FromDate;
                        DateTime toDate = skillsetWiseReportsDTO.ToDate;

                        if (skillsetWiseReportsDTO.SystemOfRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                            dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                         ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                         : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate) ";

                            if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }

                            commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                        $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                        $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                        $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                        $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                        $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                        $"ss.SkillSetName as SkillSet " +
                                        $"FROM {skillsetdetails.SkillSetName} t " +
                                        $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                        $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                        $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                        $"WHERE t.Status IS NOT NULL AND t.Status <> '' AND t.Status <> 17 ";



                            // Combine everything into the final query
                            sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                            sqlquery += $" Union " +
                                        $"{sqlquery1} " +
                                        $"NULL AS UserName, 'System-Pending' AS Status, NULL AS CompletionDate, NULL AS Remarks,NULL AS StartTime,NULL AS EndTime, NULL AS TimeTaken, '{skillsetdetails.SkillSetName}' AS SkillSet FROM {skillsetdetails.SkillSetName} t WHERE t.Status = 17 " +
                                        $"{dateFilterCondition} " +
                                        $"order by ps.Status ";

                        }


                        if (skillsetWiseReportsDTO.SystemOfRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }


                            dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";

                            commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillsetdetails.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.Status IS NOT NULL AND t.Status <> '' ";



                            // Combine everything into the final query
                            sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        }



                        using SqlCommand sqlCommand = connection.CreateCommand();
                        sqlCommand.CommandText = sqlquery;

                        sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                        sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt2 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt2);
                    }

                }

                if (skillsetWiseReportsDTO.SkillSetId != null && skillsetWiseReportsDTO.StatusId != null)
                {
                    SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetWiseReportsDTO.SkillSetId && x.IsActive).FirstOrDefault();

                    List<int> statusid = skillsetWiseReportsDTO.StatusId.ToList();
                    string csStatusId = string.Join(",", statusid);

                    reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                 join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                 where rc.SkillSetId == skillsetWiseReportsDTO.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillsetWiseReportsDTO.SystemOfRecordId
                                 orderby rc.ColumnSequence
                                 select mrc.ReportColumnName
                                  ).ToList();

                    // Query to get column data types for the dynamic table
                    SqlCommand sqlCommand_columnTypeQuery;
                    SqlDataAdapter dataAdapter_columnTypeQuery;
                    List<Dictionary<string, object>> columnTypes;
                    GetDataType(connection, skillset, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                    string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName, ps.Status as Status, ";

                    string completionDateField = "";
                    string dateFilterCondition = "";
                    string sqlquery = "";

                    DateTime fromDate = skillsetWiseReportsDTO.FromDate;
                    DateTime toDate = skillsetWiseReportsDTO.ToDate;

                    if (skillsetWiseReportsDTO.SystemOfRecordId == 1)
                    {
                        // Adjust CompletionDate field placement after Status
                        completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                        dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                    ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                    : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                        if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }

                        List<int> Sci_statusid = skillsetWiseReportsDTO.StatusId.ToList();
                        Sci_statusid.Remove(17);

                        string sci_csStatusId = string.Join(",", Sci_statusid);

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                       $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                       $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                       $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                       $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                       $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                       $"ss.SkillSetName as SkillSet " +
                                       $"FROM {skillset.SkillSetName} t " +
                                       $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                       $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                       $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                       $"WHERE t.Status IS NOT NULL AND t.Status <> ''  AND t.Status <> 17 ";

                        if (!sci_csStatusId.IsNullOrEmpty())
                        {
                            commonSqlPart += $"AND t.Status IN ({sci_csStatusId}) ";
                        }

                        else if (sci_csStatusId.IsNullOrEmpty())
                        {
                            commonSqlPart += $"AND 1 = 0 ";
                        }

                        // Combine everything into the final query
                        sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        if (statusid.Contains(17))
                        {
                            sqlquery += $" Union " +
                                    $"{sqlquery1} " +
                                    $"NULL AS UserName, 'System-Pending' AS Status, NULL AS CompletionDate, NULL AS Remarks,NULL AS StartTime,NULL AS EndTime, NULL AS TimeTaken, '{skillset.SkillSetName}' AS SkillSet FROM {skillset.SkillSetName} t WHERE t.Status = 17 " +
                                    $"{dateFilterCondition} " +
                                    $"order by ps.Status ";


                        }

                    }


                    if (skillsetWiseReportsDTO.SystemOfRecordId != 1)
                    {
                        completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                        if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            // Convert IST to UTC
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                        else
                        {
                            fromDate = fromDate.AddHours(8);
                            toDate = toDate.AddDays(1).AddHours(8);
                        }


                        dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                 ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                 : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                    $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                    $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                    $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                    $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                    $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                    $"ss.SkillSetName as SkillSet " +
                                    $"FROM {skillset.SkillSetName} t " +
                                    $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                    $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                    $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                    $"WHERE t.Status IS NOT NULL AND t.Status <> '' AND t.Status IN ({csStatusId}) ";



                        // Combine everything into the final query
                        sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                    }

                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sqlquery;

                    sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                    allCompletedRecords.AddRange(querydt2);

                }

                if (skillsetWiseReportsDTO.SkillSetId == null && skillsetWiseReportsDTO.StatusId != null)
                {
                    List<string> skillsetnames = (from ss in _oMTDataContext.SkillSet
                                                  where ss.SystemofRecordId == skillsetWiseReportsDTO.SystemOfRecordId && ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                  select ss.SkillSetName).ToList();

                    List<int> statusid = skillsetWiseReportsDTO.StatusId.ToList();
                    string csStatusId = string.Join(",", statusid);

                    foreach (string skillsetname in skillsetnames)
                    {
                        var skillsetdetails2 = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillsetname && x.IsActive).FirstOrDefault();

                        reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                     join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                     where rc.SkillSetId == skillsetdetails2.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillsetWiseReportsDTO.SystemOfRecordId
                                     orderby rc.ColumnSequence
                                     select mrc.ReportColumnName
                                    ).ToList();

                        // Query to get column data types for the dynamic table
                        SqlCommand sqlCommand_columnTypeQuery;
                        SqlDataAdapter dataAdapter_columnTypeQuery;
                        List<Dictionary<string, object>> columnTypes;
                        GetDataType(connection, skillsetdetails2, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                        string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName, ps.Status as Status, ";

                        string completionDateField = "";
                        string dateFilterCondition = "";
                        string sqlquery = "";

                        DateTime fromDate = skillsetWiseReportsDTO.FromDate;
                        DateTime toDate = skillsetWiseReportsDTO.ToDate;

                        if (skillsetWiseReportsDTO.SystemOfRecordId == 1)
                        {
                            // Adjust CompletionDate field placement after Status
                            completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                            dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                        ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                        : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                            if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }

                            List<int> Sci_statusid = skillsetWiseReportsDTO.StatusId.ToList();
                            Sci_statusid.Remove(17);

                            string sci_csStatusId = string.Join(",", Sci_statusid);

                            commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                        $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                        $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                        $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                        $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                        $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                        $"ss.SkillSetName as SkillSet " +
                                         $"FROM {skillsetdetails2.SkillSetName} t " +
                                        $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                        $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                        $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                        $"WHERE t.Status IS NOT NULL AND t.Status <> ''  AND t.Status <> 17 ";

                            if (!sci_csStatusId.IsNullOrEmpty())
                            {
                                commonSqlPart += $"AND t.Status IN ({sci_csStatusId}) ";
                            }

                            else if (sci_csStatusId.IsNullOrEmpty())
                            {
                                commonSqlPart += $"AND 1 = 0 ";
                            }

                            // Combine everything into the final query
                            sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                            if (statusid.Contains(17))
                            {
                                sqlquery += $" Union " +
                                        $"{sqlquery1} " +
                                        $"NULL AS UserName, 'System-Pending' AS Status, NULL AS CompletionDate, NULL AS Remarks,NULL AS StartTime,NULL AS EndTime, NULL AS TimeTaken, '{skillsetdetails2.SkillSetName}' AS SkillSet FROM {skillsetdetails2.SkillSetName} t WHERE t.Status = 17 " +
                                        $"{dateFilterCondition} " +
                                        $"order by ps.Status ";


                            }
                        }


                        if (skillsetWiseReportsDTO.SystemOfRecordId != 1)
                        {
                            completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                            if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                            {
                                // Convert IST to UTC
                                fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                                toDate = toDate.AddHours(-5).AddMinutes(-30);
                            }
                            else
                            {
                                fromDate = fromDate.AddHours(8);
                                toDate = toDate.AddDays(1).AddHours(8);
                            }


                            dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                     ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                     : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";

                            commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                         $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                         $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                         $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                         $"ss.SkillSetName as SkillSet " +
                                          $"FROM {skillsetdetails2.SkillSetName} t " +
                                         $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                         $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                         $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                         $"WHERE t.Status IS NOT NULL AND t.Status <> '' AND t.Status IN ({csStatusId}) ";



                            // Combine everything into the final query
                            sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        }



                        using SqlCommand sqlCommand = connection.CreateCommand();
                        sqlCommand.CommandText = sqlquery;

                        sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                        sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                        using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                        DataSet dataset = new DataSet();

                        dataAdapter.Fill(dataset);

                        DataTable datatable = dataset.Tables[0];

                        //query dt to get records
                        var querydt2 = datatable.AsEnumerable()
                                      .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                          column => column.ColumnName,
                                          column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                        allCompletedRecords.AddRange(querydt2);
                    }
                }

                if (skillsetWiseReportsDTO.SkillSetId != null && skillsetWiseReportsDTO.StatusId == null)
                {
                    SkillSet skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetWiseReportsDTO.SkillSetId && x.IsActive).FirstOrDefault();

                    reportcol = (from mrc in _oMTDataContext.MasterReportColumns
                                 join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                 where rc.SkillSetId == skillsetWiseReportsDTO.SkillSetId && rc.IsActive && rc.SystemOfRecordId == skillsetWiseReportsDTO.SystemOfRecordId
                                 orderby rc.ColumnSequence
                                 select mrc.ReportColumnName
                                 ).ToList();

                    // Query to get column data types for the dynamic table
                    SqlCommand sqlCommand_columnTypeQuery;
                    SqlDataAdapter dataAdapter_columnTypeQuery;
                    List<Dictionary<string, object>> columnTypes;
                    GetDataType(connection, skillset, out sqlCommand_columnTypeQuery, out dataAdapter_columnTypeQuery, out columnTypes);

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

                    string commonSqlPart = $"CONCAT(up.FirstName, ' ', up.LastName) as UserName, ps.Status as Status, ";

                    string completionDateField = "";
                    string dateFilterCondition = "";
                    string sqlquery = "";

                    DateTime fromDate = skillsetWiseReportsDTO.FromDate;
                    DateTime toDate = skillsetWiseReportsDTO.ToDate;

                    if (skillsetWiseReportsDTO.SystemOfRecordId == 1)
                    {
                        // Adjust CompletionDate field placement after Status
                        completionDateField = "CONVERT(VARCHAR(10), t.AllocationDate, 101) as CompletionDate";

                        dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                    ? "AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                    : "AND t.AllocationDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)";

                        if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }

                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                       $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                       $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                       $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                       $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                       $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                       $"ss.SkillSetName as SkillSet " +
                                       $"FROM {skillset.SkillSetName} t " +
                                       $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                       $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                       $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                       $"WHERE t.Status IS NOT NULL AND t.Status <> '' AND t.Status <> 17 ";



                        // Combine everything into the final query
                        sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;

                        sqlquery += $" Union " +
                                    $"{sqlquery1} " +
                                    $"NULL AS UserName, 'System-Pending' AS Status, NULL AS CompletionDate, NULL AS Remarks,NULL AS StartTime,NULL AS EndTime, NULL AS TimeTaken, '{skillset.SkillSetName}' AS SkillSet FROM {skillset.SkillSetName} t WHERE t.Status = 17 " +
                                    $"{dateFilterCondition} " +
                                    $"order by ps.Status ";


                    }


                    if (skillsetWiseReportsDTO.SystemOfRecordId != 1)
                    {
                        completionDateField = $"CONVERT(VARCHAR(10), (t.CompletionDate AT TIME ZONE 'UTC' AT TIME ZONE 'Pacific Standard Time'), 101) as CompletionDate";

                        if (skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime)
                        {
                            // Convert IST to UTC
                            fromDate = fromDate.AddHours(-5).AddMinutes(-30);
                            toDate = toDate.AddHours(-5).AddMinutes(-30);
                        }
                        else
                        {
                            fromDate = fromDate.AddHours(8);
                            toDate = toDate.AddDays(1).AddHours(8);
                        }


                        dateFilterCondition = skillsetWiseReportsDTO.DateFilter == Dateoption.FilterBasedOnCompletiontime
                                                                                 ? $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate"
                                                                                 : $"AND t.CompletionDate BETWEEN @FromDate AND @ToDate";
                        commonSqlPart += $"{completionDateField}, t.Remarks, " +
                                    $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.StartTime)), 120) as StartTime, " +
                                    $"CONVERT(VARCHAR(19), DATEADD(hour, 5, DATEADD(minute, 30, t.EndTime)), 120) as EndTime, " +
                                    $"CONCAT(RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 3600) AS VARCHAR), 2), ':', " +
                                    $"RIGHT('0' + CAST(((DATEDIFF(SECOND, t.StartTime, t.EndTime) / 60) % 60) AS VARCHAR), 2), ':', " +
                                    $"RIGHT('0' + CAST((DATEDIFF(SECOND, t.StartTime, t.EndTime) % 60) AS VARCHAR), 2)) as TimeTaken, " +
                                    $"ss.SkillSetName as SkillSet " +
                                    $"FROM {skillset.SkillSetName} t " +
                                    $"INNER JOIN SkillSet ss on ss.SkillSetId = t.SkillSetId " +
                                    $"INNER JOIN ProcessStatus ps on ps.Id = t.Status " +
                                    $"INNER JOIN UserProfile up on up.UserId = t.UserId " +
                                    $"WHERE t.Status IS NOT NULL AND t.Status <> '' ";



                        // Combine everything into the final query
                        sqlquery = sqlquery1 + commonSqlPart + dateFilterCondition;
                    }



                    using SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandText = sqlquery;

                    sqlCommand.Parameters.AddWithValue("@FromDate", fromDate);
                    sqlCommand.Parameters.AddWithValue("@ToDate", toDate);

                    using SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);

                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    //query dt to get records
                    var querydt2 = datatable.AsEnumerable()
                                  .Select(row => datatable.Columns.Cast<DataColumn>().ToDictionary(
                                      column => column.ColumnName,
                                      column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                    allCompletedRecords.AddRange(querydt2);
                }

                if (allCompletedRecords.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = allCompletedRecords.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = allCompletedRecords.Count;
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
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }
                    else
                    {

                        resultDTO.IsSuccess = true;
                        resultDTO.Data = allCompletedRecords;
                        resultDTO.Message = "Completed orders has been fetched successfully";
                    }


                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Completed orders not found";
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

        public void GetDataType(SqlConnection connection, SkillSet skillset, out SqlCommand sqlCommand_columnTypeQuery, out SqlDataAdapter dataAdapter_columnTypeQuery, out List<Dictionary<string, object>> columnTypes)
        {
            string columnTypeQuery = $@"
                                                SELECT COLUMN_NAME, DATA_TYPE 
                                                FROM INFORMATION_SCHEMA.COLUMNS 
                                                WHERE TABLE_NAME = '{skillset.SkillSetName}'";
            sqlCommand_columnTypeQuery = connection.CreateCommand();
            sqlCommand_columnTypeQuery.CommandText = columnTypeQuery;
            dataAdapter_columnTypeQuery = new SqlDataAdapter(sqlCommand_columnTypeQuery);
            DataSet dataset_columnTypeQuery = new DataSet();

            dataAdapter_columnTypeQuery.Fill(dataset_columnTypeQuery);

            DataTable datatable_columnTypeQuery = dataset_columnTypeQuery.Tables[0];

            //query dt to get records
            columnTypes = datatable_columnTypeQuery.AsEnumerable()
                              .Select(row => datatable_columnTypeQuery.Columns.Cast<DataColumn>().ToDictionary(
                              column => column.ColumnName,
                              column => row[column] == DBNull.Value ? "" : row[column])).ToList();
        }

        public ResultDTO GetMandatoryColumnNames(int skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetid && x.IsActive).FirstOrDefault();

                List<string> defaultTemplateColumns = _oMTDataContext.DefaultTemplateColumns.Where(x => x.SystemOfRecordId == skillSet.SystemofRecordId && x.IsMandatoryColumn && x.IsActive).Select(_ => _.DefaultColumnName).ToList();
                List<string> duplicatecheckcolumns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillsetid && x.IsDuplicateCheck).Select(_ => _.ColumnAliasName).ToList();

                List<string> combinedList = defaultTemplateColumns.Concat(duplicatecheckcolumns).ToList();

                if (combinedList.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = combinedList;
                    resultDTO.Message = "Mandatory columns fetched successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Mandatory columns not found";
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

        public ResultDTO GetTrdPendingOrders(int userid)
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
                    var userskillsetlist = (from us in _oMTDataContext.UserSkillSet
                                            join ss in _oMTDataContext.SkillSet on us.SkillSetId equals ss.SkillSetId
                                            where us.UserId == userid && us.IsActive == true && ss.SystemofRecordId == 3
                                            select new
                                            {
                                                skillsetname = ss.SkillSetName,
                                                skillsetid = us.SkillSetId,
                                                SystemofRecord = ss.SystemofRecordId
                                            }).ToList();

                    if (userskillsetlist.Count > 0)
                    {
                        string updatedOrder;
                        SqlCommand command1 = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "GetTrdPendingOrder"
                        };
                        command1.Parameters.AddWithValue("@userid", userid);
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
                            resultDTO.Message = "Order assigned successfully";
                        }
                    }
                    else
                    {
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "You dont have any TRD skillsets mapped to your account.";
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
