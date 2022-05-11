using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Notifications.Infrastructure.BlobStorage
{
    public class BlobStorageService
    {
        private string folderPath = string.Empty;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        public BlobStorageService(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public async Task<string> SaveHostedFile(string fileNameOnBlob,string fileName, string folderPath)
        { 
            var apiKey = _configuration["BlobStorage:ConectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(apiKey);

            string containerName = _configuration["BlobStorage:ContainerName"];
            
            string localFilePath = Path.Combine(folderPath, fileName);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileNameOnBlob);
            containerClient.GetBlobs();
            
            var result = await blobClient.UploadAsync(localFilePath, true);

            var maxNumberOfRegisters = 5;
            int.TryParse(_configuration["BlobStorage:MaxNumberOfLogsStored"],out maxNumberOfRegisters);
            await DeleteABlobAfter(containerClient, maxNumberOfRegisters);
            return result.ToString();
        }

        public async Task<Pageable<BlobItem>> DeleteABlobAfter(int maxNumOfRegisters)
        {
            var apiKey = _configuration["BlobStorage:ConectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(apiKey);

            string containerName = _configuration["BlobStorage:ContainerName"];

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var result = containerClient.GetBlobs();
            if (result.Count() > maxNumOfRegisters) { 
                var ordered = result.OrderBy(x => x.Properties.CreatedOn);
                var rangeToDelete = result.Count() - maxNumOfRegisters;
                var filesToDelete = ordered.Take(rangeToDelete);
                foreach (var item in filesToDelete)
                {
                    await containerClient.DeleteBlobIfExistsAsync(item.Name, DeleteSnapshotsOption.IncludeSnapshots);
                }
            }
            return result;
        }
        public async Task<Pageable<BlobItem>> DeleteABlobAfter(BlobContainerClient blobContainerClient,int maxNumOfRegisters)
        {
            var result = blobContainerClient.GetBlobs();
            if (result.Count() > maxNumOfRegisters)
            {
                var ordered = result.OrderBy(x => x.Properties.CreatedOn);
                var rangeToDelete = result.Count() - maxNumOfRegisters;
                var filesToDelete = ordered.Take(rangeToDelete);
                foreach (var item in filesToDelete)
                {
                    await blobContainerClient.DeleteBlobIfExistsAsync(item.Name, DeleteSnapshotsOption.IncludeSnapshots);
                }
            }
            return result;
        }
    }
}