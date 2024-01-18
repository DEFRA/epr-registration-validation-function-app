namespace EPR.RegistrationValidation.Application.Validators;
using FluentValidation;

public static class ExtendedValidator
{
    public static IRuleBuilderOptions<T, string> IsInAllowedOptions<T>(this IRuleBuilder<T, string> ruleBuilder, params string[] allowedOptions)
    {
        return ruleBuilder.Must(property => string.IsNullOrEmpty(property) || allowedOptions.Contains(property, StringComparer.OrdinalIgnoreCase));
    }
}