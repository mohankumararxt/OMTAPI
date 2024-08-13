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

            int idno = GetLastId();
            GetTrdOrders(idno);
        }
        public static void GetTrdOrders(int idno)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PostgressSqlConnection"].ConnectionString;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = $@"Select oo.id,oo.referenceid as ""OrderId"",oo.projectid as ""ProjectId"",oo.docimagedate as ""DocImageDate"",dc.documentname as ""DocType"",oo.doctypeid,oo.status as ""HaStatus"", 'Trailing_Doc_Review' as ""WorkflowStatus"", 1 as ""IsPriority""
                                 from public.tbl_omt_orders oo
                                 inner join public.tbl_doctypes dc on dc.id = oo.doctypeid 
                                 where oo.id > @idno Order By oo.id ASC;";

                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("idno", idno);

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

                        if (TRDorderstoupload.Any())
                        {
                            InsertIntoSqlServer(TRDorderstoupload.CopyToDataTable(), projid, docid);

                        }
                    }

                }

                int idValue = 0;
                if (datatable.Rows.Count > 0)
                {
                    DataRow lastRow = datatable.Rows[datatable.Rows.Count - 1];
                    idValue = (int)lastRow["id"];
                }

                // call method to update the last id of trd order uploaded in trdtrack table

                updateid(idValue);
            }
        }

        public static void InsertIntoSqlServer(DataTable TRDorderstoupload, string projid, int docid)
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

                var duplicateorders = new HashSet<string>(
                                         DuplicatecheckDT.AsEnumerable()
                                         .Select(row => row.Field<string>("OrderId")));

                var filteredOrders = TRDorderstoupload.AsEnumerable()
                                    .Where(row => !duplicateorders.Contains(row.Field<string>("OrderId")));

                if (filteredOrders.Any())
                {
                    var recordsList = filteredOrders
                                      .Select(row => TRDorderstoupload.Columns
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

        public static int GetLastId()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT TrackTrdId FROM TrackTrdOrders WHERE Id = 1 AND IsActive = 1";

                SqlCommand cmd = new SqlCommand(sql, connection);

                connection.Open();

                object result = cmd.ExecuteScalar();

                int trackTrdId = Convert.ToInt32(result);

                return trackTrdId;
            }
        }
        public static void updateid(int idValue)
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
    }
}
