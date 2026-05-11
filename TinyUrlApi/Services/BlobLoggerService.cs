using Azure.Storage.Blobs;

namespace TinyUrlApi.Services
{
    public class BlobLoggerService
    {
        private readonly IConfiguration _config;

        public BlobLoggerService(IConfiguration config)
        {
            _config = config;
        }

        public async Task LogAsync(string message)
        {
            var connectionString =
                _config["AzureBlobStorage"];

            var containerName = "logs";

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            var containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            //var fileName =
            //    $"log-{DateTime.UtcNow:yyyyMMdd}.txt";
            var fileName =
                $"log-{Guid.NewGuid()}.txt";

            var blobClient =
                containerClient.GetBlobClient(fileName);

            string logMessage =
                $"{DateTime.UtcNow} - {message}\n";

            using var stream =
                new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(logMessage));

            await blobClient.UploadAsync(
                stream,
                overwrite: true);
        }
    }
}
