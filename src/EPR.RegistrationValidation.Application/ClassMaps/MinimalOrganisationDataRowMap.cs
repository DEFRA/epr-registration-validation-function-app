namespace EPR.RegistrationValidation.Application.ClassMaps;

using CsvHelper.Configuration;
using Data.Models;

public sealed class MinimalOrganisationDataRowMap : ClassMap<OrganisationDataRow>
{
    public MinimalOrganisationDataRowMap()
    {
        Map(x => x.LineNumber).Convert(args => args.Row.Parser.Row);
        Map(x => x.DefraId).Name("organisation_id");
        Map(x => x.SubsidiaryId).Name("subsidiary_id");
        Map(x => x.OrganisationTypeCode).Name("organisation_type_code");
        Map(x => x.PackagingActivitySO).Name("packaging_activity_so");
    }
}