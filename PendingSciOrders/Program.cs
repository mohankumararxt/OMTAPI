using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();

            SetSciOrdersToPending();
        }

        public static void SetSciOrdersToPending()
        {
            //string connectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();

            //  //  string SciSkillsets = @"";
                                


            //}

        }

    }
}
