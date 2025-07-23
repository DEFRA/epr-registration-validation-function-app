namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using FluentValidation;

public class StatusCodeValidator : AbstractCodeValidator
{
    public StatusCodeValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
        : base(uploadedByComplianceScheme, enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (!enableLeaverCodeValidation)
        {
            RuleFor(r => r.LeaverCode)
                .Must(x => StatusCodeIsValid(x))
                .When(r => !string.IsNullOrEmpty(r.LeaverCode))
                .WithErrorCode(ErrorCodes.InvalidStatusCode);
        }
    }

    private bool StatusCodeIsValid(string statusCode)
    {
        return statusCode == StatusCode.A ||
            statusCode == StatusCode.B ||
            statusCode == StatusCode.C ||
            statusCode == StatusCode.D ||
            statusCode == StatusCode.E ||
            statusCode == StatusCode.F ||
            statusCode == StatusCode.G ||
            statusCode == StatusCode.H ||
            statusCode == StatusCode.I ||
            statusCode == StatusCode.J ||
            statusCode == StatusCode.K ||
            statusCode == StatusCode.L ||
            statusCode == StatusCode.M ||
            statusCode == StatusCode.N ||
            statusCode == StatusCode.O ||
            statusCode == StatusCode.P ||
            statusCode == StatusCode.Q;
    }
}
