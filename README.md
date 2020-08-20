<h1 align="center">Form Builder👷</h1>
<div align="center">:page_facing_up: :pencil2:</div>
<div align="center">
  <sub>Built with ❤︎ by
  <a href="https://www.stockport.gov.uk">Stockport Council</a>
</div>

## Table of Contents
- [Requirements & Prereqs](#requirements-&-prereqs)
- [Base JSON Structure](#base-json-structure)
- [Validators](#validators)
- [Address/Street/Organisation Providers](#address-providers)
- [Payment Providers](#payment-providers)
- [Storage Providers](#storage-providers)
- [Running High Availability](#running-high-availability)
- [UI Tests](#ui-Tests)

# Requirements & Prereqs
- dotnet core 3.1

# Element Types and Properties

## Generic Options
    * [LegendAsH1](#LegendAsH1) (Set legend to a h1 heading)
    * [LabelAsH1](#LabelAsH1) (Set label to a h1 heading)


### <a name="LegendAsH1">**LegendAsH1**</a> (*boolean*) (defaults to false)

      This is only valid for the elements listed below:    
      * **DateInput**
      * **Address**
      * **Organisation**
      * **Street**
      * **Radio**
      * **Checkbox**
      * **Declaration**
      * **Time**

    Sets the heading level of the legend to H1
    ```json
    {
    "Type": "Radio",
        "Properties": {
            "LegendAsH1": true
        }
    }
    ```

### <a name="LabelAsH1">**LabelAsH1**</a>(*boolean*) (defaults to false)

      This is only valid for the elements listed below:    
      * **Select**
      * **Textbox**
      * **Textarea**
      * **FileUpload**
      * **Numeric**
      * **DatePicker**

    Sets the heading level of the legend to H1
    ```json
    {
    "Type": "Textbox",
        "Properties": {
            "LabelAsH1": true
        }
    }
    ```
### A note about Labels

Most controls come with labels which can be configured in that controls properties.

In some cases there maybe a need to emphasize a label (this is seen commonly on the address control), in the case the ```"StrongLabel": true``` property should be specified.

**note**: This feature is new an may not work in some controls.


### **[Lookups](https://github.com/smbc-digital/form-builder/wiki/Lookup)** (*string*)
For controls that have multiple options you can use the Lookup property to reference an existing list.

### **[Reusable Elements](https://github.com/smbc-digital/form-builder/wiki/ReusableElements)**
It's possible to build a library of consistent elements that can used in multiple forms. These use a special element called "Reusable".

Similar to a lookup they allow to specify and reference element, this element will be substituted for the specified reusable elements when the form schema loaded.

Reusable elements can contain references to lookups, so for example you could create a reusable element that specifies all the information surrounding a dropdown list (label, hint, validations etc.) but have the list content specified in the lookup.

## Available Elements

* **Type** (*string*) (HTML element)
    * [H2-H6](#headingprops) (Heading levels)
    * [P](#ptextprops) (Paragraph text)
    * [Textbox](https://github.com/smbc-digital/form-builder/wiki/Textbox)
    * [TextBox(Numeric)](https://github.com/smbc-digital/form-builder/wiki/Textbox)
    * [Textbox(Email)](https://github.com/smbc-digital/form-builder/wiki/Textbox)
    * [Textbox(Postcode)](https://github.com/smbc-digital/form-builder/wiki/Textbox)
    * [Textbox(Stockport postcode)](https://github.com/smbc-digital/form-builder/wiki/Textbox)
    * [RequiredIf](#requiredif)
    * [Textarea](https://github.com/smbc-digital/form-builder/wiki/Textarea)
    * [Radio](https://github.com/smbc-digital/form-builder/wiki/Radio)
    * [Select](https://github.com/smbc-digital/form-builder/wiki/Select)
    * [Checkbox](https://github.com/smbc-digital/form-builder/wiki/Checkbox)
    * [Declaration](https://github.com/smbc-digital/form-builder/wiki/Declaration)
    * [Link](https://github.com/smbc-digital/form-builder/wiki/Link)
    * [InlineAlert](https://github.com/smbc-digital/form-builder/wiki/InlineAlert)
    * [Button](https://github.com/smbc-digital/form-builder/wiki/Button)
    * [UL](https://github.com/smbc-digital/form-builder/wiki/UL) (Unordered List)
    * [OL](https://github.com/smbc-digital/form-builder/wiki/OL) (Ordered List)
    * [Img](https://github.com/smbc-digital/form-builder/wiki/Image) (Image)
    * [DateInput](https://github.com/smbc-digital/form-builder/wiki/DateInput)
    * [Address](https://github.com/smbc-digital/form-builder/wiki/Address)
    * [Street](https://github.com/smbc-digital/form-builder/wiki/Street)
    * [Time](https://github.com/smbc-digital/form-builder/wiki/Time)
    * [DatePicker](https://github.com/smbc-digital/form-builder/wiki/DatePicker)
    * [Organisation](https://github.com/smbc-digital/form-builder/wiki/Organisation)
    * [FileUpload](https://github.com/smbc-digital/form-builder/wiki/FileUpload)
    * [Reusable](https://github.com/smbc-digital/form-builder/wiki/ReusableElements)
    

## <a name="pagebehaviours">PageBehaviours[*object*]</a>
Example where if a user selects yes they will continue on with the form, otherwise they will submit their answer.
In the instance of a GoToPage it will expect a "PageSlug": "[url]" 
If it is a SubmitForm behaviour we have submitslugs which will (using the environment the form is running in e.g. local/Int/qa etc) determine the URL that the form will submit to.
:
```json
    {
        "PageBehaviours": [
            {
                "Conditions": [
                    {
                        "QuestionID": "DoYouLikeApples",
                        "EqualTo": "Yes"
                    }
                ],
                "BehaviourType": "GoToPage",
                "PageSlug": "why-do-you-like-apples"
            },
            {
                "Conditions": [],
                "BehaviourType": "SubmitForm",
                "PageSlug": "",
                "SubmitSlugs": [
                  {
                      "Location": "local",
                      "URL": "https://localhost:44359/api/v1/home",
                      "callbackUrl": "<url>",
                  },
                  {
                       "Location": "Int",
                       "URL": "http://scninthub-int1.stockport.gov.uk/formbuilderservice/api/v1/home",
                       "callbackUrl": "<url>",
                   },
                   {
                        "Location": "QA",
                        "URL": "http://scninthub-qa1.stockport.gov.uk/formbuilderservice/api/v1/home",
                        "callbackUrl": "<url>",
                    }
                    ]
            }
        ]
    }
```
**callbackUrl**

The callbackUrl property is used when third party systems are involved and logic has be performed on the receipt of a response from the third party system.
In the current code base this is used when a call is made to invoke Civica Pay and the result outcome specifies whether the payment was successful or not. In this scenario we need the form to behave accordingly.




# Validators

Comming Soon...

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

Comming Soon...

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

# UI Tests

The form builder app has UI tests to ensure that the UI is expected. Our UI Tests are written using SpecFlow

### Requirements
- chromdrivers.exe
- make (not required)

### How to run UI tests locally

```console
$ make ui-test
```

It is also possible to run just a specific test case rather then running the whole ui-test suite, this is made possible by running the `ui-test-feature` make command. with a FEATURE paramater as the test suite to run. The example below runs only the organisation ui-tests.
```console
$ make FEATURE=organisation ui-test-feature
```

without make you can also run the UI Test locally by running the two commands listed below

```console
$ cd ./src && dotnet run
```
```console
$ dotnet test ./form-builder-tests-ui/form-builder-tests-ui.csproj
```

It is possible to change the default browser the UI Tests are run from, to do this you need to modify the BrowserConfiguration to run with firefox or another browser of your choice.

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
