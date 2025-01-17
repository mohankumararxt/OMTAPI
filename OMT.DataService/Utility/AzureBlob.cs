using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OMT.DataService.Utility
{
    public class AzureBlob
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlob(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        // Upload file to a blob
        public async Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        // Generate a SAS token for a blob
        public string GetBlobSasUri(string containerName, string AccountKey, string blobName, DateTimeOffset expiryTime, BlobSasPermissions permissions)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if the blob exists
            if (!blobClient.Exists())
                throw new FileNotFoundException($"Blob '{blobName}' does not exist in container '{containerName}'.");

            // Generate the SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // 'b' for blob
                ExpiresOn = expiryTime
            };

            sasBuilder.SetPermissions(permissions);

            // Get account credentials
            var storageAccountName = _blobServiceClient.AccountName;

            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, AccountKey);

            return $"{blobClient.Uri}?{sasBuilder.ToSasQueryParameters(sharedKeyCredential)}";
        }

        // Delete a blob
        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if the blob exists
            if (!blobClient.Exists())
                throw new FileNotFoundException($"Blob '{blobName}' does not exist in container '{containerName}'.");

            // Delete the blob
            var response = await blobClient.DeleteIfExistsAsync();

            return response.Value; // Returns true if the blob was deleted
        }
    }
}
