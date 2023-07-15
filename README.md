# EPR Registration Validation Function

## Overview

This function listens to an Azure ServiceBus Queue, for messages indicating a user has uploaded a registration file via the front end, to blob storage. It retreives the file, and calculates whether further uploads of Brands and Partnership files are needed.
 
## How To Run 
 
### Prerequisites 
In order to run the service you will need the following dependencies 
 
- .Net 6 
 
### Dependencies 
 
 
 
### Run 
 On root directory, execute
```
make run
```
### Docker
Run in terminal at the solution source root -

```
docker build -t registrationvalidation -f EPR.RegistrationValidation.Functions/Dockerfile .
```

Fill out the environment variables and run the following command -
```
docker run -e AzureWebJobsStorage:"X" -e FUNCTIONS_WORKER_RUNTIME:"X" -e FUNCTIONS_WORKER_RUNTIME_VERSION:"X" -e ServiceBus:ConnectionString="X" -e ServiceBus:UploadQueueName="X" -e StorageAccount:ConnectionString="X" -e StorageAccount:BlobContainerName="file" -e SubmissionApi:BaseUrl="X" -e SubmissionApi:SubmissionEndpoint="X" -e SubmissionApi:Version="X" -e SubmissionApi:SubmissionEventEndpoint="X" -e SubmissionApi:ValidationEventType=0 registrationvalidation
```

## How To Test 
 
### Unit tests 

On root directory, execute
```
make unit-tests
```
 
 
### Pact tests 
 
N/A
 
### Integration tests

N/A
 
## How To Debug 
 
 
## Environment Variables - deployed environments 
A copy of the configuration file and a description of each variable can be found [here](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4343267583/Registration+Validation+Variables).

Also please add the serviceDepedencies.local.json to EPR.RegistrationValidation.Functions/Properties. It can be found [here](https://defra.sharepoint.com/teams/Team1478/Digital/Forms/AllItems.aspx?id=%2Fteams%2FTeam1478%2FDigital%2FEPR%20Digital%202022%2FDevelopment%20Artefacts%20RESTRICTED%2FRegistration%20Validation&viewid=59eed614%2D88d7%2D4c83%2D92d6%2D6532031f9fa9&OR=Teams%2DHL&CT=1679588745055&clickparams=eyJBcHBOYW1lIjoiVGVhbXMtRGVza3RvcCIsIkFwcFZlcnNpb24iOiIyNy8yMzAyMDUwMTQyMSIsIkhhc0ZlZGVyYXRlZFVzZXIiOmZhbHNlfQ%3D%3D). 

## Additional Information 
 
### Logging into Azure 
 
### Usage 
 
### Monitoring and Health Check 
 
## Directory Structure 

### Source files 
- `EPR.RegistrationValidation.Application` - Application .Net source files
- `EPR.RegistrationValidation.Data` - Data .Net source files
- `EPR.RegistrationValidation.Functions` - Function .Net source files
- `EPR.RegistrationValidation.UnitTests` - .Net unit test files
 
### Source packages 

## Contributing 
 
