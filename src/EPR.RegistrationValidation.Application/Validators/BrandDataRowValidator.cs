namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class BrandDataRowValidator : AbstractValidator<BrandDataRow>
{
    public BrandDataRowValidator()
    {
        Include(new BrandDataRowCharacterLengthValidator());
    }
}