namespace EPR.RegistrationValidation.Application.Helpers;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.RegistrationValidation.Application.ClassMaps;
using EPR.RegistrationValidation.Data.Constants;
using Exceptions;
using Microsoft.FeatureManagement;

public class CsvStreamParser : ICsvStreamParser
{
    private readonly ColumnMetaDataProvider _metaDataProvider;
    private readonly IFeatureManager _featureManager;

    public CsvStreamParser(ColumnMetaDataProvider metaDataProvider)
    {
        _metaDataProvider = metaDataProvider;
    }

    public CsvStreamParser(ColumnMetaDataProvider metaDataProvider, IFeatureManager featureManager)
    {
        _metaDataProvider = metaDataProvider;
        _featureManager = featureManager;
    }

    private static CsvConfiguration CsvConfiguration => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
    };

    public async Task<List<T>> GetItemsFromCsvStreamAsync<T>(MemoryStream memoryStream, bool useMinimalClassMaps = false)
    {
        try
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            using var csv = new CsvReader(reader, CsvConfiguration);

            if (_featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result == false &&
                _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result == false)
            {
                csv.Context.RegisterClassMap<OrganisationDataRowWithoutOrgSizeLeaverAndJoinerColumnsMap>();
            }
            else if (_featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result == false)
            {
                // Register class map to populate data row without the (newer) organisation size property
                csv.Context.RegisterClassMap<OrganisationDataRowWithoutOrgSizeColumnMap>();
            }
            else if (_featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result == false)
            {
                // Register class map to populate data row without the (newer) organisation size property
                csv.Context.RegisterClassMap<OrganisationDataRowWithoutLeaverAndJoinerColumnsMap>();
            }

            if (useMinimalClassMaps)
            {
                // Register class map to populate minimal set of properties to keep memory usage to a minimum
                csv.Context.RegisterClassMap<MinimalOrganisationDataRowMap>();
            }

            csv.Read();
            csv.ReadHeader();

            var header = csv.HeaderRecord;
            var columnAttributes = _metaDataProvider.ListColumnMetaData<T>();
            var orderedHeaders = columnAttributes
                .Select(x => x.Value)
                .OrderBy(x => x.Index)
                .ToList();

            if (_featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result == false)
            {
                var toBeRemoved = orderedHeaders.SingleOrDefault(x => x.Name == "organisation_size");
                if (toBeRemoved != null)
                {
                    orderedHeaders.Remove(toBeRemoved);
                }
            }

            if (!_featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result)
            {
                var leaverAndJoinerColumns = new HashSet<string>
                {
                    "leaver_code",
                    "leaver_reason",
                    "leaver_date",
                    "reporting_type",
                    "joiner_date",
                };

                orderedHeaders.RemoveAll(header => leaverAndJoinerColumns.Contains(header.Name));
            }

            if (!header.SequenceEqual(orderedHeaders.Select(x => x.Name)))
            {
                throw new CsvHeaderException("The CSV file header is invalid.");
            }

            var list = csv.GetRecords<T>().ToList();
            return list;
        }
        catch (CsvHeaderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CsvParseException("Error parsing CSV", ex);
        }
    }
}