namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

[ExcludeFromCodeCoverage]
public sealed class OrganisationDataRowWithoutOrgSizeAndClosedLoopColumnsMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutOrgSizeAndClosedLoopColumnsMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.OrganisationSize).Ignore();
        Map(x => x.ClosedLoopRegistration).Ignore();
    }
}
