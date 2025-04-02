namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Refactoring related validators in one accessible class
/// by avoiding passing mulitple parameters to the validationService.
/// </summary>
[ExcludeFromCodeCoverage]
public class RowValidators
{
    public RowValidators(
        OrganisationDataRowValidator organisationDataRowValidator,
        OrganisationDataRowWarningValidator organisationDataRowWarningValidator,
        BrandDataRowValidator brandDataRowValidator,
        PartnerDataRowValidator partnerDataRowValidator)
    {
        OrganisationDataRowValidator = organisationDataRowValidator;
        OrganisationDataRowWarningValidator = organisationDataRowWarningValidator;
        BrandDataRowValidator = brandDataRowValidator;
        PartnerDataRowValidator = partnerDataRowValidator;
    }

    public OrganisationDataRowValidator OrganisationDataRowValidator { get; }

    public OrganisationDataRowWarningValidator OrganisationDataRowWarningValidator { get; }

    public BrandDataRowValidator BrandDataRowValidator { get; }

    public PartnerDataRowValidator PartnerDataRowValidator { get; }
}
