<h1 align="center">Form Builder👷</h1>
<div align="center">:page_facing_up: :pencil2:</div>
<div align="center">
  <sub>Built with ❤︎ by
  <a href="https://www.stockport.gov.uk">Stockport Council</a>
</div>

## Table of Contents
- [Requirements & Prereqs](#requirements-&-prereqs)
- [Environments](#environments)
- [Wiki](https://github.com/smbc-digital/form-builder/wiki)
- [Validators](https://github.com/smbc-digital/form-builder/wiki/Validators)
- [Address/Street/Organisation Providers](#address-providers)
- [Payment Providers](#payment-providers)
- [Storage Providers](#storage-providers)
- [Running High Availability](#running-high-availability)
- [Structure Tests](#structure-tests)
- [Preview](#preview)
- [Data Structure Preview](#data-structure-preview)

# Requirements & Prereqs
- dotnet core 3.1
- GPG key added to collaborators

# Environments

Which environment and configuration is used depends on the value of the `ASPNETCORE_ENVIURONMENT` environment variable.

For example if set to `int` then by convention the following configuration files are required and will be loaded.

* config/appsettings.json
* config/appsettings.int.json
* config/secrets/appsettings.secrets.json
* config/secrets/appsettings.int.secrets.json

Additionally if you need to deploy multiple different versions but using the same environment can specify and additional `FORMBUILDER_ENVIRONMENT` environment variable

For example if `ASPNETCORE_ENVIRONMENT` set to `int` and `FORMBUILDER_ENVIRONMENT` set to `Internal` then by convention the following configuration files are required and will be loaded.

* config/appsettings.json
* config/appsettings.int.internal.json
* config/secrets/appsettings.secrets.json
* config/secrets/appsettings.int.internal.secrets.json

## How this effects Forms 

The `FORMBUILDER_ENVIRONMENT` variable is not currently used in the form defenitions - so the submit behaviour below would be valid for any deployment with an `ASPNETCORE_ENVIRONMENT` of `Int`

```
 "Behaviours": [
        {
          "conditions": [],
          "behaviourType": "SubmitForm",
          "SubmitSlugs": [
            {
              "Environment": "Int",
              "URL": "{my-target-url}",
              "AuthToken": "int-authtoken"
            }
          ]
        }
      ]
```
# Startup

By default when the application starts it will use `Startup.cs` to define the applications behaviour and dependecies based on configuration.

There are some scenarios, where different configurations need to be deployed to different environments, and as such we might want to define a seperate startup with different behaviours and dependencies. For example we might want to specify we use local file SchemaProvider as opposed to S3 for an internal deployment. 

This is done based on the `FORMBUILDER_ENVIRONMENT` envinroment variable. If specified the application will by convention attempt to use `Startup{FORMBUILDER_ENVIRONMENT}.cs`.

For example if `FORMBUILDER_ENVIRONMENT` set to `Internal` the file `StartupInternal.cs` must be present and will be used to define application behaviour and dependencies.

# Providers
## Address Providers

`IAddressProvider` is provided to enable integration with different address data sources. 

The interface requires a ProviderName and a SearchAsync method which must return an `AddressSearchResult` object. 

```c#
string ProviderName { get; }

Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode);
```

You can register new/multiple address providers in startup 

```c#
services.AddSingleton<IAddressProvider, FakeAddressProvider>();
services.AddSingleton<IAddressProvider, CRMAddressProvider>();
services.AddSingleton<IAddressProvider, MyCustomAddressProvider>();
```

You can specify in the JSON form defenition for Address elements, which address provider you want use. To do this set `AddressProvider` to the value of the required `IAddressProvider.ProviderName`, for example to use a Provider with a ProviderName of "Fake";

```json
{
  "Type": "Address",
  "Properties": {
    "QuestionId": "address",
    "AddressProvider": "Fake",
  }
}
```

## Street Providers

`IStreetProvider` is provided to enable integration with different street level data sources. 

Implementation is almost identical to that described in the AddressProvider

Register for use 

```c#
services.AddSingleton<IStreetProvider, FakeStreetProvider>();
services.AddSingleton<IStreetProvider, CRMStreetProvider>();
return services;
```

Specify which street provider to use.

```json
"Elements": [
  {
    "Type": "Street",
    "Properties": {
      "QuestionId": "customersstreet",
      "StreetProvider": "Fake",
    }
```


## Organisation Providers

`IOrganisationProvider` is provided to enable integration with different organisation data sources. 

Implementation is almost identical to that described in the AddressProvider/StreetProvider, however Organisation Providers return an `OrganisationSearchResult`

Register for user

```c#
services.AddSingleton<IOrganisationProvider, FakeOrganisationProvider>();
services.AddSingleton<IOrganisationProvider, CRMOrganisationProvider>();
return services;
```

Specify which organisation provider to use.

```json
{
  "Type": "Organisation",
  "Properties": {
    "QuestionId": "organisation",
    "Label": "Enter the organisation",
    "OrganisationProvider": "Fake",
  }
```
## Payment Providers

`IPaymentProvider` is provided to enable integration with different payment systems, this assumes that you are redirected to an external payment provider where a payment is taken and then redirected back to the forms application to continue form processing.
  
The payment provider enables this on the method `IPaymentProvider.GeneratePaymentUrl` which carries out any pre-payment actions, such as setting up payment/baskets etc. and the returns the URL to which form builder should direct the user.
  
The payment provider also abstracts the detail of understanding the payment response using the `IPaymentProvider.VerifyPaymentResponse` method. The payment provider return nothing (if payment success) or throw an exception of type `PaymentDeclinedException` or `PaymentFailurexception` depending on the payment repsponse.
  
### A note about workflow
In order to maintain auditability and tracking the payments work flow enables an initial submission pre-payment, typically used to setup cases etc. and a further callback post payment to update cases, bookings etc. reflecting the result of the payment process.
 
Regsitering a payment provider for use

```c#
services.AddSingleton<IPaymentProvider, FakePaymentProvider>();
services.AddSingleton<IPaymentProvider, MyRealPaymentProvider>();
return services;
```

Specify which organisation provider to use.

```json
  {
    "formName": [ "my-payment-form" ],
    "PaymentProvider": "MyRealPay",
    "settings": {
      "accountReference": "our-account-reference",
      "addressReference": "{{QUESTION:Address}}",
      "name": "{{QUESTION:firstName}} {{QUESTION:lastName}}",
      "email": "{{QUESTION:email}}",
      "catalogueId": "our-catalog-id",
      ...
    }
```
  
### Fake payment provider
 
In order to enable a faster testing process of forms and formbuilder capability `FakePaymentProvider` has been created. This functionality is turned off by default and should never be turned on in a production environment as it will effect every payment regardless of specified payment provider.
  
Fake payment will act exactly the same as any other payment provider in terms of input and output - but will allow the Tester/QA to specift the desired payment response (Success, Declined, Failure)
  
To turn fake payment on set the following value in the relevant environment configuration.
 
```"PaymentConfiguration:FakePayment": true```

## Storage Providers

As users submit data and move through forms the data they are submitting is temporarily stored before submission.

This function uses [IDistributedCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.1).

Storage providers can be added from config and registered on app startup using.

```c#
services.AddStorageProvider(Configuration);
```

### Application
By default setup using config currently supports "Application", Distributed In Memory Cache (which ironically isn't actually distributed!), but allows us to develop locally and deploy to single instances.

This can be set up using the following config

```json
  {
    "StorageProvider": {
      "Type": "Application"
    }
  }
```

### Redis
For a truly distributed we support Redis, this can be setup using the StorageProvider config below.

```json
{
  "StorageProvider": {
      "Type": "Redis",
      "Address": "YourRedisUrlHere",
      "Instance": "YourPreferredInstanceNameHere"
  }
}
```

### Running High Availability/Load Balancing
If running in a load balanced environment you will need to be running a truly distributed cache (i.e. Redis).

In this case it's worth noting that for sessions to work correctly there needs to be a shared keystore.

Distributed cache and DataProtecton key storage specification is wrapped up in the ```services.AddStorageProvider(Configuration);``` service registration (we should seperate this out!) so when you set storage provider to 'Redis' it also shares the data protection keys in redis as well (as per the code below).

```c#
  var redis = ConnectionMultiplexer.Connect(storageProviderConfiguration["Address"]);
  services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, $"{storageProviderConfiguration["InstanceName"]}DataProtection-Keys");
```

# Structure Tests
  
Structure tests use Cypress to compare the DOM structure of components against snapshots. New structure tests should be added when a new component type is created. Existing tests may need to be modified, or snapshots replaced if the DOM structure of an existing component changes. More info on this feature can be found [here](https://github.com/smbc-digital/form-builder/wiki/Structure-tests)

# AWS Architecture Decisions #
This section will be used to document the decisions made throughout the development process.

## Form Builder Storage Stack
The formbuilder storage components will be provisioned by a Cloudformation stack. Jon H & Jake decided that the stack would include an S3 Bucket, IAM User and a User Policy to handle the storage of json files. 

### IAM User Policy Example
The example below shows the policy we manually created and applied to a test user:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:PutAccountPublicAccessBlock",
                "s3:GetAccountPublicAccessBlock",
                "s3:ListAllMyBuckets",
                "s3:ListJobs",
                "s3:CreateJob",
                "s3:HeadBucket"
            ],
            "Resource": "*"
        },
        {
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "arn:aws:s3:::bucketname/*",
                "arn:aws:s3:::bucketname"
            ]
        }
    ]
}
```
## FormBuilder Bucket Config
The S3 bucket used during testing was cofigured to 'Block all public access' to 'On'.

The bucket will be created using a Cloudformation template with the folder structure created by the template.

## Preview

Preview allows you to view and validate a form without the need of uploading it to your configured Form schema provider. More info on this feature can be found [here](https://github.com/smbc-digital/form-builder/wiki/Preview-Mode).
  
## Data Structure Preview
  
Data Structure Preview allows you to view the data structure that will be submitted to the form Service. More info on this feature can be found [here](https://github.com/smbc-digital/form-builder/wiki/Preview-data-structure)

## Analytics
  
Analytic events can be enabled within appsettings.json `Analytics` config section, by default this is GA but can be extended by creating your own `IAnalyticsProvider`

```json
{
  "Analytics": {
    "Enabled": true,
    "Type": "GA"
  }
}
```

`IAnalyticsProvider` is provided to enable integration with different analytics providers. The interface requires a ProviderName and a RaiseEventAsync method.


Analaytics event on successful form submission can be raised in the configured Analytics provider. More info on this feature can be found [here](https://github.com/smbc-digital/form-builder/wiki/Analaytics)
