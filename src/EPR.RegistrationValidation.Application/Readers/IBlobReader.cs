namespace EPR.RegistrationValidation.Application.Readers;

public interface IBlobReader
{
    MemoryStream DownloadBlobToStream(string name);
}