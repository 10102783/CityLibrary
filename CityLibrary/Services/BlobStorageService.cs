using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CityLibrary.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(string connectionString, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName.ToLower(); // Ensure the container name is lowercase
        }

        public async Task UploadBlobAsync(string blobName, Stream data)
        {
            blobName = SanitizeBlobName(blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(data, overwrite: true);
        }

        public async Task<Stream> DownloadBlobAsync(string blobName)
        {
            blobName = SanitizeBlobName(blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);
            ms.Position = 0;
            return ms;
        }

        public async Task DeleteBlobAsync(string blobName)
        {
            blobName = SanitizeBlobName(blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<IEnumerable<string>> ListBlobsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobs = new List<string>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }

            return blobs;
        }

        private string SanitizeBlobName(string blobName)
        {
            return Uri.EscapeDataString(blobName);
        }
    }
}
