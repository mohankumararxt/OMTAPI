using Azure.Storage.Blobs;
using ClosedXML.Excel;
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
        string ListQuery = "SELECT * FROM ArchiveTableList";

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

            Console.WriteLine("Month -3 (number): " + monthValue);
            Console.WriteLine("Month -3 (name): " + monthName);

            DataTable TableData = new DataTable();
            string TableDataListQuery = "SELECT * FROM "+ row["TableName"].ToString() +" Where " + row["ColumnName"].ToString() +" < '" + threeMonthsAgo +"'";

            using (SqlConnection conn = new SqlConnection(sqlConnString))
            {
                // Upload to Azure Blob Storage
                string blobName = Year +"/" + monthName + "/" + row["TableName"].ToString()+"_" + monthName + ".xlsx";

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

                        Console.WriteLine("Excel file uploaded to Blob Storage: " + blob.Uri);
                    }
                }

                Console.WriteLine(blobName + " Created Successfully.");
            }

        }

        //// Create Excel workbook
        //using (XLWorkbook wb = new XLWorkbook())
        //{
        //    wb.Worksheets.Add(dt, "Sheet1");

        //    using (var stream = new MemoryStream())
        //    {
        //        wb.SaveAs(stream);
        //        stream.Position = 0;

        //        // Upload to Azure Blob Storage
        //        string blobName = "YourTableData.xlsx";

        //        BlobContainerClient container = new BlobContainerClient(blobConnString, containerName);
        //        container.CreateIfNotExists();

        //        BlobClient blob = container.GetBlobClient(blobName);
        //        blob.Upload(stream, overwrite: true);

        //        Console.WriteLine("Excel file uploaded to Blob Storage: " + blob.Uri);
        //    }
        //}
    }
}