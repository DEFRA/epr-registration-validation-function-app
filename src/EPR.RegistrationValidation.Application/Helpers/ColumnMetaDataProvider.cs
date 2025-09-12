namespace EPR.RegistrationValidation.Application.Helpers;

using System.Collections.Generic;
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
    private const string StatusCodeName = "StatusCode";
    private const string LeaverDateName = "LeaverDate";
    private const string OrganisationChangeReasonName = "OrganisationChangeReason";
    private const string ReportingTypeName = "ReportingType";
    private const string JoinerDateName = "JoinerDate";
    private readonly IFeatureManager _featureManager;

    private readonly Dictionary<string, ColumnMetaData> _organisationMetaData;
    private readonly Dictionary<string, ColumnMetaData> _brandMetaData;
    private readonly Dictionary<string, ColumnMetaData> _partnerMetaData;

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
    private Dictionary<string, ColumnMetaData> LoadColumnMetaData<T>()
    {
        var columnValues = typeof(T).GetProperties()
            .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
            .Select(x =>
            {
                var nameAttributes = x.GetCustomAttribute<NameAttribute>().Names;
                string name = nameAttributes[0];

                if (name == "status_code" && !_featureManager.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn).Result)
                {
                    name = nameAttributes[1];
                }

                return (
                    Key: x.Name,
                    Index: x.GetCustomAttribute<ColumnAttribute>().Index,
                    Name: name);
            })
            .ToList();

        var returnDictionary = columnValues.Count > 0
            ? columnValues.ToDictionary(x => x.Key, x => new ColumnMetaData(x.Name, x.Index))
            : new Dictionary<string, ColumnMetaData>();

        if (typeof(T).Equals(typeof(OrganisationDataRow)) &&
            !_featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result)
        {
            returnDictionary.Remove(OrganisationSizeColumnName);
        }

        if (typeof(T).Equals(typeof(OrganisationDataRow)) &&
            !_featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result)
        {
            returnDictionary.Remove(StatusCodeName);
            returnDictionary.Remove(LeaverDateName);
            returnDictionary.Remove(OrganisationChangeReasonName);
            returnDictionary.Remove(ReportingTypeName);
            returnDictionary.Remove(JoinerDateName);
        }

        return returnDictionary;
    }
}