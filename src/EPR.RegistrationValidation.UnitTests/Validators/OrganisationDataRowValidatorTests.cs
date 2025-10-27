namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class OrganisationDataRowValidatorTests
{
    private Mock<IFeatureManager> _featureManagerMock = null!;

    [TestInitialize]
    public void SetUp()
    {
        _featureManagerMock = new Mock<IFeatureManager>(MockBehavior.Strict);

        // Enable organisation size validation so OrganisationSizeTurnoverValidator is included.
        _featureManagerMock
            .Setup(m => m.IsEnabledAsync("EnableOrganisationSizeFieldValidation"))
            .ReturnsAsync(true);

        // Disable joiner/leaver related flags for this repro.
        _featureManagerMock
            .Setup(m => m.IsEnabledAsync("EnableSubsidiaryJoinerAndLeaverColumns"))
            .ReturnsAsync(false);
        _featureManagerMock
            .Setup(m => m.IsEnabledAsync("EnableAdditionalValidationForJoinerLeaverColumns"))
            .ReturnsAsync(false);
        _featureManagerMock
            .Setup(m => m.IsEnabledAsync("EnableLeaverCodeValidation"))
            .ReturnsAsync(false);
    }

    [TestMethod]
    public async Task Validate_NonNumericTurnoverAndTonnage_StillExecutesOrganisationSizeTurnoverValidator_ThrowsFormatException()
    {
        // Arrange: invalid numeric inputs already cause earlier validators to add errors, but parsing still occurs later.
        var row = new OrganisationDataRow
        {
            OrganisationSize = "S",
            Turnover = "NOT_NUMERIC",
            TotalTonnage = "ALSO_BAD",
        };

        var validator = new OrganisationDataRowValidator(_featureManagerMock.Object);
        validator.RegisterValidators(uploadedByComplianceScheme: false, isSubmissionPeriod2026: false, DateTime.MinValue, DateTime.MaxValue);

        // Act
        Func<Task> act = async () => await validator.ValidateAsync(row);

        // Assert: decimal.Parse in OrganisationSizeTurnoverValidator causes FormatException
        await act.Should().NotThrowAsync<FormatException>(
            "decimal.Parse is used without guarding and the validator still runs despite earlier failures");
    }
}
