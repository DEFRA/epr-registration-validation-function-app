namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

[ExcludeFromCodeCoverage]
public sealed class OrganisationDataRowWithoutClosedLoopColumnMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutClosedLoopColumnMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.ClosedLoopRegistration).Ignore();
    }
}
