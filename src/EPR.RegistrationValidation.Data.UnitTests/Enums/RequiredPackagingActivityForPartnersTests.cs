namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RequiredPackagingActivityForPartnersTests
{
    [TestMethod]
    public void EnumValues_ShouldMatchExpectedValues()
    {
        RequiredPackagingActivityForBrands.Primary.ToString().Should().Be("Primary");
        RequiredPackagingActivityForBrands.Secondary.ToString().Should().Be("Secondary");
    }
}