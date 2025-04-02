namespace EPR.RegistrationValidation.Application.Services.HelperFunctions
{
    using System.Globalization;
    using EPR.RegistrationValidation.Application.Constants;
    using EPR.RegistrationValidation.Data.Models;

    public static class HelperFunctions
    {
        public static bool HasMetZeroReturnYesNoCondition(OrganisationDataRow row)
        {
            var flagsWithYesOption = new[]
            {
                row.LiableForDisposalCostsFlag,
                row.MeetReportingRequirementsFlag,
            };

            var hasYesFlags = Array.TrueForAll(flagsWithYesOption, flag => !string.IsNullOrWhiteSpace(flag) &&
                                               flag.Equals(YesNoOption.Yes, StringComparison.OrdinalIgnoreCase));

            var hasNoFlag = !string.IsNullOrWhiteSpace(row.ProduceBlankPackagingFlag) &&
                             row.ProduceBlankPackagingFlag.Equals(YesNoOption.No, StringComparison.OrdinalIgnoreCase);

            if (hasYesFlags && hasNoFlag && HasMetZeroReturnNoPackagingActivity(row))
            {
                return true;
            }

            return false;
        }

        public static bool HasMetZeroReturnNoPackagingActivity(OrganisationDataRow row)
        {
            var packagingActivities = new[]
            {
                row.PackagingActivitySO,
                row.PackagingActivityPf,
                row.PackagingActivityIm,
                row.PackagingActivitySe,
                row.PackagingActivityHl,
                row.PackagingActivityOm,
                row.PackagingActivitySl,
            };

            return Array.TrueForAll(packagingActivities, activity => !string.IsNullOrWhiteSpace(activity) &&
                                    activity.Equals(PackagingActivities.No, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsGreaterThanOrEqualToZero(string number)
        {
            return !string.IsNullOrWhiteSpace(number)
                   && decimal.TryParse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal decimalValue)
                   && decimalValue >= 0;
        }
    }
}
