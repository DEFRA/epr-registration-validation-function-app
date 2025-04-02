namespace EPR.RegistrationValidation.UnitTests.Validators.WarningValidators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators.WarningValidators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TurnOverValueValidatorTests : TurnoverValueValidator
{
    [TestMethod]
    [DataRow("0", "0")]
    [DataRow("0.0", "0")]
    [DataRow("0", "0.00")]
    [DataRow("0.00", "0.00")]
    [DataRow("0", "1")]
    [DataRow("0", "10.50")]
    [DataRow("0.0", "13.5")]
    [DataRow("0.0", "10.50")]
    public async Task TurnoverValueValidator_Throws_Valid_WarningErrorCode_WhenMet_AllTheOtherConditions(string turnoverValue, string totalTonnage)
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow
        {
            PackagingActivitySO = PackagingActivities.No,
            PackagingActivityPf = PackagingActivities.No,
            PackagingActivityIm = PackagingActivities.No,
            PackagingActivitySe = PackagingActivities.No,
            PackagingActivityHl = PackagingActivities.No,
            PackagingActivityOm = PackagingActivities.No,
            PackagingActivitySl = PackagingActivities.No,
            ProduceBlankPackagingFlag = YesNoOption.No,
            LiableForDisposalCostsFlag = YesNoOption.Yes,
            MeetReportingRequirementsFlag = YesNoOption.Yes,
            Turnover = turnoverValue,
            TotalTonnage = totalTonnage,
        };
        var validator = new TurnoverValueValidator();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.WarningZeroTurnover);
    }

    [TestMethod]
    [DataRow("0", "0")]
    [DataRow("0", "10")]
    [DataRow("0.0", "13.5")]
    public async Task TurnoverValueValidator_HasNo_WarningErrorCode_When_HasNot_MetZeroReturnYesNoCondition(string turnoverValue, string totalTonnage)
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow
        {
            PackagingActivitySO = PackagingActivities.No,
            PackagingActivityPf = PackagingActivities.No,
            PackagingActivityIm = PackagingActivities.Primary,
            PackagingActivitySe = PackagingActivities.No,
            PackagingActivityHl = PackagingActivities.No,
            PackagingActivityOm = PackagingActivities.No,
            PackagingActivitySl = PackagingActivities.No,
            ProduceBlankPackagingFlag = YesNoOption.Yes,
            LiableForDisposalCostsFlag = YesNoOption.Yes,
            MeetReportingRequirementsFlag = YesNoOption.No,
            Turnover = turnoverValue,
            TotalTonnage = totalTonnage,
        };
        var validator = new TurnoverValueValidator();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("", "15.5")]
    [DataRow(null, "25")]
    [DataRow("1", "0")]
    [DataRow("1", "0.0")]
    [DataRow("0", "-1")]
    [DataRow("0", "xxx")]
    [DataRow("0", " ")]
    [DataRow("0", null)]
    public async Task TurnoverValueValidator_HasNo_WarningErrorCode_When_NotMetConditionFor_RequiredTurnoverOrTonnagevalue(string turnoverValue, string totalTonnage)
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow
        {
            PackagingActivitySO = PackagingActivities.No,
            PackagingActivityPf = PackagingActivities.No,
            PackagingActivityIm = PackagingActivities.No,
            PackagingActivitySe = PackagingActivities.No,
            PackagingActivityHl = PackagingActivities.No,
            PackagingActivityOm = PackagingActivities.No,
            PackagingActivitySl = PackagingActivities.No,
            ProduceBlankPackagingFlag = YesNoOption.No,
            LiableForDisposalCostsFlag = YesNoOption.Yes,
            MeetReportingRequirementsFlag = YesNoOption.Yes,
            Turnover = turnoverValue,
            TotalTonnage = totalTonnage,
        };
        var validator = new TurnoverValueValidator();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
