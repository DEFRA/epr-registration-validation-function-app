namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

[ExcludeFromCodeCoverage]
public sealed class OrganisationDataRowWithoutOrgSizeLeaverAndJoinerColumnsMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutOrgSizeLeaverAndJoinerColumnsMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.LeaverCode).Ignore();
        Map(x => x.OrganisationChangeReason).Ignore();
        Map(x => x.LeaverDate).Ignore();
        Map(x => x.JoinerDate).Ignore();
        Map(x => x.OrganisationSize).Ignore();
    }
}
