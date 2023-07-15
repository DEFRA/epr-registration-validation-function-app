namespace EPR.RegistrationValidation.UnitTests.Helpers;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;

public static class BlobStorageServiceTestsHelper
{
    public static Mock<BlobClient> GetBlobClientMock()
    {
        var value = BlobsModelFactory.BlobContentInfo(new ETag("a"), DateTimeOffset.Now, null, string.Empty, 0);
        var blobClientMock = new Mock<BlobClient>();

        blobClientMock
            .Setup(m => m.UploadAsync(It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(value, new Mock<Response>().Object));

        return blobClientMock;
    }

    public static Mock<BlobContainerClient> GetBlobContainerClientMock(BlobClient blobClient)
    {
        var blobContainerClientMock = new Mock<BlobContainerClient>();

        blobContainerClientMock
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClient);

        return blobContainerClientMock;
    }

    public static Mock<BlobServiceClient> GetBlobServiceClientMock(BlobContainerClient blobContainerClient)
    {
        var blobServiceClientMock = new Mock<BlobServiceClient>();

        blobServiceClientMock
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClient);

        return blobServiceClientMock;
    }
}