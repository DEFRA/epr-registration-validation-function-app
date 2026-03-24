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

            var orgSizeEnabled = _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result;
            var leaverJoinerEnabled = _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result;

            await csv.ReadAsync();
            csv.ReadHeader();

            var header = csv.HeaderRecord;

            var orgSizeOff = !orgSizeEnabled;
            var leaverJoinerOff = !leaverJoinerEnabled;
            var closedLoopOff = !(header?.Contains("closed_loop_registration") ?? false);

            if (!useMinimalClassMaps)
            {
                if (orgSizeOff && leaverJoinerOff)
                {
                    // Also ignores ClosedLoopRegistration — leaver/joiner off implies ClosedLoop absent too
                    csv.Context.RegisterClassMap<OrganisationDataRowWithoutOrgSizeLeaverAndJoinerColumnsMap>();
                }
                else if (orgSizeOff && closedLoopOff)
                {
                    csv.Context.RegisterClassMap<OrganisationDataRowWithoutOrgSizeAndClosedLoopColumnsMap>();
                }
                else if (orgSizeOff)
                {
                    csv.Context.RegisterClassMap<OrganisationDataRowWithoutOrgSizeColumnMap>();
                }
                else if (leaverJoinerOff)
                {
                    // Also ignores ClosedLoopRegistration — leaver/joiner off implies ClosedLoop absent too
                    csv.Context.RegisterClassMap<OrganisationDataRowWithoutLeaverAndJoinerColumnsMap>();
                }
                else if (closedLoopOff)
                {
                    csv.Context.RegisterClassMap<OrganisationDataRowWithoutClosedLoopColumnMap>();
                }
            }
            else
            {
                // Register class map to populate minimal set of properties to keep memory usage to a minimum
                csv.Context.RegisterClassMap<MinimalOrganisationDataRowMap>();
            }

            var columnAttributes = _metaDataProvider.ListColumnMetaData<T>();
            var orderedHeaders = columnAttributes
                .Select(x => x.Value)
                .OrderBy(x => x.Index)
                .ToList();

            if (orgSizeOff)
            {
                var toBeRemoved = orderedHeaders.SingleOrDefault(x => x.Name == "organisation_size");
                if (toBeRemoved != null)
                {
                    orderedHeaders.Remove(toBeRemoved);
                }
            }

            if (leaverJoinerOff)
            {
                var leaverAndJoinerColumns = new HashSet<string>
                {
                    _featureManager.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn).Result ? "status_code" : "leaver_code",
                    "organisation_change_reason",
                    "leaver_date",
                    "joiner_date",
                };

                orderedHeaders.RemoveAll(h => leaverAndJoinerColumns.Contains(h.Name));
            }

            if (closedLoopOff)
            {
                orderedHeaders.RemoveAll(h => h.Name == "closed_loop_registration");
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