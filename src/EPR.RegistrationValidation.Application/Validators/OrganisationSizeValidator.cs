namespace EPR.RegistrationValidation.Application.Validators
{
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Constants;
    using EPR.RegistrationValidation.Data.Models;
    using FluentValidation;

    public class OrganisationSizeValidator : AbstractValidator<OrganisationDataRow>
    {
        public OrganisationSizeValidator(string? registrationJourney)
        {
            RuleFor(size => size.OrganisationSize)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationSizeValue)
                .Must(BeValidValue).WithErrorCode(ErrorCodes.InvalidOrganisationSizeValue);

            if (registrationJourney != null)
            {
                var errorCode = registrationJourney.ToLowerInvariant().Contains("large")
                    ? ErrorCodes.SmallProducerInLargeProducerFile
                    : ErrorCodes.LargeProducerInSmallProducerFile;

                RuleFor(size => size.OrganisationSize)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationSizeValue)
                    .Must(size => BeCorrectSizeForJourneyChosen(size, registrationJourney))
                    .WithErrorCode(errorCode);
            }
        }

        private static bool BeCorrectSizeForJourneyChosen(string size, string registrationJourney)
        {
            if (registrationJourney.ToLowerInvariant().Contains("large") && size.ToUpper() == OrganisationSizeCodes.L.ToString())
            {
                return true;
            }
            else if (registrationJourney.ToLowerInvariant().Contains("small") && size.ToUpper() == OrganisationSizeCodes.S.ToString())
            {
                return true;
            }

            return false;
        }

        private static bool BeValidValue(string size)
        {
            return !string.IsNullOrEmpty(size)
                && (Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToUpper()) || Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToLower()));
        }
    }
}
