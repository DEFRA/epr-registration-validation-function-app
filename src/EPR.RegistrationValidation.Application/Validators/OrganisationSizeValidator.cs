namespace EPR.RegistrationValidation.Application.Validators
{
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Constants;
    using EPR.RegistrationValidation.Data.Models;
    using FluentValidation;

    public class OrganisationSizeValidator : AbstractValidator<OrganisationDataRow>
    {
        public OrganisationSizeValidator()
        {
            RuleFor(size => size.OrganisationSize)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationSizeValue)
                .Must(BeValidValue).WithErrorCode(ErrorCodes.InvalidOrganisationSizeValue);
        }

        private static bool BeValidValue(string size)
        {
            return !string.IsNullOrEmpty(size)
                && (Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToUpper()) || Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToLower()));
        }
    }
}
