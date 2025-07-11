﻿namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class CompanyHouseValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _codes =
    {
        UnIncorporationTypeCodes.CoOperative,
        UnIncorporationTypeCodes.SoleTrader,
        UnIncorporationTypeCodes.Partnership,
        UnIncorporationTypeCodes.Others,
        UnIncorporationTypeCodes.OutsideUk,
    };

    public CompanyHouseValidator()
    {
        RuleFor(row => row.CompaniesHouseNumber)
            .Must(ValidNumber)
            .When(row => !string.IsNullOrEmpty(row.CompaniesHouseNumber))
            .WithErrorCode(ErrorCodes.InvalidCompanyHouseNumber);
    }

    private static bool ValidNumber(string companiesHouseNumber)
    {
        return companiesHouseNumber != "00000000";
    }
}