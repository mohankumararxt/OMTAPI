using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;


namespace TrdIntegrator
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            // var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            int normalTRDidno = GetLastNormalTrdOrdersId();
            GetTrdOrders(normalTRDidno);
            int pendingTRDidno = GetLastPendingTrdOrdersId();
            GetTrdPendingOrders(pendingTRDidno);

        }

        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
        public static void GetTrdOrders(int normalTRDidno)
        {
            string Url = "";

            try
            {
                Url = ConfigurationManager.AppSettings["SendEmailUrl"];
                string connectionString = ConfigurationManager.ConnectionStrings["PostgressSqlConnection"].ConnectionString;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $@"Select oo.id,oo.referenceid as ""OrderId"",oo.projectid as ""ProjectId"",oo.docimagedate as ""DocImageDate"",dc.documentname as ""DocType"",oo.doctypeid,oo.status as ""HaStatus"", 'Trailing_Doc_Review' as ""WorkflowStatus"", 0 as ""IsPriority"",0 as ""IsPending""
                                 from public.tbl_omt_orders oo
                                 inner join public.tbl_doctypes dc on dc.id = oo.doctypeid 
                                 where oo.id > @idno Order By oo.id ASC;";

                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("idno", normalTRDidno);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var distinctProjectIDs = datatable.AsEnumerable()
                                                          .Select(row => row.Field<string>("ProjectId"))
                                                          .Distinct()
                                                          .ToList();

                    List<DataRow> TRDorderstoupload = new List<DataRow>();

                    foreach (string projid in distinctProjectIDs)
                    {
                        var doctypeids = datatable.AsEnumerable()
                                         .Where(row => row.Field<string>("ProjectId") == projid)
                                         .Select(row => row.Field<int>("doctypeid"))
                                         .Distinct().ToList();


                        foreach (var docid in doctypeids)
                        {
                            TRDorderstoupload = datatable.AsEnumerable()
                                                     .Where(row => row.Field<string>("ProjectId") == projid && row.Field<int>("doctypeid") == docid)
                                                     .ToList();

                            var docname = datatable.AsEnumerable()
                                                    .Where(row => row.Field<int>("doctypeid") == docid)
                                                    .Select(row => row.Field<string>("DocType"))
                                                    .FirstOrDefault();

                            if (TRDorderstoupload.Any())
                            {
                                InsertIntoSqlServer(TRDorderstoupload.CopyToDataTable(), projid, docid, docname);

                            }
                        }

                    }

                    int idValue = normalTRDidno;
                    if (datatable.Rows.Count > 0)
                    {
                        DataRow lastRow = datatable.Rows[datatable.Rows.Count - 1];
                        idValue = (int)lastRow["id"];
                    }

                    // call method to update the last id of trd order uploaded in trdtrack table

                    UpdateNormalTrdOrdersId(idValue);
                }
            }
            catch (Exception ex)
            {
                string toEmailIds = ConfigurationManager.AppSettings["ToEmailIds"];

                EmailDetails sendEmail = new EmailDetails
                {
                    ToEmailIds = toEmailIds?.Split(',').Select(email => email.Trim()).ToList() ?? new List<string>(),
                    Subject = "Trd Orders - Fetching normal trd orders from postgres",
                    Body = $"TrdIntegrator webjob failed with the following exception:  {ex.Message}",
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmail);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var webApiUrl = new Uri(Url);
                    var response = client.PostAsync(webApiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = response.Content.ReadAsStringAsync().Result;

                    }
                }
                throw;
            }
        }

        public static void InsertIntoSqlServer(DataTable TRDorderstoupload, string projid, int docid, string docname)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string skillsetDetailsQuery = @"SELECT DISTINCT tm.SkillSetId,ss.SkillSetName,ss.SystemofRecordId,STRING_AGG(CASE WHEN dt.IsDuplicateCheck = 1 THEN dt.DefaultColumnName END, ', ') AS Columns 
                                 FROM TrdMap tm
                                 INNER JOIN TemplateColumns TC ON TC.SkillSetId = tm.SkillSetId
                                 INNER JOIN SkillSet ss ON ss.SkillSetId = tm.SkillSetId
                                 INNER JOIN DefaultTemplateColumns dt on dt.SystemofRecordId = ss.SystemofRecordId
                                 WHERE tm.IsActive = 1 AND tm.DoctypeId = @docid AND tm.ProjectId = @projid
                                 GROUP BY  tm.SkillSetId,ss.SkillSetName ,ss.SystemofRecordId; 
                                 SELECT DocumentName,TrdDocTypeId FROM DocType WHERE TrdDocTypeId = @docid AND IsActive = 1";

                    SqlCommand skillSetCommand = new SqlCommand(skillsetDetailsQuery, connection);

                    skillSetCommand.Parameters.AddWithValue("@docid", docid);
                    skillSetCommand.Parameters.AddWithValue("@projid", projid);

                    SqlDataAdapter skillsetdetailsAdapter = new SqlDataAdapter(skillSetCommand);
                    DataSet skillsetds = new DataSet();

                    skillsetdetailsAdapter.Fill(skillsetds);

                    DataTable skillsetdt = skillsetds.Tables[0];
                    DataTable doctypedt = skillsetds.Tables[1];

                    int skillsetid = 0;
                    string SkillSetName = "";
                    string duplicatecol = "";
                    string DocumentName = "";
                    int TrdDocTypeId = 0;


                    if (skillsetdt.Rows.Count > 0)
                    {
                        DataRow row = skillsetdt.Rows[0];

                        skillsetid = Convert.ToInt32(row["SkillSetId"]);
                        SkillSetName = row["SkillSetName"].ToString();
                        duplicatecol = row["Columns"].ToString();

                    }
                    if (doctypedt.Rows.Count > 0)
                    {
                        DataRow dataRow = doctypedt.Rows[0];

                        TrdDocTypeId = Convert.ToInt32(dataRow["TrdDocTypeId"]);
                        DocumentName = dataRow["DocumentName"].ToString();

                    }

                    if (TrdDocTypeId <= 0 && string.IsNullOrEmpty(DocumentName))
                    {
                        string createdoctype = $@"INSERT INTO DocType (DocumentName, IsActive, TrdDocTypeId) VALUES (@docname,1,@TrdDocTypeId)";

                        SqlCommand createdoctypecmd = new SqlCommand(createdoctype, connection);

                        createdoctypecmd.Parameters.AddWithValue("@docname", docname);
                        createdoctypecmd.Parameters.AddWithValue("@TrdDocTypeId", docid);

                        createdoctypecmd.ExecuteNonQuery();
                    }

                    if (skillsetid <= 0 && string.IsNullOrEmpty(SkillSetName))
                    {
                        string modifiedDocname = Regex.Replace(docname, @"[^a-zA-Z0-9_]", "_");
                        SkillSetName = projid + "_" + modifiedDocname;

                        SqlCommand CreateTrdDetails = new SqlCommand("CreateTrdDetails", connection);
                        CreateTrdDetails.CommandType = CommandType.StoredProcedure;

                        CreateTrdDetails.Parameters.AddWithValue("@ProjectId", projid);
                        CreateTrdDetails.Parameters.AddWithValue("@Skillsetname", SkillSetName);
                        CreateTrdDetails.Parameters.AddWithValue("@DocTypeId", docid);

                        CreateTrdDetails.Parameters.Add("@DupCol", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
                        CreateTrdDetails.Parameters.Add("@SkillSetId", SqlDbType.Int).Direction = ParameterDirection.Output;

                        SqlParameter returnTrdValue = new SqlParameter
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };

                        CreateTrdDetails.Parameters.Add(returnTrdValue);

                        CreateTrdDetails.ExecuteNonQuery();

                        duplicatecol = (string)CreateTrdDetails.Parameters["@DupCol"].Value;
                        skillsetid = (int)CreateTrdDetails.Parameters["@SkillSetId"].Value;

                        int returnCode = (int)CreateTrdDetails.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while creating TRD details for " + SkillSetName + " .");
                        }
                        else
                        {
                            Console.WriteLine("TRD details succesfully created for " + SkillSetName + " .");
                        }
                    }

                    List<string> duplicatecolumnsList = duplicatecol.Split(',').Select(s => s.Trim()).ToList();  // for duplicate check

                    List<DataRow> TRDdataRowList = TRDorderstoupload.AsEnumerable().ToList();

                    List<JObject> TRDjObjectList = TRDdataRowList
                                                .Select(row =>
                                                {
                                                    JObject jObject = new JObject();

                                                    foreach (DataColumn column in row.Table.Columns)
                                                    {
                                                        if (column.ColumnName != "id" && column.ColumnName != "doctypeid")
                                                        {
                                                            jObject[column.ColumnName] = JToken.FromObject(row[column]);
                                                        }
                                                    }

                                                    return jObject;
                                                })
                                                .ToList();


                    JObject TRDrecordsObject = new JObject();
                    TRDrecordsObject["Records"] = JArray.FromObject(TRDjObjectList);

                    // Serialize the records to a JSON string - to insert into templates
                    string TRDjsonToInsert = TRDrecordsObject.ToString();

                    //parse json data - to validate duplicate records
                    JObject TRDjsondata = JObject.Parse(TRDjsonToInsert);
                    JArray TRDrecordsarray = TRDjsondata.Value<JArray>("Records");

                    // duplicate check

                    string duplchckquery = $"SELECT * FROM {SkillSetName} WHERE ";

                    foreach (JObject TRDrecords in TRDrecordsarray)
                    {
                        string duplchck = "(";
                        foreach (string DuplicateColumnname in duplicatecolumnsList)
                        {
                            string columndata = TRDrecords.Value<string>(DuplicateColumnname);

                            duplchck += $"[{DuplicateColumnname}] = '{columndata}' AND ";
                        }

                        duplchck = duplchck.Substring(0, duplchck.Length - 5);
                        duplchck += ") OR ";

                        duplchckquery += duplchck;
                    }

                    duplchckquery = duplchckquery.Substring(0, duplchckquery.Length - 4);

                    SqlDataAdapter DuplicatecheckdataAdapter = new SqlDataAdapter(duplchckquery, connection);

                    DataSet DuplicatecheckDS = new DataSet();

                    DuplicatecheckdataAdapter.Fill(DuplicatecheckDS);

                    DataTable DuplicatecheckDT = DuplicatecheckDS.Tables[0];

                    //var duplicateorders = new HashSet<string>(
                    //                         DuplicatecheckDT.AsEnumerable()
                    //                         .Select(row => row.Field<string>("OrderId")));

                    //var filteredOrders = TRDorderstoupload.AsEnumerable()
                    //                  .Where(row => !duplicateorders.Contains(row.Field<string>("OrderId")));

                    var duplicateorders = new HashSet<(string OrderId, DateTime DocImageDate)>(
                                          DuplicatecheckDT.AsEnumerable()
                                          .Select(row => (
                                              row.Field<string>("OrderId"),
                                              row.Field<DateTime>("DocImageDate").Date // Extract only the Date part
                                          )));

                    IEnumerable<DataRow> filteredOrders;
                    IEnumerable<DataRow> orderstoreplace;

                    if (duplicateorders.Count > 0)
                    {
                        filteredOrders = TRDorderstoupload.AsEnumerable()
                                      .Where(row => !duplicateorders.Contains((
                                          row.Field<string>("OrderId"),
                                          row.Field<DateTime>("DocImageDate").Date // Extract only the Date part
                                      )));

                        orderstoreplace = filteredOrders.Where(row => duplicateorders.Any(dup =>
                                          dup.OrderId == row.Field<string>("OrderId") &&    // Match on OrderId
                                          dup.DocImageDate != row.Field<DateTime>("DocImageDate").Date // Ensure DocImageDate is different
                                          ));
                    }
                    else
                    {
                        filteredOrders = TRDorderstoupload.AsEnumerable();
                        orderstoreplace = Enumerable.Empty<DataRow>();
                    }


                    if (orderstoreplace.Any())
                    {
                        var orderIdList = string.Join(",", orderstoreplace.Select(row => $"'{row.Field<string>("OrderId")}'"));

                        string updatequery = $@"
                                                UPDATE {SkillSetName}
                                                SET DocImageDate = CASE OrderId
                                                    {string.Join(Environment.NewLine, orderstoreplace.Select(row =>
                                                       $"WHEN '{row.Field<string>("OrderId")}' THEN '{row.Field<DateTime>("DocImageDate").ToString("yyyy-MM-dd")}'"))}
                                                END
                                                WHERE OrderId IN ({orderIdList}) AND Status IS NULL;
                                            ";


                        SqlCommand updateordercommand = new SqlCommand(updatequery, connection);

                        updateordercommand.ExecuteNonQuery();
                    }

                    var filteredOrdersList = filteredOrders.ToList();
                    var updatedFilteredOrders = filteredOrders;

                    if (orderstoreplace.Any())
                    {
                        // Remove these orders from filteredOrders
                        updatedFilteredOrders = filteredOrdersList.Except(orderstoreplace);

                    }

                    DataTable filteredOrdersDT = new DataTable();

                    if (updatedFilteredOrders.Any())
                    {
                        // Convert to DataTable if there are rows
                        filteredOrdersDT = updatedFilteredOrders.CopyToDataTable();
                    }

                    filteredOrders = filteredOrdersDT.AsEnumerable();

                    if (filteredOrders.Any())
                    {
                        var recordsList = filteredOrders
                                          .Select(row => filteredOrdersDT.Columns
                                              .Cast<DataColumn>()
                                              .ToDictionary(col => col.ColumnName, col => row[col]))
                                          .ToList();

                        var result = new
                        {
                            Records = recordsList
                        };

                        // Serialize to JSON
                        TRDjsonToInsert = JsonConvert.SerializeObject(result, Formatting.Indented);

                    }
                    else
                    {
                        TRDjsonToInsert = "";
                    }


                    if (!string.IsNullOrEmpty(TRDjsonToInsert))
                    {
                        // insert into templates

                        SqlCommand InsertCmd = new SqlCommand("InsertData", connection);
                        InsertCmd.CommandType = CommandType.StoredProcedure;
                        InsertCmd.Parameters.AddWithValue("@SkillSetId", skillsetid);
                        InsertCmd.Parameters.AddWithValue("@jsonData", TRDjsonToInsert);

                        SqlParameter returnValue = new SqlParameter
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };

                        InsertCmd.Parameters.Add(returnValue);

                        InsertCmd.ExecuteNonQuery();

                        int returnCode = (int)InsertCmd.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while inserting the orders in " + SkillSetName + " template.");
                        }
                        else
                        {
                            Console.WriteLine("TRD orders succesfully loaded in " + SkillSetName + " template.");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertIntoSqlServer method: {ex.Message}");
                throw;
            }
        }

        public static void GetTrdPendingOrders(int pendingTRDidno)
        {
            string Url = "";

            try
            {
                Url = ConfigurationManager.AppSettings["SendEmailUrl"];
                string connectionString = ConfigurationManager.ConnectionStrings["PostgressSqlConnection"].ConnectionString;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $@"Select oo.id,oo.referenceid as ""OrderId"",oo.projectid as ""ProjectId"",oo.docimagedate as ""DocImageDate"",dc.documentname as ""DocType"",oo.doctypeid,oo.status as ""HaStatus"", 'Trailing_Doc_Review' as ""WorkflowStatus"", 1 as ""IsPriority"", 1 as ""IsPending""
                                 from public.tbl_omt_orders_pending oo
                                 inner join public.tbl_doctypes dc on dc.id = oo.doctypeid 
                                 where oo.id > @idno Order By oo.id ASC;";

                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("idno", pendingTRDidno);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    var distinctProjectIDs = datatable.AsEnumerable()
                                                          .Select(row => row.Field<string>("ProjectId"))
                                                          .Distinct()
                                                          .ToList();

                    List<DataRow> TrdPendingorderstoupload = new List<DataRow>();

                    foreach (string projid in distinctProjectIDs)
                    {
                        var doctypeids = datatable.AsEnumerable()
                                         .Where(row => row.Field<string>("ProjectId") == projid)
                                         .Select(row => row.Field<int>("doctypeid"))
                                         .Distinct().ToList();


                        foreach (var docid in doctypeids)
                        {
                            TrdPendingorderstoupload = datatable.AsEnumerable()
                                                     .Where(row => row.Field<string>("ProjectId") == projid && row.Field<int>("doctypeid") == docid)
                                                     .ToList();

                            if (TrdPendingorderstoupload.Any())
                            {
                                InsertPendingOrdersIntoSqlServer(TrdPendingorderstoupload.CopyToDataTable(), projid, docid);

                            }
                        }

                    }

                    int idValue = pendingTRDidno;
                    if (datatable.Rows.Count > 0)
                    {
                        DataRow lastRow = datatable.Rows[datatable.Rows.Count - 1];
                        idValue = (int)lastRow["id"];
                    }

                    // call method to update the last id of trd order uploaded in trdtrack table

                    UpdatePendingTrdOrdersId(idValue);
                }
            }
            catch (Exception ex)
            {
                string toEmailIds = ConfigurationManager.AppSettings["ToEmailIds"];

                EmailDetails sendEmail = new EmailDetails
                {
                    ToEmailIds = toEmailIds?.Split(',').Select(email => email.Trim()).ToList() ?? new List<string>(),
                    Subject = "Trd Orders - Fetching pending trd orders from postgres",
                    Body = $"TrdIntegrator webjob failed with the following exception:  {ex.Message}",
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmail);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var webApiUrl = new Uri(Url);
                    var response = client.PostAsync(webApiUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = response.Content.ReadAsStringAsync().Result;

                    }
                }
                throw;
            }
        }

        public static void InsertPendingOrdersIntoSqlServer(DataTable TrdPendingorderstoupload, string projid, int docid)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string skillsetDetailsQuery = @"SELECT DISTINCT tm.SkillSetId,ss.SkillSetName,ss.SystemofRecordId,STRING_AGG(CASE WHEN dt.IsDuplicateCheck = 1 THEN dt.DefaultColumnName END, ', ') AS Columns 
                                 FROM TrdMap tm
                                 INNER JOIN TemplateColumns TC ON TC.SkillSetId = tm.SkillSetId
                                 INNER JOIN SkillSet ss ON ss.SkillSetId = tm.SkillSetId
                                 INNER JOIN DefaultTemplateColumns dt on dt.SystemofRecordId = ss.SystemofRecordId
                                 WHERE tm.IsActive = 1 AND tm.DoctypeId = @docid AND tm.ProjectId = @projid
                                 GROUP BY  tm.SkillSetId,ss.SkillSetName ,ss.SystemofRecordId";

                    SqlCommand skillSetCommand = new SqlCommand(skillsetDetailsQuery, connection);

                    skillSetCommand.Parameters.AddWithValue("@docid", docid);
                    skillSetCommand.Parameters.AddWithValue("@projid", projid);

                    SqlDataAdapter skillsetdetailsAdapter = new SqlDataAdapter(skillSetCommand);
                    DataSet skillsetds = new DataSet();

                    skillsetdetailsAdapter.Fill(skillsetds);

                    DataTable skillsetdt = skillsetds.Tables[0];

                    int skillsetid = 0;
                    string SkillSetName = "";
                    string duplicatecol = "";

                    if (skillsetdt.Rows.Count > 0)
                    {
                        DataRow row = skillsetdt.Rows[0];

                        skillsetid = Convert.ToInt32(row["SkillSetId"]);
                        SkillSetName = row["SkillSetName"].ToString();
                        duplicatecol = row["Columns"].ToString();
                    }

                    List<DataRow> TRDdataRowList = TrdPendingorderstoupload.AsEnumerable().ToList();

                    List<JObject> TRDjObjectList = TRDdataRowList
                                              .Select(row =>
                                              {
                                                  JObject jObject = new JObject();

                                                  foreach (DataColumn column in row.Table.Columns)
                                                  {
                                                      if (column.ColumnName != "id" && column.ColumnName != "doctypeid")
                                                      {
                                                          jObject[column.ColumnName] = JToken.FromObject(row[column]);
                                                      }
                                                  }

                                                  return jObject;
                                              })
                                              .ToList();


                    JObject TRDrecordsObject = new JObject();
                    TRDrecordsObject["Records"] = JArray.FromObject(TRDjObjectList);

                    // Serialize the records to a JSON string - to insert into templates
                    string TRDjsonToInsert = TRDrecordsObject.ToString();

                    // insert into templates

                    SqlCommand InsertCmd = new SqlCommand("InsertData", connection);
                    InsertCmd.CommandType = CommandType.StoredProcedure;
                    InsertCmd.Parameters.AddWithValue("@SkillSetId", skillsetid);
                    InsertCmd.Parameters.AddWithValue("@jsonData", TRDjsonToInsert);

                    SqlParameter returnValue = new SqlParameter
                    {
                        ParameterName = "@RETURN_VALUE",
                        Direction = ParameterDirection.ReturnValue
                    };

                    InsertCmd.Parameters.Add(returnValue);

                    InsertCmd.ExecuteNonQuery();

                    int returnCode = (int)InsertCmd.Parameters["@RETURN_VALUE"].Value;

                    if (returnCode != 1)
                    {
                        throw new InvalidOperationException("Something went wrong while inserting the orders in " + SkillSetName + " template.");
                    }
                    else
                    {
                        Console.WriteLine("TRD Pending orders succesfully loaded in " + SkillSetName + " template.");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertPendingOrdersIntoSqlServer method: {ex.Message}");
                throw;
            }
        }

        public static int GetLastNormalTrdOrdersId()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT TrackTrdId FROM TrackTrdOrders WHERE Id = 1 AND IsActive = 1 AND TrdOrderType = 'Normal'";

                    SqlCommand cmd = new SqlCommand(sql, connection);

                    connection.Open();

                    object result = cmd.ExecuteScalar();

                    int trackTrdId = Convert.ToInt32(result);

                    return trackTrdId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLastNormalTrdOrdersId method: {ex.Message}");
                throw;
            }
        }
        public static void UpdateNormalTrdOrdersId(int idValue)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    DateTime dateTime = DateTime.Now;

                    string sql = $"UPDATE TrackTrdOrders SET TrackTrdId = @idValue,CreatedDate = @dateTime,IsActive = 1 WHERE Id = 1";

                    SqlCommand cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@idValue", idValue);
                    cmd.Parameters.AddWithValue("@dateTime", dateTime);

                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateNormalTrdOrdersId method: {ex.Message}");
                throw;
            }
        }

        public static int GetLastPendingTrdOrdersId()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"SELECT TrackTrdId FROM TrackTrdOrders WHERE Id = 2 AND IsActive = 1 AND TrdOrderType = 'Pending'";

                    SqlCommand cmd = new SqlCommand(sql, connection);

                    connection.Open();

                    object result = cmd.ExecuteScalar();

                    int trackTrdId = Convert.ToInt32(result);

                    return trackTrdId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLastPendingTrdOrdersId method: {ex.Message}");
                throw;
            }
        }

        public static void UpdatePendingTrdOrdersId(int idValue)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    DateTime dateTime = DateTime.Now;

                    string sql = $"UPDATE TrackTrdOrders SET TrackTrdId = @idValue,CreatedDate = @dateTime,IsActive = 1 WHERE Id = 2";

                    SqlCommand cmd = new SqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@idValue", idValue);
                    cmd.Parameters.AddWithValue("@dateTime", dateTime);

                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePendingTrdOrdersId method: {ex.Message}");
                throw;
            }
        }

    }
}
