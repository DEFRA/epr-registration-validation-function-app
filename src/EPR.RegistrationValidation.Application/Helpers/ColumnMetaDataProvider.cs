namespace EPR.RegistrationValidation.Application.Helpers;

using System.Reflection;
using CsvHelper.Configuration.Attributes;
using EPR.RegistrationValidation.Data.Attributes;
using EPR.RegistrationValidation.Data.Models;

public class ColumnMetaDataProvider
{
    private readonly IDictionary<string, ColumnMetaData> _organisationMetaData;
    private readonly IDictionary<string, ColumnMetaData> _brandMetaData;
    private readonly IDictionary<string, ColumnMetaData> _partnerMetaData;

    public ColumnMetaDataProvider()
    {
        _organisationMetaData = LoadColumnMetaData<OrganisationDataRow>();
        _brandMetaData = LoadColumnMetaData<BrandDataRow>();
        _partnerMetaData = LoadColumnMetaData<PartnersDataRow>();
    }

    public IDictionary<string, ColumnMetaData> ListColumnMetaData<T>()
    {
        return typeof(T).Name switch
        {
            nameof(OrganisationDataRow) => _organisationMetaData,
            nameof(BrandDataRow) => _brandMetaData,
            nameof(PartnersDataRow) => _partnerMetaData,
            _ => new Dictionary<string, ColumnMetaData>(),
        };
    }

    public ColumnMetaData GetOrganisationColumnMetaData(string propertyName)
    {
        return _organisationMetaData[propertyName];
    }

    private static IDictionary<string, ColumnMetaData> LoadColumnMetaData<T>()
    {
        var columnValues = typeof(T).GetProperties()
            .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
            .Select(x => (
                Key: x.Name,
                Index: x.GetCustomAttribute<ColumnAttribute>().Index,
                Name: x.GetCustomAttribute<NameAttribute>()?.Names.Single()))
            .ToList();

        if (columnValues.Any())
        {
            return columnValues.ToDictionary(x => x.Key, x => new ColumnMetaData(x.Name, x.Index));
        }

        return new Dictionary<string, ColumnMetaData>();
    }
}