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
- dotnet core 2.2

# Base JSON Structure
```json
    {
        "FormName": "",
        "BaseURL": "",
        "StartPageSlug": "",
        "FeedbackForm": "https://stockportcouncil.typeform.com/to/yzIJEe",
        "DocumentDownload": true,
        "DocumentType": [
          "Txt“
        ],
        "Pages": [
          {
            "Title": "",
            "HideTitle": ,
            "PageSlug": "",
            "Elements": [],
            "Behaviours": []
          }
        ],
    }
```
**FormName** (*string*) - Name of the form - Will display in browser tab name

**BaseURL** (*string*) - "test-form" would create stockport.gov.uk/test-form/

**StartPageSlug** (*string*) - The first PageSlug users will visit

**FeedbackForm** (*string*) - If present this will be used at the top of the form to link them to, commonly, a TypeForm form that will be created to capture feedback on the new form

**DocumentDownload** (*bool*) - Enable Document download (optional)

**DocumentType** (*Array[string]*) - If Document download is enabled what document type is required.

Allowed Values:
* **Txt**

**Pages**[*object*]:

* **PageSlug** (*string*) - The slug for a page after the BaseURL e.g. stockport.gov.uk/{BaseURL}/{PageSlug}
* **HideTitle** (*bool*) - This will hide the page title when set to true
* **Title** (*string*) - The Title is used within the browser tab along with the form name e.g. Page 1 - Contact us - Stockport Council
* **Elements**[*object*]- List of HTML elements to display on page
* [Behaviours[*object*]](#pagebehaviours) - List of conditionals & page redirects for the page

## Controlling Form Availaiblity ##

Form availability can be controlled per Environment by setting a boolean availability value against the ```ASPNETCORE_ENVIRONMENT``` value for the specified environment. 

For example:

 ```json
 {
  "FormName": "My test form",
  "BaseURL": "my-test-form",
  "EnvironmentAvailabilities":[
    {
      "Environment": "Local",
      "IsAvailable": true
    },
    {
      "Environment": "Int",
      "IsAvailable": false
    }
  ],
  ...
 ```

If the availabilities block is not present or the availability for the requested environment is not specified **forms will be assumed to be available**.

## Element Types & Properties
**Elements[*object*]**:
## Target Mapping

Example JSON:
```json
    {
        "Elements": [
            {
                "Type": "Textbox",
                "Properties":
                {
                    "Label": "Enter your first name",
                    "Name": "firstName",
                    "QuestionId": "first-name",
                    "TargetMapping": "customer.firstname"
                }
            }
        ]
    }
```
When building elements you have the ability to map the answer to custom properties. To allow for this you can specify a `"TargetMapping"` property which will be used to map the answer too. The example above would map the firstName textbox to a Customer object with the firstname property. The ability to map multiple questions to the same field is also posibile. An example below shows a custom mapping.

Target Mapping EXample:
```
    {
        "Elements": [
            {
                ...
                "Properties":
                {
                    ...
                    "TargetMapping": "customer.firstname"
                }
            },
            {
                ...
                "Properties":
                {
                    ...
                    "TargetMapping": "customer.lastname"
                }
            },
            {
                ...
                "Properties":
                {
                    ...
                    "TargetMapping": "customer.additionalinfomation"
                }
            },
            {
                ...
                "Properties":
                {
                    ...
                    "TargetMapping": "customer.additionalinfomation"
                }
            },
             {
                ...
                "Properties":
                {
                    ...
                    "TargetMapping": "one.two.three"
                }
            }
        ]
    }
```
The target mapping above would produce this object
```json
    {
        "customer": {
            "lastname": "",
            "firstname": "",
            "additionalinformation": ""
        },
        "one": {
            "two": {
                "three": ""
            }
        }
    }
```

* **Generic Options**
    * [LegendAsH1](#LegendAsH1) (Set legend to a h1 heading)
    * [LabelAsH1](#LabelAsH1) (Set label to a h1 heading)


* <a name="LegendAsH1">**LegendAsH1**</a>
    * LegendAsH1 (*boolean*) (defaults to false)

      This is only valid for the elements listed below:    
      * **Select**
      * **DateInput**
      * **DatePicker**
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

* <a name="LabelAsH1">**LabelAsH1**</a>
    * LabelAsH1 (*boolean*) (defaults to false)

      This is only valid for the elements listed below:    
      * **Textbox**
      * **Textarea**
      * **FileUpload**
      * **Numeric**

    Sets the heading level of the legend to H1
    ```json
    {
    "Type": "Textbox",
        "Properties": {
            "LabelAsH1": true
        }
    }
    ```
#

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
    

* **Properties** (*object*) (Prop types of an element - * = Mandatory)
#
   * <a name="headingprops">**H2-H6** (Heading levels)</a>
        * Text (*string*) __*__
#
   * <a name="ptextprops">**P** (Paragraph text)</a>
        * Text (*string*) (Can embed HTML code e.g. \<strong\>) __*__

Paragraph text JSON example:
```json
{
  "Type": "p",
  "Properties": {
    "Text": "<strong>This is strong text</strong>"
  }
}
```
#
   * <a name="requiredif">**Required if**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * MaxLength (*int*) (defaulted to 200)
        * Optional (*boolean*) (need to be set as true)
        * RequiredIf (*string*)
        * TargetMapping (*string*)  
        
### Textbox JSON example when required if is being used:
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "niNumber",
      "Label": "NI Number",
      "Optional": true,
      "MaxLength": 9,
      "RequiredIf" : "questionOne:yes"
    }
  }
```
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


Example where dependant on their answers to certain questions the form navigates to different pages (the questions don't need to have been answered on that page but can have been answered earlier in the form:
```json
    {
        "Behaviours": [
            {
                "Conditions": [
                    {
                        "QuestionID": "DoYouLikeApples",
                        "EqualTo": "Yes"
                    },
                    {
                        "QuestionID": "DoYouLikeOranges",
                        "EqualTo": "Yes"
                    },
                ],
                "BehaviourType": "GoToPage",
                "PageSlug": "you-like-apples-oranges"
            },
            {
                "Conditions": [
                    {
                        "QuestionID": "DoYouLikeApples",
                        "EqualTo": "No"
                    },
                    {
                        "QuestionID": "DoYouLikeOranges",
                        "EqualTo": "No"
                    },
                ],
                "BehaviourType": "GoToPage",
                "PageSlug": "you-dont-like-apples-oranges"
            },        ]
    }
```
***Date After and DateBefore Example

```json
 "Behaviours": [
        {
          "Conditions": [
            {
              // If Today is before 43 days before the testDate then...
              "QuestionID": "testDate",
              "IsBefore": 43,
              "ComparisonDate": "Today",
              "Unit": "day"
            }
          ],
          "BehaviourType": "GoToPage",
          "PageSlug": "page-is-before"
        },
        {
          "Conditions": [
            {
              // If testDate is before 43 days from today the  then...
              "QuestionID": "testDate",
              "IsAfter": 1,
              "ComparisonDate": "Today",
              "Unit": "year"
            }
          ],
          "BehaviourType": "GoToPage",
          "PageSlug": "page-is-after"
        },
        {
          "Conditions": [
            {
              // If date is after one year from today
              "QuestionID": "testDate",
              "IsAfter": 1,
              "ComparisonDate": "Today",
              "Unit": "year"
            }
          ],
          "BehaviourType": "GoToPage",
          "PageSlug": "page-is-after"
        },
        {
          "conditions": [],
          "behaviourType": "GoToPage",
          "PageSlug": "page-two"
        }
      ]
    }

