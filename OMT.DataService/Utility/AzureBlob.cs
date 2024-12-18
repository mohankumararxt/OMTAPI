using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using Azure.Storage;

namespace OMT.DataService.Utility
{
    public  class AzureBlob
    {

        public string DownloadBlobUsingSasToken(string connectionString, string containerName, string blobName, string downloadFilePath)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Get the container client
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            containerClient.CreateIfNotExistsAsync();

            // Get the blob client
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set SAS permissions and expiry time (default to 1 hour from now)
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // 'b' for blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // Default expiry of 1 hour
            };

            // Define permissions (read)
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(
                "<your-account-name>",
                "<your-account-key>"
            )).ToString();

            // Generate the Blob Uri with SAS token
            var blobUriWithSas = new Uri($"{blobClient.Uri}?{sasToken}");

            // Create a new BlobClient using the SAS URI
            var blobClientWithSas = new BlobClient(blobUriWithSas);

            // Download the blob to the specified file path
            Console.WriteLine($"Downloading blob to {downloadFilePath}...");
            blobClientWithSas.DownloadToAsync(downloadFilePath).Wait(); // Wait for the async task to complete

            return "Download completed successfully.";
        }


    }
}
