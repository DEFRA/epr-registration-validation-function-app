namespace EPR.RegistrationValidation.Application.Validators;

using System.Text.RegularExpressions;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class OrganisationChangeReasonValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationChangeReasonValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.OrganisationChangeReason)
           .NotEmpty()
           .When(x => !string.IsNullOrEmpty(x.LeaverCode) && (x.LeaverCode == JoinerCode.LeaverCode20 || x.LeaverCode == LeaverCode.LeaverCode21))
           .WithErrorCode(ErrorCodes.OrganisationChangeReasonMustBePresent);

        RuleFor(r => r.OrganisationChangeReason)
            .Must(x => x.Length <= 200)
            .When(x => !string.IsNullOrEmpty(x.OrganisationChangeReason))
            .WithErrorCode(ErrorCodes.OrganisationChangeReasonCannotBeLongerThan200Characters);

        RuleFor(r => r.OrganisationChangeReason)
            .Must(x => Regex.IsMatch(x, "^[a-zA-Z0-9 ]*$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
            .When(x => !string.IsNullOrEmpty(x.OrganisationChangeReason))
            .WithErrorCode(ErrorCodes.OrganisationChangeReasonCannotIncludeSpecialCharacters);
    }
}
