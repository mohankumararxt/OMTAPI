using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Configuration;

namespace CreateInvoiceSummary
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?linkid=2250384
    internal class Program
    {
        // Please set AzureWebJobsStorage connection strings in appsettings.json for this WebJob to run.
        static void Main(string[] args)
        {
            callInvoiceSummarySp();
        }

        public static void callInvoiceSummarySp()
        {

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand spCommand = new SqlCommand("GenerateInvoiceSummary", connection))
                    {
                        spCommand.CommandType = CommandType.StoredProcedure;

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
                            Console.WriteLine($"Invoice summary details uploaded successfully.");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }
    }
}


