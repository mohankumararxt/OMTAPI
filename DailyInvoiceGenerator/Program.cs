using Microsoft.Azure.WebJobs;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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

        public static void callInvoiceSp()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            { 
                connection.Open();

                string query = @"
                                SELECT DISTINCT SS.SkillSetId, SS.SystemofRecordId ,SS.SkillsetName
                                FROM skillset SS
                                INNER JOIN templatecolumns TC ON TC.skillsetid = SS.skillsetid
                                WHERE SS.isactive = 1 
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

                    if (systemOfRecordId == 3)
                    {
                        string tabelname = row["SkillsetName"].ToString();

                        DateTime utcNow = DateTime.UtcNow;
                        DateTime yesterday = utcNow.Date.AddDays(-1).AddHours(12);
                        DateTime today = utcNow.Date.AddHours(12);

                        string updateAllocationdate = $"UPDATE {tabelname} SET AllocationDate = @yesterday WHERE CompletionDate BETWEEN @starttime AND @endtime";

                        SqlCommand upaldate = new SqlCommand(updateAllocationdate, connection);
                        upaldate.CommandType = CommandType.Text;
                       
                        upaldate.Parameters.AddWithValue("@yesterday", yesterday.Date);
                        upaldate.Parameters.AddWithValue("@starttime", yesterday);
                        upaldate.Parameters.AddWithValue("@endtime", today);

                        upaldate.ExecuteNonQuery();
                    }
                        // Call the stored procedure with parameters derived from the current row
                        using (SqlCommand spCommand = new SqlCommand("GetInvoice", connection))
                        {
                            spCommand.CommandType = CommandType.StoredProcedure;

                            // Add parameters
                            spCommand.Parameters.Add("@SkillSetId", SqlDbType.Int).Value = skillSetId;
                            spCommand.Parameters.Add("@SystemOfRecordId", SqlDbType.Int).Value = systemOfRecordId;

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
                                Console.WriteLine("Invoice details uploaded successfully in InvoiceDump table.");
                            }

                        }
                }       
                 
                

            }

        }

    }
}
