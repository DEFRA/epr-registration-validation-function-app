namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using FluentValidation;

public class BrandDataRowOrganisationDataValidator : AbstractValidator<BrandDataRow>
{
    public BrandDataRowOrganisationDataValidator()
    {
        RuleFor(x => x.DefraId)
            .Must((_, val, context) =>
            {
                var lookup = GetOrganisationDataLookupTableData(context);
                if (lookup.Count > 0 && !lookup.ContainsKey(val))
                {
                    return false;
                }

                return true;
            })
            .WithErrorCode(ErrorCodes.BrandDetailsNotMatchingOrganisation);

        RuleFor(x => x.SubsidiaryId)
            .Must((x, val, context) =>
            {
                var lookup = GetOrganisationDataLookupTableData(context);
                if (!string.IsNullOrEmpty(val) &&
                    lookup.TryGetValue(x.DefraId, out var brandSubsidiaryLookup) &&
                    !brandSubsidiaryLookup.ContainsKey(val))
                {
                    return false;
                }

                return true;
            })
            .WithErrorCode(ErrorCodes.BrandDetailsNotMatchingSubsidiary);
    }

    private Dictionary<string, Dictionary<string, OrganisationIdentifiers>> GetOrganisationDataLookupTableData(FluentValidation.ValidationContext<BrandDataRow> context)
    {
        if (context.RootContextData.TryGetValue(nameof(OrganisationDataLookupTable), out var lookup)
            && lookup is OrganisationDataLookupTable table)
        {
            return table.Data;
        }

        return new();
    }
}