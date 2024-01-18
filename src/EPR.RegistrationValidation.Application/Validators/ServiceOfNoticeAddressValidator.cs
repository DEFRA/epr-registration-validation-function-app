namespace EPR.RegistrationValidation.Application.Validators;

using System.Text.RegularExpressions;
using Constants;
using Data.Constants;
using Data.Models;
using FluentValidation;

public class ServiceOfNoticeAddressValidator : AbstractValidator<OrganisationDataRow>
{
    public ServiceOfNoticeAddressValidator()
    {
        RuleFor(x => x.ServiceOfNoticeAddressLine1)
            .NotEmpty()
            .When(OtherAddressFieldExists)
            .WithErrorCode(ErrorCodes.MissingServiceOfNoticeAddressLine1);
        RuleFor(x => x.ServiceOfNoticeAddressPostcode)
            .NotEmpty()
            .When(OtherAddressFieldExists)
            .WithErrorCode(ErrorCodes.MissingServiceOfNoticePostcode);
        RuleFor(x => x.ServiceOfNoticeAddressPhoneNumber)
            .NotEmpty()
            .When(OtherAddressFieldExists)
            .WithErrorCode(ErrorCodes.MissingServiceOfNoticePhoneNumber);
    }

    private static bool OtherAddressFieldExists(OrganisationDataRow row)
    {
        return !string.IsNullOrEmpty(row.ServiceOfNoticeAddressLine1) ||
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressLine2) ||
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressCity) ||
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressCounty) ||
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressCountry) |
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressPostcode) ||
               !string.IsNullOrEmpty(row.ServiceOfNoticeAddressPhoneNumber);
    }
}