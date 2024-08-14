using Microsoft.Azure.WebJobs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PendingSciOrders
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

            //var host = new JobHost(config);
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            SetSciOrdersToPending();
        }

        public static void SetSciOrdersToPending()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string SciSkillsets = @"SELECT SS.SkillSetId, SS.SystemofRecordId ,SS.SkillsetName,PS.Id
                                        FROM skillset SS
                                        INNER JOIN SciPendingStatusSkillsets SPS ON SPS.SkillSetId = SS.SkillSetId
                                        INNER JOIN ProcessStatus PS ON PS.SystemofRecordId = SS.SystemofRecordId
                                        INNER JOIN TemplateColumns TC ON TC.SkillSetId = SS.SkillSetId
                                        WHERE SS.isactive = 1 AND PS.Status = 'Pending' 
                                        ORDER BY SkillSetId";

                SqlCommand GetSkillsets = new SqlCommand(SciSkillsets, connection);
                SqlDataAdapter SkillsetdataAdapter = new SqlDataAdapter(GetSkillsets);
                DataSet skillsetDS = new DataSet();

                SkillsetdataAdapter.Fill(skillsetDS);

                DataTable SkillsetDT = skillsetDS.Tables[0];

                foreach (DataRow Sciskillset in SkillsetDT.Rows)
                {
                    int statusid = Convert.ToInt32(Sciskillset["Id"]);
                    string skillsetname = Convert.ToString(Sciskillset["SkillsetName"]);

                    string updateToPending = $@"UPDATE {skillsetname} SET Status = @statusid WHERE UserId IS NULL AND Status IS NULL";

                    SqlCommand updateToPN = new SqlCommand(updateToPending, connection);
                    updateToPN.CommandType = CommandType.Text;

                    updateToPN.Parameters.AddWithValue("@statusid", statusid);
                    
                    updateToPN.ExecuteNonQuery();

                    Console.WriteLine("Unassigned orders succesfully updated with pending status in " + skillsetname + " template.");
                }


            }

        }

    }
}
