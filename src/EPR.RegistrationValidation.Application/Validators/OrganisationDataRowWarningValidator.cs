namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;
using Warnings = EPR.RegistrationValidation.Application.Validators.WarningValidators;

[ExcludeFromCodeCoverage]
public class OrganisationDataRowWarningValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationDataRowWarningValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        Include(new Warnings.TurnoverValueValidator());
    }
}