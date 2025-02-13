namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

[ExcludeFromCodeCoverage]
public sealed class OrganisationDataRowWithoutOrgSizeColumnMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutOrgSizeColumnMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.OrganisationSize).Ignore();
    }
}