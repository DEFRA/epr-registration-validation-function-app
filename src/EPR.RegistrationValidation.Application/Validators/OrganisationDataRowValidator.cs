namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;
using Microsoft.FeatureManagement;

[ExcludeFromCodeCoverage]
public class OrganisationDataRowValidator : AbstractValidator<OrganisationDataRow>
{
    private readonly IFeatureManager _featureManager;
    private bool _validatorsRegistred;

    public OrganisationDataRowValidator(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
        _validatorsRegistred = false;
    }

    public void RegisterValidators(bool uploadedByComplianceScheme)
    {
        if (_validatorsRegistred)
        {
            return;
        }

        Include(new OrganisationIdValidator());
        Include(new OrganisationNameValidator());
        Include(new OrganisationTradingNameValidator());
        Include(new HomeNationCodeValidator());
        Include(new OrganisationMainActivitySicValidator());
        Include(new PrimaryContactValidator());
        Include(new RegisteredAddressValidator());
        Include(new AuditAddressValidator());
        Include(new PrincipalAddressValidator());
        Include(new PackagingActivityValidator());
        Include(new ProduceBlankPackagingValidator());
        Include(new TurnoverValueValidator());
        Include(new ServiceOfNoticeAddressValidator());
        Include(new TotalTonnageValidator());
        Include(new CompanyHouseValidator());
        Include(new OrganisationTypeValidator());

        if (_featureManager != null && _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns).Result)
        {
            Include(new LeaverCodeValidator(uploadedByComplianceScheme));
            Include(new JoinerDateValidator());
            Include(new LeaverDateValidator(uploadedByComplianceScheme));
            Include(new OrganisationChangeReasonValidator());
            Include(new RegistrationTypeCodeValidator(uploadedByComplianceScheme));
        }

        if (_featureManager != null && _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation).Result)
        {
            Include(new OrganisationSizeValidator());
            Include(new OrganisationSizeTurnoverValidator());
        }

        _validatorsRegistred = true;
    }
}
