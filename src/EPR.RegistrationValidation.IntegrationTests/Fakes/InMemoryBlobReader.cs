namespace EPR.RegistrationValidation.IntegrationTests.Fakes;

using EPR.RegistrationValidation.Application.Readers;

public class InMemoryBlobReader : IBlobReader
{
    private readonly Dictionary<string, string> _blobContent = new(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdateBlob(string blobName, string csvContent)
    {
        _blobContent[blobName] = csvContent;
    }

    public MemoryStream DownloadBlobToStream(string name)
    {
        if (!_blobContent.TryGetValue(name, out var content))
        {
            throw new KeyNotFoundException($"Blob '{name}' was not configured for the integration test.");
        }

        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }
}
