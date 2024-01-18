namespace EPR.RegistrationValidation.Data.Attributes;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LineNumberAttribute : Attribute, IMemberMapper
{
    public void ApplyTo(MemberMap memberMap)
    {
        memberMap.Data.ReadingConvertExpression = (ConvertFromStringArgs args) => args.Row.Parser.Row;
    }
}