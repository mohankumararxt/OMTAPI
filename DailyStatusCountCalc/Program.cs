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

namespace DailyStatusCountCalc
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
            DailyStatusCountCalculation();
        }
        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public static void DailyStatusCountCalculation()
        {
            string EmailUrl = "";
            EmailUrl = ConfigurationManager.AppSettings["SendEmailUrl"];


            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;


                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    string insertQuery = @"
                                          INSERT INTO Daily_Status_Count (SystemOfRecordId, SkillSetId, Date, Status, Count)
                                          SELECT 
                                              SystemOfRecordId,
                                              SkillSetId,
                                              Productivity_Date, 
                                              Status,
                                              COUNT(*) AS Count
                                          FROM 
                                              Prod_Util_Tracker 
                                          WHERE 
                                              Productivity_Date  = CAST(DATEADD(DAY, -1, GETUTCDATE()) AS DATE)
                                          GROUP BY 
                                              SystemOfRecordId,
                                              SkillSetId,
                                              Productivity_Date ,
                                              Status;";

                    using (SqlCommand spCommand3 = new SqlCommand(insertQuery, connection))
                    {
                        spCommand3.CommandType = CommandType.Text;
                        spCommand3.ExecuteNonQuery();
                        Console.WriteLine($"Daily count status table updated.");
                    }

                    //back up Prod_Util_Tracker table and then delete the same data form it.

                    string prod_util_tracker_bckp = @"INSERT INTO Prod_Util_Tracker_bckp (UserId,OrderId,Status,SkillSetId,SystemofRecordId,StartDate,EndDate,TimeTaken,Productivity_Date)
	                                          SELECT 
	                                            UserId,OrderId,Status,SkillSetId,SystemofRecordId,StartDate,EndDate,TimeTaken,Productivity_Date
	                                          FROM Prod_Util_Tracker WHERE Productivity_Date  = CAST(DATEADD(DAY, -2, GETUTCDATE()) AS DATE)";

                    using (SqlCommand spCommand4 = new SqlCommand(prod_util_tracker_bckp, connection))
                    {
                        spCommand4.CommandType = CommandType.Text;
                        spCommand4.ExecuteNonQuery();
                        Console.WriteLine($"Prod_util_tracker table has been backed up successfully.");
                    }

                    // delete the data

                    string delete = @"DELETE FROM Prod_Util_Tracker WHERE CAST(Productivity_Date AS DATE) = CAST(DATEADD(DAY, -2, GETUTCDATE()) AS DATE);";

                    using (SqlCommand spCommand5 = new SqlCommand(delete, connection))
                    {
                        spCommand5.CommandType = CommandType.Text;
                        spCommand5.ExecuteNonQuery();
                        Console.WriteLine($"Two days before data has been removed from Prod_util_tracker table successfully.");
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