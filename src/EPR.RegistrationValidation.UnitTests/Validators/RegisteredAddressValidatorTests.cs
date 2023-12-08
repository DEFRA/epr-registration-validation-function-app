namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RegisteredAddressValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullRegisteredAddressLine1InChOrganisationTypeCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = IncorporationTypeCodes.LimitedCompany,
            RegisteredAddressPostcode = "WF3 8IP",
            RegisteredAddressPhoneNumber = "012397824",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.RegisteredAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressLine1);
    }

    [TestMethod]
    public async Task Validate_WithNullRegisteredAddressPostcodeInChOrganisationTypeCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = IncorporationTypeCodes.LimitedCompany,
            RegisteredAddressLine1 = "1, street",
            RegisteredAddressPhoneNumber = "012397824",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.RegisteredAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPostcode);
    }

    [TestMethod]
    public async Task Validate_WithNullRegisteredAddressPhoneNumberInChOrganisationTypeCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = IncorporationTypeCodes.LimitedCompany,
            RegisteredAddressLine1 = "1, street",
            RegisteredAddressPostcode = "WF4 2PR",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.RegisteredAddressPhoneNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPhoneNumber);
    }

    [TestMethod]
    public async Task Validate_WithNullRegisteredAddressAndNotInChOrganisationTypeCode_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = "InvalidCode",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressLine1);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPostcode);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPhoneNumber);
    }

    [TestMethod]
    public async Task Validate_WithOrganisationId_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            OrganisationTypeCode = IncorporationTypeCodes.LimitedCompany,
            RegisteredAddressLine1 = "2, street",
            RegisteredAddressPostcode = "WF3 8IP",
            RegisteredAddressPhoneNumber = "012397824",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressLine1);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPostcode);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingRegisteredAddressPhoneNumber);
    }

    private static RegisteredAddressValidator CreateValidator()
    {
        return new RegisteredAddressValidator();
    }
}