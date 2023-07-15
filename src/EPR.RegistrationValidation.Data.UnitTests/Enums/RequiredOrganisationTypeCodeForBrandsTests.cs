namespace EPR.RegistrationValidation.Data.UnitTests.Enums;

using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RequiredOrganisationTypeCodeForBrandsTests
{
    [TestMethod]
    public void EnumValues_ShouldMatchExpectedValues()
    {
        RequiredOrganisationTypeCodeForPartners.LLP.ToString().Should().Be("LLP");
        RequiredOrganisationTypeCodeForPartners.LPA.ToString().Should().Be("LPA");
        RequiredOrganisationTypeCodeForPartners.PAR.ToString().Should().Be("PAR");
    }
}