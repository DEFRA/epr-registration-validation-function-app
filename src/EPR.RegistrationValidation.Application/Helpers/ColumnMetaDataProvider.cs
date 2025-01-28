namespace EPR.RegistrationValidation.Application.Helpers;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CsvHelper.Configuration.Attributes;
using EPR.RegistrationValidation.Data.Attributes;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using Microsoft.FeatureManagement;

public class ColumnMetaDataProvider
{
    private const string OrganisationSizeColumnName = "OrganisationSize";
    private static IFeatureManager _featureManager;

    private readonly IDictionary<string, ColumnMetaData> _organisationMetaData;
    private readonly IDictionary<string, ColumnMetaData> _brandMetaData;
    private readonly IDictionary<string, ColumnMetaData> _partnerMetaData;

    public ColumnMetaDataProvider(IFeatureManager featureManager)
    {
        _featureManager = featureManager;

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

    [ExcludeFromCodeCoverage]
    private static Dictionary<string, ColumnMetaData> LoadColumnMetaData<T>()
    {
        var columnValues = typeof(T).GetProperties()
            .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
            .Select(x => (
                Key: x.Name,
                Index: x.GetCustomAttribute<ColumnAttribute>().Index,
                Name: x.GetCustomAttribute<NameAttribute>()?.Names.Single()))
            .ToList();

        var returnDictionary = columnValues.Count > 0 ? columnValues.ToDictionary(x => x.Key, x => new ColumnMetaData(x.Name, x.Index)) : new Dictionary<string, ColumnMetaData>();

        if (typeof(T).Equals(typeof(OrganisationDataRow)) && _featureManager != null && !_featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result)
        {
            returnDictionary.Remove(OrganisationSizeColumnName);
        }

        return returnDictionary;
    }
}