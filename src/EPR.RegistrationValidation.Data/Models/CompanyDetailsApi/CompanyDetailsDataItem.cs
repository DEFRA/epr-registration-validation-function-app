namespace EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;

using Newtonsoft.Json;

public class CompanyDetailsDataItem
{
    [JsonProperty("RN")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("CHN")]
    public string CompaniesHouseNumber { get; set; }
}