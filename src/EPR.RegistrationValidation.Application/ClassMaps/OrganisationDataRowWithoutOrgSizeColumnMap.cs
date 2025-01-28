namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

public sealed class OrganisationDataRowWithoutOrgSizeColumnMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutOrgSizeColumnMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.OrganisationSize).Ignore();
    }
}