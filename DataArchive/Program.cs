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
        string query = "SELECT * FROM userprofile";

        // Fetch data
        DataTable dt = new DataTable();
        
        using (SqlConnection conn = new SqlConnection(sqlConnString))
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            adapter.Fill(dt);
        }

        // Create Excel workbook
        using (XLWorkbook wb = new XLWorkbook())
        {
            wb.Worksheets.Add(dt, "Sheet1");

            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                stream.Position = 0;

                // Upload to Azure Blob Storage
                string blobName = "YourTableData.xlsx";

                BlobContainerClient container = new BlobContainerClient(blobConnString, containerName);
                container.CreateIfNotExists();

                BlobClient blob = container.GetBlobClient(blobName);
                blob.Upload(stream, overwrite: true);

                Console.WriteLine("Excel file uploaded to Blob Storage: " + blob.Uri);
            }
        }
    }
}