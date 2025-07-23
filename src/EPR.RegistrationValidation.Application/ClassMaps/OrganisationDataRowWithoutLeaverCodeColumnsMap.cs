namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

public sealed class OrganisationDataRowWithoutLeaverCodeColumnsMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutLeaverCodeColumnsMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.LeaverCode).Ignore();
        Map(x => x.OrganisationChangeReason).Ignore();
        Map(x => x.LeaverDate).Ignore();
        Map(x => x.JoinerDate).Ignore();
    }
}
