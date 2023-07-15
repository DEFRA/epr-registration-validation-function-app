namespace EPR.RegistrationValidation.UnitTests.Readers;

using Application.Readers;
using Azure.Storage.Blobs;
using Data.Config;
using FluentAssertions;
using Helpers;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class BlobReaderTests
{
    private BlobReader _sut;
    private Mock<BlobClient> _blobClientMock;
    private Mock<BlobContainerClient> _blobContainerClientMock;
    private Mock<BlobServiceClient> _blobServiceClientMock;

    [TestInitialize]
    public void SetUp()
    {
        IOptions<StorageAccountConfig> options = Options.Create<StorageAccountConfig>(new StorageAccountConfig
        {
            ConnectionString = "A",
            BlobContainerName = "B",
        });
        _blobClientMock = BlobStorageServiceTestsHelper.GetBlobClientMock();
        _blobContainerClientMock =
            BlobStorageServiceTestsHelper.GetBlobContainerClientMock(_blobClientMock.Object);
        _blobServiceClientMock =
            BlobStorageServiceTestsHelper.GetBlobServiceClientMock(_blobContainerClientMock.Object);

        _sut = new BlobReader(_blobServiceClientMock.Object, options);
    }

    [TestMethod]
    public async Task TestDownloadBlobToStream_WhenDownloadToAssignsAMemoryStream_ReturnsMemoryStream()
    {
        // ARRANGE
        var fileString = "Test";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileString);
        await writer.FlushAsync();

        // ACT
        var memoryStream = _sut.DownloadBlobToStream("test");

        // ASSERT
        StreamReader reader = new StreamReader(memoryStream);
        string text = await reader.ReadToEndAsync();
        text.Should().BeEmpty();
    }

    [TestMethod]
    public async Task TestGetMessageFromJson_WhenMessageIsInvalid_ThrowsError()
    {
        // ARRANGE
        var fileString = "Test";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileString);
        await writer.FlushAsync();
        _blobServiceClientMock.Setup(m => m.GetBlobContainerClient(It.IsAny<string>()))
            .Throws(new Exception());

        // ACT
        Action act = () => _sut.DownloadBlobToStream("test");

        // ASSERT
        act.Should().Throw<Exception>();
    }
}