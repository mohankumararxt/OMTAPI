using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PendingSciOrders_4._30pm
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {

            //var host = new JobHost(config);
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            SetSciOrdersToPending_4_30pm();
        }

        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public static void SetSciOrdersToPending_4_30pm()
        {

            string Url = "";

            try
            {
                Url = ConfigurationManager.AppSettings["SendEmailUrl"];
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string scheduledTime = ConfigurationManager.AppSettings["Scheduled_Time"];

                    string SciSkillsets = @"SELECT DISTINCT SS.SkillSetId, SS.SystemofRecordId ,SS.SkillsetName,PS.Id
                                            FROM skillset SS
                                            INNER JOIN SciPendingStatusSkillsets SPS ON SPS.SkillSetId = SS.SkillSetId
                                            INNER JOIN ProcessStatus PS ON PS.SystemofRecordId = SS.SystemofRecordId
                                            INNER JOIN TemplateColumns TC ON TC.SkillSetId = SS.SkillSetId
                                            WHERE SS.isactive = 1 AND PS.Status = 'System-Pending'  AND SPS.IsActive =1 AND SPS.Scheduled_Time = @ScheduledTime AND SPS.Scheduled_Days = 'Mon-Sun'
                                            ORDER BY SkillSetId";

                    SqlCommand GetSkillsets = new SqlCommand(SciSkillsets, connection);
                    GetSkillsets.Parameters.AddWithValue("@ScheduledTime", scheduledTime);

                    SqlDataAdapter SkillsetdataAdapter = new SqlDataAdapter(GetSkillsets);
                    DataSet skillsetDS = new DataSet();

                    SkillsetdataAdapter.Fill(skillsetDS);

                    DataTable SkillsetDT = skillsetDS.Tables[0];

                    DateTime dateTime = DateTime.Now;

                    foreach (DataRow Sciskillset in SkillsetDT.Rows)
                    {
                        int SystemofRecordId = 1;
                        int SkillSetId = Convert.ToInt32(Sciskillset["SkillSetId"]);
                        int statusid = Convert.ToInt32(Sciskillset["Id"]);
                        string skillsetname = Convert.ToString(Sciskillset["SkillsetName"]);

                        //get the sp count and update in Daily_system_pending_Count
                        string spcount = $@"SELECT COUNT(Id) FROM {skillsetname} WHERE STATUS IS NULL AND USERID IS NULL";

                        SqlCommand getcount = new SqlCommand(spcount, connection);
                        getcount.CommandType = CommandType.Text;

                        int dspcount = (int)getcount.ExecuteScalar();

                        string updatedspc = $@"INSERT INTO Daily_system_pending_Count (SystemofRecordId,SkillSetId,Date,Count) VALUES (@SystemofRecordId,@SkillSetId,@Date,@Count)";

                        SqlCommand updateToSPN = new SqlCommand(updatedspc, connection);
                        updateToSPN.CommandType = CommandType.Text;

                        updateToSPN.Parameters.AddWithValue("@SystemofRecordId", SystemofRecordId);
                        updateToSPN.Parameters.AddWithValue("@SkillSetId", SkillSetId);
                        updateToSPN.Parameters.AddWithValue("@Date", DateTime.UtcNow.Date.AddDays(-1));
                        updateToSPN.Parameters.AddWithValue("@Count", dspcount);

                        updateToSPN.ExecuteNonQuery();

                        //move to system pending

                        string updateToPending = $@"UPDATE {skillsetname} SET Status = @statusid, CompletionDate = @CompletionDate WHERE UserId IS NULL AND Status IS NULL";

                        SqlCommand updateToPN = new SqlCommand(updateToPending, connection);
                        updateToPN.CommandType = CommandType.Text;

                        updateToPN.Parameters.AddWithValue("@statusid", statusid);
                        updateToPN.Parameters.AddWithValue("@CompletionDate", dateTime);

                        updateToPN.ExecuteNonQuery();

                        Console.WriteLine("Unassigned orders succesfully updated with pending status in " + skillsetname + " template.");

                        //move to systempending bckp table

                        SqlCommand insertToBckp = new SqlCommand("BackupSkillset_SysPen", connection);
                        insertToBckp.CommandType = CommandType.StoredProcedure;

                        SqlParameter returnvalue = new SqlParameter
                        {
                            ParameterName = "@RETURN_VALUE",
                            Direction = ParameterDirection.ReturnValue
                        };

                        insertToBckp.Parameters.Add(returnvalue);

                        insertToBckp.Parameters.AddWithValue("@SkillsetTable", skillsetname);
                        insertToBckp.Parameters.AddWithValue("@StatusId", statusid);
                        insertToBckp.Parameters.AddWithValue("@CompletionDate", dateTime);

                        insertToBckp.ExecuteNonQuery();

                        int returnCode = (int)insertToBckp.Parameters["@RETURN_VALUE"].Value;

                        if (returnCode != 1)
                        {
                            throw new InvalidOperationException("Stored Procedure call failed.");
                        }
                        else
                        {
                            Console.WriteLine($"System pending orders inserted into " + skillsetname + "_Sys_Pen table successfully.");
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
                    Subject = "Trd Orders - Fetching from postgres",
                    Body = $"SciPendingUpdate webjob failed with the following exception:  {ex.Message}",
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