```

**Conditions**[*object*]
* QuestionID (*string*) - The name of the Radio/Checkbox list to evaluate
* EqualTo (*string*) - The value it must equal to for the behaviour to happen
* CheckboxContains (*string*) - The value will be checked against the list supplied by the checkbox. If it is found it will go to the behaviour
* DateBefore (*int*) - The value will be checked against the dateBefore if the date is less than comparison date it will got to the behavior. Units are either year,month day. 
* DateAfter (*int*) - The value will be checked against the dateAfter if te date is less than camparison date it will got to the behavior. Units are either year,month day. 

**BehaviourType** (*enum*) __*__
* 0 = GoToPage
* 1 = SubmitForm
* 2 = GoToExternalPage

**PageSlug** (*string*) __*__ - The PageSlug the form will redirect to

## Success Page

The success page is a page with with the PageSlug of success it is of the form it should be at the end of the form afte the submit.

```json
{
  "Title": "Thank you for submitting your views on fruit",
  "PageSlug": "success",
  "Elements": [
    {
      "Type": "p",
      "Properties": {
        "Text": "The wikipedia page on fruit is at <a href=\"https://en.wikipedia.org/wiki/fruit\">Fruits</a>"
      }
    }]
}
```
It will also diplay the form name and the FeedbackUrl if one is specified.

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
