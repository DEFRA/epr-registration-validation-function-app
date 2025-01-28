namespace EPR.RegistrationValidation.Application.Services.Subsidiary
{
    using EPR.RegistrationValidation.Data.Models;
    using EPR.RegistrationValidation.Data.Models.Subsidiary;

    public interface ISubsidiaryDetailsRequestBuilder
    {
        SubsidiaryDetailsRequest CreateRequest(List<OrganisationDataRow> rows);
    }
}
