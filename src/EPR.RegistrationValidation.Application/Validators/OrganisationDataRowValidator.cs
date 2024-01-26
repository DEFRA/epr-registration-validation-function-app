namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class OrganisationDataRowValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationDataRowValidator()
    {
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
    }
}
