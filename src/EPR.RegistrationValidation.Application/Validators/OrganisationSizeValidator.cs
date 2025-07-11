namespace EPR.RegistrationValidation.Application.Validators
{
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Constants;
    using EPR.RegistrationValidation.Data.Models;
    using FluentValidation;

    public class OrganisationSizeValidator : AbstractValidator<OrganisationDataRow>
    {
        public OrganisationSizeValidator(bool uploadedByComplianceScheme, bool isSubmissionPeriod2026, DateTime smallProducersRegStartTime2026, DateTime smallProducersRegEndTime2026)
        {
            RuleFor(size => size.OrganisationSize)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationSizeValue)
                .Must(BeValidValue).WithErrorCode(ErrorCodes.InvalidOrganisationSizeValue);

            if (isSubmissionPeriod2026)
            {
                var errorCode = uploadedByComplianceScheme
                    ? ErrorCodes.InvalidRegistrationWindowForSmallProducers2026CS
                    : ErrorCodes.InvalidRegistrationWindowForSmallProducers2026DP;

                RuleFor(size => size.OrganisationSize)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationSizeValue)
                    .Must(size =>
                        BeTimeToRegisterAsSmallProducerFor2026(size, smallProducersRegStartTime2026, smallProducersRegEndTime2026))
                    .WithErrorCode(errorCode);
            }
        }

        private static bool BeValidValue(string size)
        {
            return !string.IsNullOrEmpty(size)
                && (Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToUpper()) || Enum.IsDefined(typeof(OrganisationSizeCodes), size.ToLower()));
        }

        private static bool BeTimeToRegisterAsSmallProducerFor2026(string size, DateTime start, DateTime end)
        {
            bool isSmallProducer = string.Equals(size.ToUpper(), OrganisationSizeCodes.S.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isDateInRange = DateTime.UtcNow >= start && DateTime.UtcNow <= end;

            if (isSmallProducer)
            {
                return isDateInRange;
            }
            else
            {
                return true;
            }
        }
    }
}
