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

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            int idno =  GetLastId();
            GetTrdOrders(idno);
        }
        public static void GetTrdOrders(int idno)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PostgressSqlConnection"].ConnectionString;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = $@"select oo.id,oo.referenceid as ""OrderId"",oo.projectid as ""ProjectId"",oo.docimagedate as ""DocImageDate"",dc.documentname as ""DocType"",oo.doctypeid,oo.status as ""HaStatus"", 'Trailing Doc Review' as ""WorkflowStatus"", 1 as ""IsPriority""
                                 from public.tbl_omt_orders oo
                                 inner join public.tbl_doctypes dc on dc.id = oo.doctypeid 
                                 where oo.id > @idno";

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

                List<DataRow> orderstoupload = new List<DataRow>();

                foreach (string projid in distinctProjectIDs)
                {
                    var doctypeids = datatable.AsEnumerable()
                                     .Where(row => row.Field<string>("ProjectId") == projid)
                                     .Select(row => row.Field<int>("doctypeid"))
                                     .Distinct().ToList();

                    foreach (var docid in doctypeids)
                    {
                        orderstoupload = datatable.AsEnumerable()
                                                 .Where(row => row.Field<string>("ProjectId") == projid && row.Field<int>("doctypeid") == docid)
                                                 .ToList();

                        if (orderstoupload.Any())
                        {
                            InsertIntoSqlServer(orderstoupload, projid, docid);
                        }
                    }

                }

                // get id of last record which was fetched from postgres

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

        public static void InsertIntoSqlServer(List<DataRow> orderstoupload,string projid,int docid)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"SELECT DISTINCT tm.SkillSetId,ss.SkillSetName 
                                 FROM TrdMap tm
                                 INNER JOIN TemplateColumns TC ON TC.SkillSetId = tm.SkillSetId
                                 INNER JOIN SkillSet ss ON ss.SkillSetId = tm.SkillSetId
                                 WHERE tm.IsActive = 1 AND tm.DoctypeId = @docid AND tm.ProjectId = @projid";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@docid", docid);
                command.Parameters.AddWithValue("@projid", projid);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataSet dataset = new DataSet();

                dataAdapter.Fill(dataset);

                DataTable datatable = dataset.Tables[0];

                int skillsetid = 0;
                string SkillSetName = "";

                if (datatable.Rows.Count > 0 )
                {
                    DataRow row = datatable.Rows[0];

                    skillsetid = Convert.ToInt32(row["SkillSetId"]);
                    SkillSetName = row["SkillSetName"].ToString();
                }


                List<JObject> jObjectList = orderstoupload
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



                JObject recordsObject = new JObject();
                recordsObject["Records"] = JArray.FromObject(jObjectList);

                // Serialize the records to a JSON string
                string jsonToInsert = recordsObject.ToString();

                SqlCommand cmd = new SqlCommand("InsertData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SkillSetId", skillsetid);
                cmd.Parameters.AddWithValue("@jsonData", jsonToInsert);

                SqlParameter returnValue = new SqlParameter
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
                else
                {
                    Console.WriteLine("TRD orders succesfully loaded in respective template.");
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
        public static void updateid(int idno)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DateTime dateTime = DateTime.Now;

                string sql = $"UPDATE TrackTrdOrders SET TrackTrdId = @idno,CreatedDate = @dateTime,IsActive = 1 WHERE Id = 1";

                SqlCommand cmd = new SqlCommand(sql,connection);
                cmd.Parameters.AddWithValue("@idno", idno);
                cmd.Parameters.AddWithValue("@dateTime", dateTime);

                cmd.ExecuteNonQuery();

            }
        }
    }
}
