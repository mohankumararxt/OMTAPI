using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;


namespace DailyInvoiceGenerator
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

            //call method to exceute invoice storeprocedure
            callInvoiceSp();


        }

        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public static void callInvoiceSp()
        {
            string Url = ConfigurationManager.AppSettings["SendEmailUrl"];
            string toEmailIds = ConfigurationManager.AppSettings["ToEmailIds"];
            var sp = "";

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                                SELECT DISTINCT SS.SkillSetId, SS.SystemofRecordId ,SS.SkillsetName
                                FROM skillset SS
                                INNER JOIN templatecolumns TC ON TC.skillsetid = SS.skillsetid
                                WHERE SS.isactive = 1 AND SS.InvoiceMandatory = 1 
                                ORDER BY SystemofRecordId, SkillSetId";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataSet dataset = new DataSet();

                    dataAdapter.Fill(dataset);

                    DataTable datatable = dataset.Tables[0];

                    foreach (DataRow row in datatable.Rows)
                    {
                        int skillSetId = Convert.ToInt32(row["SkillSetId"]);
                        int systemOfRecordId = Convert.ToInt32(row["SystemofRecordId"]);
                        string tabelname = row["SkillsetName"].ToString();

                        if (systemOfRecordId == 3)
                        {
                            DateTime utcNow = DateTime.UtcNow;
                            DateTime yesterday = utcNow.Date.AddDays(-1).AddHours(11).AddMinutes(30);
                            DateTime today = utcNow.Date.AddHours(11).AddMinutes(30);

                            string updateAllocationdate = $"UPDATE {tabelname} SET AllocationDate = @yesterday WHERE CompletionDate BETWEEN @starttime AND @endtime";

                            SqlCommand upaldate = new SqlCommand(updateAllocationdate, connection);
                            upaldate.CommandType = CommandType.Text;

                            upaldate.Parameters.AddWithValue("@yesterday", yesterday.Date);
                            upaldate.Parameters.AddWithValue("@starttime", yesterday);
                            upaldate.Parameters.AddWithValue("@endtime", today);

                            upaldate.ExecuteNonQuery();

                            sp = "GetInvoice_TRD";
                        }

                        if (systemOfRecordId == 1)
                        {
                            sp = "GetInvoice_SCI";
                        }

                        if (systemOfRecordId == 2)
                        {
                            sp = "GetInvoice_Resware";
                        }

                        if (systemOfRecordId == 4)
                        {
                            sp = "GetInvoice_TIQE";
                        }
                        // Call the stored procedure with parameters derived from the current row
                        using (SqlCommand spCommand = new SqlCommand(sp, connection))
                        {
                            spCommand.CommandType = CommandType.StoredProcedure;

                            // Add parameters
                            spCommand.Parameters.Add("@SkillSetId", SqlDbType.Int).Value = skillSetId;
                            spCommand.Parameters.Add("@SystemOfRecordId", SqlDbType.Int).Value = systemOfRecordId;
                            spCommand.CommandTimeout = 300;

                            SqlParameter returnValue = new SqlParameter
                            {
                                ParameterName = "@RETURN_VALUE",
                                Direction = ParameterDirection.ReturnValue
                            };

                            spCommand.Parameters.Add(returnValue);
                            spCommand.ExecuteNonQuery();

                            int returnCode = (int)spCommand.Parameters["@RETURN_VALUE"].Value;

                            if (returnCode != 1)
                            {
                                throw new InvalidOperationException("Stored Procedure call failed.");
                            }
                            else
                            {
                                Console.WriteLine($"Invoice details of {tabelname} uploaded successfully in InvoiceDump table.");
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                EmailDetails sendEmail = new EmailDetails
                {
                    ToEmailIds = toEmailIds?.Split(',').Select(email => email.Trim()).ToList() ?? new List<string>(),
                    Subject = "Invoice webjob status",
                    Body = $"Invoice webjob failed with following exception: {ex.Message}",
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

    }
}
