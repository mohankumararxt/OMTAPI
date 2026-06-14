using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
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
                                              CheckIn_date, 
                                              Status,
                                              COUNT(*) AS Count
                                          FROM 
                                              Prod_Util_Tracker 
                                          WHERE 
                                              CheckIn_date  = CAST(DATEADD(DAY, -1, GETUTCDATE()) AS DATE)
                                          GROUP BY 
                                              SystemOfRecordId,
                                              SkillSetId,
                                              CheckIn_date,
                                              Status;";

                    using (SqlCommand spCommand3 = new SqlCommand(insertQuery, connection))
                    {
                        spCommand3.CommandType = CommandType.Text;
                        spCommand3.ExecuteNonQuery();
                        Console.WriteLine($"Daily count status table updated.");
                    }

                    //back up Prod_Util_Tracker table and then delete the same data form it.

                    string prod_util_tracker_bckp = @"INSERT INTO Prod_Util_Tracker_bckp (UserId,OrderId,Status,SkillSetId,SystemofRecordId,StartDate,EndDate,TimeTaken,Productivity_Date,CheckIn_date)
	                                          SELECT 
	                                            UserId,OrderId,Status,SkillSetId,SystemofRecordId,StartDate,EndDate,TimeTaken,Productivity_Date,CheckIn_date
	                                          FROM Prod_Util_Tracker WHERE CheckIn_date  = CAST(DATEADD(DAY, -2, GETUTCDATE()) AS DATE)";

                    using (SqlCommand spCommand4 = new SqlCommand(prod_util_tracker_bckp, connection))
                    {
                        spCommand4.CommandType = CommandType.Text;
                        spCommand4.ExecuteNonQuery();
                        Console.WriteLine($"Prod_util_tracker table has been backed up successfully.");
                    }

                    // delete the data

                    string delete = @"DELETE FROM Prod_Util_Tracker WHERE CAST(CheckIn_date AS DATE) = CAST(DATEADD(DAY, -2, GETUTCDATE()) AS DATE);";

                    using (SqlCommand spCommand5 = new SqlCommand(delete, connection))
                    {
                        spCommand5.CommandType = CommandType.Text;
                        spCommand5.ExecuteNonQuery();
                        Console.WriteLine($"Two days before data has been removed from Prod_util_tracker table successfully.");
                    }

                    // insert into monthly status count skillset table

                    string mscs = @"MERGE Monthly_Status_Count_SkillSet AS target
                                    USING (
                                        SELECT 
                                            SystemOfRecordId,
                                            SkillSetId,
                                            MONTH([Date]) AS [Month],
                                            YEAR([Date])  AS [Year],
                                            Status,
                                            SUM([Count]) AS Count
                                        FROM Daily_Status_Count
                                        WHERE 
                                            MONTH([Date]) = MONTH(GETDATE()) 
                                            AND YEAR([Date]) = YEAR(GETDATE())
                                        GROUP BY 
                                            SystemOfRecordId,
                                            SkillSetId,
                                            MONTH([Date]),
                                            YEAR([Date]),
                                            Status
                                    ) AS source
                                    ON (
                                        target.SystemOfRecordId = source.SystemOfRecordId
                                        AND target.SkillSetId    = source.SkillSetId
                                        AND target.Month         = source.Month
                                        AND target.Year          = source.Year
                                        AND target.Status        = source.Status
                                    )
                                    WHEN MATCHED THEN 
                                        UPDATE SET target.Count = source.Count
                                    WHEN NOT MATCHED THEN
                                        INSERT (SystemOfRecordId, SkillSetId, Month, Year, Status, Count)
                                        VALUES (source.SystemOfRecordId, source.SkillSetId, source.Month, source.Year, source.Status, source.Count);";

                    using (SqlCommand spCommand6 = new SqlCommand(mscs, connection))
                    {
                        spCommand6.CommandType = CommandType.Text;
                        spCommand6.ExecuteNonQuery();
                        Console.WriteLine($"Monthly status count skillset table has been updated.");
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