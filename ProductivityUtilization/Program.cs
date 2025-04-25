using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityUtilization
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

            //call method to calculate prod and util
            Productivity_Utilization();
        }
        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public static void Productivity_Utilization()
        {
            string EmailUrl = "";
            EmailUrl = ConfigurationManager.AppSettings["SendEmailUrl"];


            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand spCommand = new SqlCommand("Master_Productivity_Percentage", connection))
                    {
                        connection.Open();
                        spCommand.CommandType = CommandType.StoredProcedure;

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
                            Console.WriteLine($"Productivity_Percentage table updated successfully.");
                        }

                    }

                    using (SqlCommand spCommand2 = new SqlCommand("Calculate_Prod_Util", connection))
                    {
                        spCommand2.CommandType = CommandType.StoredProcedure;

                        SqlParameter returnValue2 = new SqlParameter
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };

                        spCommand2.Parameters.Add(returnValue2);
                        spCommand2.ExecuteNonQuery();

                        int returnCode2 = (int)spCommand2.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode2 != 1)
                        {
                            throw new InvalidOperationException("Stored Procedure call failed.");
                        }
                        else
                        {
                            Console.WriteLine($"Prod_Util table updated successfully.");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string toEmailIds = ConfigurationManager.AppSettings["ToEmailIds"];

                EmailDetails sendEmail = new EmailDetails
                {
                    ToEmailIds = toEmailIds?.Split(',').Select(email => email.Trim()).ToList() ?? new List<string>(),
                    Subject = "Daily update of Productivity_Utilization.",
                    Body = $"Productivity_Utilization webjob failed with the following exception:  {ex.Message}",
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmail);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var webApiUrl = new Uri(EmailUrl);
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