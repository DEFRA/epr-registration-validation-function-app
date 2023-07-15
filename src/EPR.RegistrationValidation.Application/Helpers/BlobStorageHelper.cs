namespace EPR.RegistrationValidation.Application.Helpers;

using Azure.Storage.Blobs;

public class BlobStorageHelper
{
    public static BlobClient GetBlobClient(
        BlobServiceClient blobServiceClient,
        string containerName,
        string blobName)
    {
        BlobClient client =
            blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        return client;
    }
}