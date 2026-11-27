using Azure.Storage.Blobs;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

class Program
{
    static void Main()
    {
        // Build configuration
        var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Read settings
        string sqlConnString = config.GetConnectionString("DefaultConnection") ??"";
        string blobConnString = config["BlobStorage:ConnectionString"]??"";
        string containerName = config["BlobStorage:ContainerName"] ?? "";

        // SQL connection
        string ListQuery = "SELECT * FROM ArchiveTableList WHERE IsActive=1";

        // Fetch data
        DataTable ArchiveListdt = new DataTable();
        
        using (SqlConnection conn = new SqlConnection(sqlConnString))
        {
            SqlDataAdapter adapter = new SqlDataAdapter(ListQuery, conn);
            adapter.Fill(ArchiveListdt);
        }

        foreach(DataRow row in ArchiveListdt.Rows)
        {
            Console.WriteLine("Table Name : " + row["TableName"].ToString() +", Column Name : " + row["ColumnName"].ToString());

            DateTime currentDate = DateTime.Now;
            DateTime threeMonthsAgo = currentDate.AddMonths(-3);

            int monthValue = threeMonthsAgo.Month; // Gives the month number (1–12)
            string monthName = threeMonthsAgo.ToString("MMMM"); // Gives the full month name
            string Year = threeMonthsAgo.ToString("yyyy"); // Gives the year

            string strDayofmonth = Year + "-" + monthValue.ToString() + "-1";

            DataTable TableData = new DataTable();
            //string TableDataListQuery = "SELECT * FROM " + row["TableName"].ToString() + " Where " + row["ColumnName"].ToString() + " between '" + strDayofmonth + "' and '" + strendofmonth + "' order by " + row["ColumnName"].ToString();
            string TableDataListQuery = "SELECT * FROM " + row["TableName"].ToString() + " Where " + row["ColumnName"].ToString() + " < '" + strDayofmonth + "' order by " + row["ColumnName"].ToString();
            string TableDataDeleteQuery = "DELETE FROM " + row["TableName"].ToString() + " Where " + row["ColumnName"].ToString() + " < '" + strDayofmonth + "'";

            using (SqlConnection conn = new SqlConnection(sqlConnString))
            {
                // Upload to Azure Blob Storage - excel file name format: Year/Month/TableName_Month.xlsx
                string blobName = Year +"/" + monthName + "/" + row["TableName"].ToString()+"_" + monthName + ".xlsx";
                //string blobName = Year + "/till_april2026/" + row["TableName"].ToString() + "_tillapril2026.xlsx";

                SqlDataAdapter adapter = new SqlDataAdapter(TableDataListQuery, conn);
                adapter.Fill(TableData);

                // Create Excel workbook
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(TableData, "Sheet1");

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        stream.Position = 0;
                        BlobContainerClient container = new BlobContainerClient(blobConnString, containerName);
                        container.CreateIfNotExists();

                        BlobClient blob = container.GetBlobClient(blobName);
                        blob.Upload(stream, overwrite: true);
                    }
                }

                //Console.WriteLine(blobName + " Created Successfully.");

                // delete the archived data from the table.

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using (SqlCommand cmd = new SqlCommand(TableDataDeleteQuery, conn))
                {
                    cmd.CommandTimeout = 1000; // Set command timeout to 1000 seconds
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} record(s) deleted.");
                }
            }



        }

    }
}