namespace EPR.RegistrationValidation.Application.Validators;

using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using FluentValidation;

public class BasicPackagingActivityValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _activities =
    {
        PackagingActivities.Primary,
        PackagingActivities.Secondary,
        PackagingActivities.No,
    };

    public BasicPackagingActivityValidator()
    {
       RuleFor(x => x.PackagingActivitySO)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivitySo)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivitySe)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivitySe)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivityHl)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivityHl)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivityIm)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivityIm)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivityOm)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivityOm)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivityPf)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivityPf)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);

       RuleFor(x => x.PackagingActivitySl)
           .NotEmpty().WithErrorCode(ErrorCodes.MissingPackagingActivitySl)
           .Must(act => _activities.Contains(act, StringComparer.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCodes.InvalidPackagingActivity);
    }
}