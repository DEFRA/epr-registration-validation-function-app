namespace EPR.RegistrationValidation.Application.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureSection<TOptions>(
        this IServiceCollection services,
        string sectionKey,
        bool validate = true)
        where TOptions : class, new()
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (sectionKey == null)
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }

        services.AddOptions<TOptions>().Configure((TOptions options, IConfiguration config) =>
        {
            config.GetSection(sectionKey).Bind(options);
            if (validate)
            {
                var validationContext = new ValidationContext(options);
                var list = new List<ValidationResult>();
                if (!Validator.TryValidateObject(options, validationContext, list, validateAllProperties: true))
                {
                    IEnumerable<string> failureMessages = list.Select(r => r.ErrorMessage);
                    throw new OptionsValidationException(string.Empty, typeof(TOptions), failureMessages);
                }
            }
        });

        return services;
    }
}