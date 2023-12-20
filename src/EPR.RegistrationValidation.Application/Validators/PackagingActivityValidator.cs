namespace EPR.RegistrationValidation.Application.Validators;

using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using FluentValidation;
using FluentValidation.Results;

public class PackagingActivityValidator : AbstractValidator<OrganisationDataRow>
{
    public PackagingActivityValidator()
    {
        Include(new BasicPackagingActivityValidator());

        When(row => PrimaryPackagingActivityCount(row) > 1, () =>
        {
            RuleFor(x => x.PackagingActivitySO)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivityHl)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivityIm)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivityOm)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivityPf)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivitySe)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
            RuleFor(x => x.PackagingActivitySl)
                .Must(IsNotPrimaryActivity)
                .WithErrorCode(ErrorCodes.MultiplePrimaryActivity);
        });

        When(row => PrimaryPackagingActivityCount(row) == 0, () =>
        {
            RuleFor(x => x.PackagingActivitySO)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivityHl)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivityIm)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivityOm)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivityPf)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivitySe)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
            RuleFor(x => x.PackagingActivitySl)
                .Must(IsPrimaryActivity)
                .WithErrorCode(ErrorCodes.MissingPrimaryActivity);
        });
    }

    private int PrimaryPackagingActivityCount(OrganisationDataRow row)
    {
        var packagingActivities = new[]
        {
            row.PackagingActivitySO,
            row.PackagingActivityHl,
            row.PackagingActivityIm,
            row.PackagingActivityOm,
            row.PackagingActivityPf,
            row.PackagingActivitySe,
            row.PackagingActivitySl,
        };
        return packagingActivities.Count(activity => activity?.Equals(PackagingActivities.Primary, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private bool IsPrimaryActivity(string? packagingActivity)
    {
        return packagingActivity?.Equals(PackagingActivities.Primary, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private bool IsNotPrimaryActivity(string? packagingActivity)
    {
        return !IsPrimaryActivity(packagingActivity);
    }
}