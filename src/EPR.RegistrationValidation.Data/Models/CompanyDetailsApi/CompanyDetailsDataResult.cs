namespace EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;

using Newtonsoft.Json;

public class CompanyDetailsDataResult
{
    [JsonProperty(nameof(Organisations))]
    public IEnumerable<CompanyDetailsDataItem> Organisations { get; set; }
}
