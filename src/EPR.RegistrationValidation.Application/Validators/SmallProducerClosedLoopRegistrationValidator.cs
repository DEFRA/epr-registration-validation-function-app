namespace EPR.RegistrationValidation.Application.Validators
{
    using System.Diagnostics.CodeAnalysis;
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Constants;
    using EPR.RegistrationValidation.Data.Models;
    using FluentValidation;

    [ExcludeFromCodeCoverage]
    public class SmallProducerClosedLoopRegistrationValidator : AbstractValidator<OrganisationDataRow>
    {
        public SmallProducerClosedLoopRegistrationValidator()
        {
            RuleFor(row => row)
                .Cascade(CascadeMode.Stop)
                .Must(row => !IsSmallProducerWithClosedLoopYes(row.OrganisationSize, row.ClosedLoopRegistration))
                .WithErrorCode(ErrorCodes.ClosedLoopRegistrationNotAllowedForSmallProducer)
                .WithName(nameof(OrganisationDataRow.ClosedLoopRegistration))
                .When(row => !string.IsNullOrWhiteSpace(row.OrganisationSize)
                          && !string.IsNullOrWhiteSpace(row.ClosedLoopRegistration));
        }

        private static bool IsSmallProducerWithClosedLoopYes(string size, string closedLoop)
        {
            return OrganisationSizeCodes.S.ToString().Equals(size, StringComparison.CurrentCultureIgnoreCase)
                && YesNoOption.Yes.Equals(closedLoop, StringComparison.OrdinalIgnoreCase);
        }
    }
}
