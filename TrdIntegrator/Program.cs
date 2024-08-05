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

            GetTrdOrders();
        }
        public static void GetTrdOrders()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PostgressSqlConnection"].ConnectionString;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = @"select * from public.tbl_omt_orders";

                NpgsqlCommand command = new NpgsqlCommand(query, connection);

                NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                DataSet dataset = new DataSet();

                dataAdapter.Fill(dataset);

                DataTable datatable = dataset.Tables[0];

                var distinctProjectIDs = datatable.AsEnumerable()
                                                      .Select(row => row.Field<string>("projectid"))
                                                      .Distinct()
                                                      .ToList();


                foreach (string projid in distinctProjectIDs)
                {
                    var doctypeids = datatable.AsEnumerable()
                                     .Where(row => row.Field<string>("projectid") == projid)
                                     .Select(row => row.Field<int>("doctypeid"))
                                     .Distinct().ToList();

                    foreach (var docid in doctypeids)
                    {
                        foreach (DataRow row1 in datatable.Rows)
                        {
                            var orderstoupload = datatable.AsEnumerable()
                                                 .Where(row => row.Field<string>("projectid") == projid && row.Field<int>("doctypeid") == docid)
                                                 .ToList();

                            if (orderstoupload.Any())
                            {

                                int trackid = InsertIntoSqlServer(orderstoupload.CopyToDataTable(), projid, docid);
                            }
                        }
                    }

                }

            }
        }

        public static int InsertIntoSqlServer(DataTable orderstoupload,string projid,int docid)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return 1;
            }
        }
    }
}
