namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using System.Reflection;
using System.Text;

public static class CsvFileReader
{
    public static MemoryStream ReadFile(string fileName)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SampleFiles", fileName);
        using var fileStream = File.OpenRead(path);
        var memStream = new MemoryStream();

        memStream.SetLength(fileStream.Length);

        fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
        return memStream;
    }

    public static MemoryStream ReadString(string csvString)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(csvString));
    }
}