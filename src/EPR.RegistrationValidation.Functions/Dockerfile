# FROM defradigital/dotnetcore-development:dotnet8.0 AS installer-env
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Copy everything
COPY stylecop.ruleset ./
COPY Directory.Build.props ./
COPY EPR.RegistrationValidation.Application/. ./EPR.RegistrationValidation.Application/.
COPY EPR.RegistrationValidation.Data/. ./EPR.RegistrationValidation.Data/.
COPY EPR.RegistrationValidation.Functions/. ./EPR.RegistrationValidation.Functions/.

RUN dotnet publish EPR.RegistrationValidation.Functions/*.csproj --output /home/site/wwwroot

# FROM defradigital/dotnetcore:dotnet8.0
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]