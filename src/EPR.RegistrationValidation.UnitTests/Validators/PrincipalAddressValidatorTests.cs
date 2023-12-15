namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PrincipalAddressValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullPrincipalAddressLine1WhenUnincorporated_IsNotValid()
    {
        // Arrange
        var validator = new PrincipalAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "12344",
            OrganisationTypeCode = UnIncorporationTypeCodes.CoOperative,
            PrincipalAddressPostcode = "KT5 9JW",
            PrincipalAddressPhoneNumber = "0123456789",
            PrincipalAddressLine1 = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrincipalAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressLine1);
    }

    [TestMethod]
    public async Task Validate_WithNullPrincipalAddressPostcodeWhenUnincorporated_IsNotValid()
    {
        // Arrange
        var validator = new PrincipalAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "12344",
            OrganisationTypeCode = UnIncorporationTypeCodes.CoOperative,
            PrincipalAddressPostcode = null,
            PrincipalAddressPhoneNumber = "0123456789",
            PrincipalAddressLine1 = "5,SouthCote Ave",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrincipalAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPostcode);
    }

    [TestMethod]
    public async Task Validate_WithNullPrincipalAddressPhoneNumberWhenUnincorporated_IsNotValid()
    {
        // Arrange
        var validator = new PrincipalAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "12344",
            OrganisationTypeCode = UnIncorporationTypeCodes.CoOperative,
            PrincipalAddressPostcode = "KT5 9JW",
            PrincipalAddressPhoneNumber = null,
            PrincipalAddressLine1 = "5,SouthCote Ave",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrincipalAddressPhoneNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPhoneNumber);
    }

    [TestMethod]
    public async Task Validate_WithNullPrincipalAddressWhenUnincorporated_IsNotValid()
    {
        // Arrange
        var validator = new PrincipalAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "12344",
            OrganisationTypeCode = UnIncorporationTypeCodes.CoOperative,
            PrincipalAddressPostcode = null,
            PrincipalAddressPhoneNumber = null,
            PrincipalAddressLine1 = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPhoneNumber);
    }

    [TestMethod]
    public async Task Validate_PrincipalAddress_IsValid()
    {
        // Arrange
        var validator = new PrincipalAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = UnIncorporationTypeCodes.CoOperative,
            PrincipalAddressPostcode = "KT5 9JW",
            PrincipalAddressPhoneNumber = "0123456789",
            PrincipalAddressLine1 = "5,SouthCote Ave",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressLine1);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPostcode);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrincipalAddressPhoneNumber);
    }
}