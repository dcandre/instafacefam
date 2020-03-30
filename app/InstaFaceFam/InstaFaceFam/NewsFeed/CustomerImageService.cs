using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace InstaFaceFam.NewsFeed
{
    public class CustomerImageService : ICustomerImageService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _customerImageStorageUrl;

        public CustomerImageService(string connectionString, string containerName, string customerImageStorageUrl)
        {
            _customerImageStorageUrl = customerImageStorageUrl;
            var blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);            
        }

        public async Task UploadCustomerImageAsync(string customerImageName, Stream stream, string contentType)
        {
            try
            {                
                var blobClient = _blobContainerClient.GetBlobClient(customerImageName);
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            }
            catch(Exception e)
            {
                throw new Exception(e.Message, e);
            }                        
        }

        public string GetCustomerImageUrl(string imageName)
        {
            return _customerImageStorageUrl + imageName;
        }
    }
}
