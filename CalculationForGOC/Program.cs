using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CalculationForGOC
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

            //call method to update goc table
            DailyCalculation_Threshold();
        }
        public class EmailDetails
        {
            public List<string> ToEmailIds { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public static void DailyCalculation_Threshold()
        {
            string EmailUrl = "";
            EmailUrl = ConfigurationManager.AppSettings["SendEmailUrl"];

            string GocUrl = "";
            GocUrl = ConfigurationManager.AppSettings["UpdateGoc"];
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var webApiUrl = new Uri(GocUrl);
                    var response = client.PostAsync(webApiUrl, null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = response.Content.ReadAsStringAsync().Result;

                        Console.WriteLine("UpdateGetOrderCalculation API call successful. Response: " + responseData);
                    }
                    else
                    {
                        Console.WriteLine(" UpdateGetOrderCalculation API call failed. Status Code: " + response.StatusCode);
                    }


                }

            }
            catch (Exception ex)
            {
                string toEmailIds = ConfigurationManager.AppSettings["ToEmailIds"];

                EmailDetails sendEmail = new EmailDetails
                {
                    ToEmailIds = toEmailIds?.Split(',').Select(email => email.Trim()).ToList() ?? new List<string>(),
                    Subject = "Daily update of getordercalculation table.",
                    Body = $"CalculationForGOC webjob failed with the following exception:  {ex.Message}",
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
