namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class ProduceBlankPackagingValidator : AbstractValidator<OrganisationDataRow>
{
  private static readonly string[] _YesNoOption =
    {
        YesNoOption.Yes,
        YesNoOption.No,
    };

  public ProduceBlankPackagingValidator()
  {
      RuleFor(x => x.ProduceBlankPackagingFlag)
          .IsInAllowedOptions(_YesNoOption).WithErrorCode(ErrorCodes.InvalidProduceBlankPackagingFlag);

      RuleFor(x => x.ProduceBlankPackagingFlag)
          .NotEmpty().WithErrorCode(ErrorCodes.InvalidProduceBlankPackagingFlag)
          .When(IsBrandOwner);
  }

  private static bool IsBrandOwner(OrganisationDataRow row)
  {
          return !string.IsNullOrWhiteSpace(row.PackagingActivitySO) &&
                 (row.PackagingActivitySO.Equals(PackagingActivities.Primary, StringComparison.OrdinalIgnoreCase) ||
                 row.PackagingActivitySO.Equals(PackagingActivities.Secondary, StringComparison.OrdinalIgnoreCase));
  }
}