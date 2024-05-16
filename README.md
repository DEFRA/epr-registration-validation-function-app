# EPR Registration Validation Function

## Overview

This function listens to an Azure ServiceBus Queue, for messages indicating a user has uploaded a registration file via the front end, to blob storage. It retreives the file, and calculates whether further uploads of Brands and Partnership files are needed.

## Service Bus Payload

```json
{
    "BlobName":"19f3c7de-d41e-4eb1-b663-a31b033e5db9",
    "SubmissionId":"fa5a047c-182a-444b-bd93-ea25cb5f9d82",
    "SubmissionSubType":"CompanyDetails",
    "OrganisationId":"b672cb31-5c72-48b3-8bd3-ef8493e138ce",
    "UserId":"43683d87-2152-4654-be18-c0a0dcab92db",
    "UserType":"Producer"
}
```

## How To Run 
 
### Prerequisites 
In order to run the service you will need the following dependencies:
 
- .NET 6
- Azure CLI
 
### Run 
Go to `src/EPR.RegistrationValidation.Functions` directory and execute:

```
func start
```

### Docker

Run in terminal at the solution source root:

```
docker build -t registrationvalidation -f EPR.RegistrationValidation.Functions/Dockerfile .
```

Fill out the environment variables and run the following command:

```
docker run -e AzureWebJobsStorage="X" -e FUNCTIONS_EXTENSION_VERSION="X" -e FUNCTIONS_WORKER_RUNTIME="X" -e ServiceBus:ConnectionString="X" -e ServiceBus:UploadQueueName="X" -e StorageAccount:BlobContainerName="X" -e StorageAccount:ConnectionString="X" -e SubmissionApi:BaseUrl="X" -e SubmissionApi:SubmissionEndpoint="X" -e SubmissionApi:SubmissionEventEndpoint="X" -e SubmissionApi:Version="X" -e ValidationSettings:ErrorLimit="X" -e CompanyDetailsApi:BaseUrl="X" -e CompanyDetailsApi:Timeout="X" -e CompanyDetailsApi:ClientId="X" -e FeatureManagement:EnableRowValidation="X" -e FeatureManagement:EnableOrganisationDataRowValidation="X" -e FeatureManagement:EnableBrandPartnerDataRowValidation="X" -e FeatureManagement:EnableBrandPartnerCrossFileValidation="X" -e FeatureManagement:EnableCompanyDetailsValidation="X" registrationvalidation
```

## How To Test 
 
### Unit tests 

On root directory `src`, execute:

```
dotnet test
```
 
### Pact tests 
 
N/A
 
### Integration tests

N/A
 
## How To Debug 

Use debugging tools in your chosen IDE. 
 
## Environment Variables - deployed environments 

The structure of the appsettings can be found in the repository. Example configurations for the different environments can be found in [epr-app-config-settings](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr-app-config-settings).

| Variable Name                          | Description                                                                                            |
| -------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| AzureWebJobsStorage                    | The connection string for the Azure Web Jobs Storage                                                   |
| FUNCTIONS_EXTENSION_VERSION            | The extension version for Azure Function - i.e. ~4                                                     |
| FUNCTIONS_WORKER_RUNTIME               | The runtime name for the Azure Function - i.e. `dotnet`                                                |
| ServiceBus__ConnectionString           | The connection string for the service bus                                                              |
| ServiceBus__UploadQueueName            | The name of the upload queue                                                                           |
| StorageAccount__BlobContainerName      | The name of the blob container on the storage account, where uploaded file will be stored              |
| StorageAccount__ConnectionString       | The connection string of the blob container on the storage account, where uploaded file will be stored |
| SubmissionApi__BaseUrl                 | The base URL for the Submission Status API WebApp                                                      |
| SubmissionApi__SubmissionEndpoint      | The 'Submission' endpoint of the Submission Status API WebApp - i.e. 'submissions'                     |
| SubmissionApi__SubmissionEventEndpoint | The 'SubmissionEvent' endpoint of the Submission Status API WebApp - i.e. 'events'                     |
| SubmissionApi__Version                 | The version of the Submission Status API WebApp                                                        |

Also add the serviceDepedencies.local.json to ```EPR.RegistrationValidation.Functions/Properties``` with the following content: 

```json
{
  "dependencies": {
    "appInsights1": {
      "type": "appInsights.sdk"
    },
    "storage1": {
      "type": "storage.emulator",
      "connectionId": "AzureWebJobsStorage"
    }
  }
}
```

## Additional Information 

See [ADR-021: Registration Data Upload](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4291657945/ADR-021+Registration+Data+Upload) and [ADR-055: Organisation data validation](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4503240724/ADR-055+Organisation+data+validation)
 
### Monitoring and Health Check 

Enable Health Check in the Azure portal and set the URL path to ```ServiceBusQueueTrigger```

## Directory Structure 

### Source files 

- `EPR.RegistrationValidation.Application` - Application .NET source files
- `EPR.RegistrationValidation.Data` - Data .NET source files
- `EPR.RegistrationValidation.Data.UnitTests` - Data .NET unit test files
- `EPR.RegistrationValidation.Functions` - Function .NET source files
- `EPR.RegistrationValidation.UnitTests` - .NET unit test files

## Contributing to this project

Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).
