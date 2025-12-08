namespace EPR.RegistrationValidation.Application.Validators
{
    using Constants;
    using Data.Enums;
    using Data.Models;
    using EPR.RegistrationValidation.Data.Constants;
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
                var errorCode =
                    (registrationJourney == RegistrationJourney.CsoLargeProducer.ToString() ||
                     registrationJourney == RegistrationJourney.DirectLargeProducer.ToString())
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
            if ((registrationJourney.Equals(RegistrationJourney.CsoLargeProducer.ToString()) ||
                 registrationJourney.Equals(RegistrationJourney.DirectLargeProducer.ToString()))
                && size.ToUpper() == OrganisationSizeCodes.L.ToString())
            {
                return true;
            }
            else if ((registrationJourney.Equals(RegistrationJourney.CsoSmallProducer.ToString()) ||
                      registrationJourney.Equals(RegistrationJourney.DirectSmallProducer.ToString()))
                     && size.ToUpper() == OrganisationSizeCodes.S.ToString())
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
