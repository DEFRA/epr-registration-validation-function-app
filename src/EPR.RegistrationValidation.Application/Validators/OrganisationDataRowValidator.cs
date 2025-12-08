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
    private bool _areValidatorsRegistered;

    public OrganisationDataRowValidator(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
        _areValidatorsRegistered = false;
    }

    public async Task RegisterValidators(
        bool uploadedByComplianceScheme,
        bool isSubmissionPeriod2026,
        DateTime smallProducersRegStartTime2026,
        DateTime smallProducersRegEndTime2026,
        string? registrationJourney)
    {
        if (_areValidatorsRegistered)
        {
            return;
        }

        var isSubsidiaryJoinerLeaverEnabled = _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns);
        var isOrganisationSizeFieldValidationEnabled = _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation);
        var enableAdditionalValidationForJoinerLeaverColumnsTask = _featureManager.IsEnabledAsync(FeatureFlags.EnableAdditionalValidationForJoinerLeaverColumns);
        var enableLeaverCodeValidationTask = _featureManager.IsEnabledAsync(FeatureFlags.EnableLeaverCodeValidation);

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

        if (await isSubsidiaryJoinerLeaverEnabled)
        {
            var enableAdditionalValidationForJoinerLeaverColumns = await enableAdditionalValidationForJoinerLeaverColumnsTask;
            var enableLeaverCodeValidation = await enableLeaverCodeValidationTask;
            Include(new JoinerDateValidator(uploadedByComplianceScheme, enableAdditionalValidationForJoinerLeaverColumns, enableLeaverCodeValidation));
            Include(new LeaverDateValidator(uploadedByComplianceScheme, enableLeaverCodeValidation));
            Include(new LeaverCodeValidator(uploadedByComplianceScheme, enableLeaverCodeValidation));
            Include(new OrganisationChangeReasonValidator());
            Include(new RegistrationTypeCodeValidator(uploadedByComplianceScheme));
        }

        if (await isOrganisationSizeFieldValidationEnabled)
        {
            Include(new OrganisationSizeValidator(registrationJourney));
            Include(new OrganisationSizeTurnoverValidator());
        }

        _areValidatorsRegistered = true;
    }
}
