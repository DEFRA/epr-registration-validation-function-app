namespace EPR.RegistrationValidation.Application.ClassMaps;

using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Data.Models;

public sealed class CsvDataRowMap : ClassMap<CsvDataRow>
{
    public CsvDataRowMap()
    {
        Map(x => x.DefraId).Index(0).TypeConverter<StringConverter>();
        Map(x => x.SubsidiaryId).Index(1).TypeConverter<StringConverter>();
        Map(x => x.OrganisationTypeCode).Index(7).TypeConverter<StringConverter>();
        Map(x => x.PackagingActivitySO).Index(9).TypeConverter<StringConverter>();
    }
}