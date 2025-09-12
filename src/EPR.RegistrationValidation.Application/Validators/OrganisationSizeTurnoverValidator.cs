namespace EPR.RegistrationValidation.Application.Validators
{
    using System.Diagnostics.CodeAnalysis;
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Constants;
    using EPR.RegistrationValidation.Data.Models;
    using FluentValidation;

    [ExcludeFromCodeCoverage]
    public class OrganisationSizeTurnoverValidator : AbstractValidator<OrganisationDataRow>
    {
        public OrganisationSizeTurnoverValidator()
        {
            RuleFor(size => size)
                .Cascade(CascadeMode.Stop)
                .Must(size => BeSmallProducerSizeTurnoverValidValue(size.OrganisationSize, size.Turnover, size.TotalTonnage)).WithErrorCode(ErrorCodes.SmallProducerTurnoverInvalid).WithName(nameof(OrganisationDataRow.OrganisationSize))
                .When(size => !string.IsNullOrWhiteSpace(size.OrganisationSize) && !string.IsNullOrEmpty(size.Turnover) && !string.IsNullOrEmpty(size.TotalTonnage));
        }

        private static bool BeSmallProducerSizeTurnoverValidValue(string size, string turnover, string totalTonnage)
        {
            var turnoverValue = decimal.Parse(turnover);
            var totalTonnageValue = decimal.Parse(totalTonnage);

            if (size.Equals(OrganisationSizeCodes.S.ToString(), StringComparison.CurrentCultureIgnoreCase) && (turnoverValue > 2 && totalTonnageValue > 50))
            {
                return false;
            }

            return true;
        }
    }
}
