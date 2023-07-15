namespace EPR.RegistrationValidation.Application.Readers;

using Azure.Storage.Blobs;
using Data.Config;
using Helpers;
using Microsoft.Extensions.Options;

public class BlobReader : IBlobReader
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly StorageAccountConfig _storageAccountConfig;

    public BlobReader(BlobServiceClient blobServiceClient, IOptions<StorageAccountConfig> storageAccountOptions)
    {
        _blobServiceClient = blobServiceClient;
        _storageAccountConfig = storageAccountOptions.Value;
    }

    public MemoryStream DownloadBlobToStream(string name)
    {
        var blobClient = BlobStorageHelper.GetBlobClient(_blobServiceClient, _storageAccountConfig.BlobContainerName, name);
        var memoryStream = new MemoryStream();
        blobClient.DownloadTo(memoryStream);
        return memoryStream;
    }
}