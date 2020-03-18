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
        "HideTitle": ,
        "FeedbackForm": "https://stockportcouncil.typeform.com/to/yzIJEe",
        "Pages": [
          {
            "Title": "",
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

**FeedbackForm** (*string*) - If present this will be used at the top of the form to link them to, commonly, a TypeForm form that will be created to capture feedback on the new form.

**Pages**[*object*]:

* **PageSlug** (*string*) - The slug for a page after the BaseURL e.g. stockport.gov.uk/{BaseURL}/{PageSlug}
* **HideTitle** (*bool*) - This will hide the page title when set to true
* **Title** (*string*) - The Title is used within the browser tab along with the form name e.g. Page 1 - Contact us - Stockport Council
* **Elements**[*object*]- List of HTML elements to display on page
* [Behaviours[*object*]](#pagebehaviours) - List of conditionals & page redirects for the page


## Element Types & Properties
**Elements[*object*]**:

Example JSON:
```json
    {
        "Elements": [
            {
                "Type": "H1",
                "Properties":
                {
                    "Text": "Hello World",
                }
            },
            {
                "Type": "Textbox",
                "Properties":
                {
                    "Label": "Enter your first name",
                    "Name": "firstName",
                    "QuestionId": "first-name"
                }
            }
        ]
    }
```
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
    * [Textbox](#textboxprops)
    * [RequiredIf](#requiredif)
    * [TextBox(Numeric)](#textboxnumeric)
    * [Textbox(Email)](#textboxemailprops)
    * [Textbox(Postcode)](#textboxpostcodeprops)
    * [Textbox(Stockport postcode)](#textboxstockportpostcodeprops)
    * [Textarea](#textareaprops) (Large text box)
    * [Radio](#radioprops)
    * [Select](#selectprops)
    * [Checkbox](#checkboxprops)
    * [Link](#linkprops) (Anchor styled as button)
    * [InlineAlert](#inlinealertprops)
    * [Button](#buttonprops)
    * [UL](#Ulprops) (Unordered List)
    * [OL](#olprops) (Ordered List)
    * [Img](#Imgprops) (Image)
    * [DateInput](#DateInputprops)
    * [Address](#Address)
    * [Street](#Street)
    * [Time](#Time)
    * [DatePicker](#DatePicker)
    * [Organisation](#Organisation)
    * [FileUpload](#FileUpload)
    

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
   * <a name="textboxprops">**Textbox**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*) (defaulted to 200)
        * Optional (*boolean*) (defaults to false)
        * Regex (*string*) - The regex pattern to use
        * RegexValidationMessage (*string*) -  The validation message that appears if the input does not match the Regex pattern         (Default: Check the {Label} and try again)
        * TargetMapping (*string*)  
        
### Textbox JSON example (has to be a valid UK National Insurance Number):
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "niNumber",
      "Label": "NI Number",
      "Hint": "e.g: AB123456Z",     
      "Optional": false,
      "MaxLength": 9,
      "Regex": "^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$",
      "RegexValidationMessage": "Enter a valid NI Number",
      "TargetMapping": "customer.ninumber"
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
#
   * <a name="textboxnumeric">**Textbox(Numeric)**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)        
        * Optional (*boolean*) (defaults to false)
        * Numeric (*boolean*) (need this to validate it as an email address)
        * Max (*string*) (maxium value in integer e.g '25')
        * Min (*string*) (maxium value in integer e.g '0')
        * TargetMapping (*string*)  
        
Textbox Numeric JSON example:
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "howManyChildren",
      "Label": "How Many Children",
      "Hint": "3",
      "CustomValidationMessage": "The children should be between 0 and 20",
      "Optional": false,
      "Max": 20,
      "Min": 0,
      "Numeric": true
    }
  }
```
#
   * <a name="textboxemailprops">**Textbox(Email)**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*) (defaulted to 200)
        * Optional (*boolean*) (defaults to false)
        * Email (*boolean*) (need this to validate it as an email address)
        * TargetMapping (*string*)  
        
Textbox JSON example:
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "emailAddress",
      "Label": "Email address",
      "Hint": "ie: someone@example.com",
      "CustomValidationMessage": "Check the email address and try again",
      "Optional": false,
      "MaxLength": 60,
      "Email": true
    }
  }
```
#
   * <a name="textboxpostcodeprops">**Textbox(Postcode)**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*) (defaulted to 200)
        * Optional (*boolean*) (defaults to false)
        * Postcode (*boolean*) (need this to validate it as a postcode e.g. L15 3HJ)
        * TargetMapping (*string*)  
        
Textbox JSON example:
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "emailAddress",
      "Label": "Email address",
      "Hint": "ie: someone@example.com",
      "CustomValidationMessage": "Check the email address and try again",
      "Optional": false,
      "MaxLength": 60,
      "Postcode": true
    }
  }
```
#
   * <a name="textboxstockportpostcodeprops">**Textbox(Stockport postcode)**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*) (defaulted to 200)
        * Optional (*boolean*) (defaults to false)
        * StockportPostcode (*boolean*) (need this to validate it as a Stockport postcode e.g. SK1 3HJ)
        * TargetMapping (*string*)  
        
Textbox JSON example:
```json
  {
    "Type": "Textbox",
    "Properties": {
      "QuestionId": "emailAddress",
      "Label": "Email address",
      "Hint": "ie: someone@example.com",
      "CustomValidationMessage": "Check the email address and try again",
      "Optional": false,
      "MaxLength": 60,
      "StockportPostcode": true
    }
  }
```
#
   * <a name="textareaprops">**Textarea**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*) (defaults to 200)
        * Optional (*boolean*) (defaults to false)
        * TargetMapping (*string*)  

Textarea (Email) JSON example:
```json
  {
    "Type": "Textarea",
    "Properties": {
      "Label": "Enter your issue",
      "QuestionId": "issueOne",
      "CustomValidationMessage": "Custom validation message",
      "Hint": "Hint text",
      "MaxLength": "2000",
      "Optional": false
    }
  }
```
#
   * <a name="radioprops">**Radio**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
        * TargetMapping (*string*)  
 
 Radio JSON example:
 ```json
 {
    "Type": "Radio",
    "Properties": {
      "QuestionId": "radButton",
      "Label": "Do you like things?",
      "Hint": "<strong>Things</strong> like this and that",
      "Optional": true,
      "Options": [
        {
          "Text": "Yes",
          "Value": "yes",
          "Hint": "<strong>This</strong> is an affirmative response."
        },
        {
          "Text": "No",
          "Value": "no",
          "Hint": "This is a negative response."
        }
      ]
 }
 ```
#
   * <a name="selectprops">**Select**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
        * TargetMapping (*string*)  
```json
{
          "Type": "Select",
          "Properties": {
            "QuestionId": "select",
            "Label": "Favourite day of the week",
            "Hint": "This is the <strong> hint </strong>",
            "CustomValidationMessage": "Awooga",
            "Optional": false,
            "Options": [
              {
                "Text": "Sunday",
                "Value": "sunday"
              },
              {
                "Text": "Monday",
                "Value": "monday"
              },
              {
                "Text": "Tuesday",
                "Value": "tuesday"
              },
              {
                "Text": "Wednesday",
                "Value": "wednesday"
              },
              {
                "Text": "Thursday",
                "Value": "thursday"
              },
              {
                "Text": "Friday",
                "Value": "friday"
              },
              {
                "Text": "Saturday",
                "Value": "saturday"
              }
            ]
          }
        }
```        
#
   * <a name="checkboxprops">**Checkbox**</a>
        * Label (*string*) __*__
        * QuestionId (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
        * TargetMapping (*string*)  

  Checkbox JSON example:
  ```json
  {
    "Type": "Checkbox",
    "Properties": {
      "Hint": "You can select <strong>more than one option</strong>",
      "Label": "Select your favorite fruits",
      "CustomValidationMessage": "Please select at least one option",
      "QuestionId": "CheckBoxList",
      "Options": [
        {
          "Text": "Apples",
          "Value": "Apples",
          "Hint": "Usually green or red"
        },
        {
          "Text": "Oranges",
          "Value": "Oranges",
          "Hint": "Nothing rhymes with orange"
        },
        {
          "Text": "Bananas",
          "Value": "Bananas",
          "Hint": "High in potassium"
        }
      ]
    }
  },
  {
    "Type": "Checkbox",
    "Properties": {
      "Label": "Declaration",
      "Hint": "Macaroon candy canes bear claw pie cupcake sweet roll macaroon. Jelly-o cheesecake danish cheesecake marzipan fruitcake pastry chocolate cake. Gingerbread jelly pudding cheesecake donut sugar plum. Cake chocolate cake jelly-o cheesecake gummies.",
      "CustomValidationMessage": "You must check this box before proceeding",
      "QuestionId": "Declaration",
      "Options": [
        {
          "Text": "I agree to the terms and conditions regarding fruit selection",
          "Value": true
        }
      ]
    }
  }
 ```  
#
   * <a name="linkprops">**Link**</a>
        * Text (*string*) __*__
        * Name (*string*) __*__
        * URL (*string*) __*__
        * ClassName (*string*)
        * QuestionId (*string*)
#        
   * <a name="inlinealertprops">**InlineAlert**</a>
      * Text (*string*) 
      * Label (*string*) (at least one Text or Label must be entered for it to render) 
      
  InlineAlert JSON example:
  ```json
  {
    "Type": "InlineAlert",
    "Properties": {
      "Label": "This is the heading of the alert",
      "Text": "This is the description of the alert"
    }
  }
 ```     
 #
   * <a name="buttonprops">**Button**</a>
        * Text (*string*) (defaulted to "Next step")
        * ClassName (*string*) (defaulted to "button-primary")
        * ButtonId (*string*)
        * HidePreviousLink (*bool*) (defaulted to false)

Button JSON example:
```json
  {
    "Type": "Button",
    "Properties": {
      "ButtonId": "nextStep",
      "Text": "Custom text",
      "className": "button-secondary",
      "HidePreviousLink": true
    }
  }
```
#
   * <a name="ulprops">**UL** (Unordered List)</a>
      * ListItems[*string*] __*__
      * ClassName (*string*)
      
Unordered List JSON example:
```json
{
  "Type": "ul",
  "Properties": {
    "ClassName":  "indented-list",
    "ListItems": [
      "List Item 1",
      "List Item 2",
      "List Item 3"
    ]
  }
}
```
#      
   * <a name="olprops">**OL** (Ordered List)</a>
      * ListItems[*string*] __*__
      * ClassName (*string*)
 
Ordered List JSON example: 
```json
{
  "Type": "ol",
  "Properties": {
    "ListItems": [
      "Ordered List Item 1",
      "Ordered List Item 2",
      "Ordered List Item 3"
    ]
  }
}
```
#
   * <a name="imgprops">**Img** (Image)</a>
      * AltText (*string*) __*__
      * Source (*string*) __*__
      * ClassName (*string*)
      
Image JSON example:
```json
  {
    "Type": "img",
    "Properties": {
       "Source": "http://url.com/image",
       "AltText": "alt image text",
       "ClassName": "image-class"
    }
  }
```
#
  * <a name="DateInputprops">**DateInput**</a>
    * Label (*string*) __*__
    * QuestionId (*string*) __*__
    * Hint (*string*)
    * RestrictFutureDate (*boolean*) (Defaults to false. If true, it will prevent users entering a date in the future)
    * RestrictPastDate (*boolean*) (Defaults to false. If true, it will prevent users entering a date in the past)
    * RestrictCurrentDate (*boolean*) (Defaults to false. If true, it will prevent users entering today's date)
    * CustomValidationMessage (*string*) (Set a custom validation message for when a user doesn't input a date)
    * ValidationMessageInvalidDate (*string*) (Set a custom validation message for when a user enters an invalid date)
    * ValidationMessageRestrictFutureDate (*string*) (Set a custom validation message for when a user enters a date in the future)
    * ValidationMessageRestrictPastDate (*string*) (Set a custom validation message for when a user enters a date in the past)
    * ValidationMessageRestrictCurrentDate (*string*) (Set a custom validation message for when a user enters today's date)
    * UpperLimitValidationMessage (*string*) (Set a custom validation message for when a user enters the year greater than 100 years           from today)
    * TargetMapping (*string*)  

Date Input example:
```json
    {
        "Type": "DateInput",
        "Properties": {
            "QuestionId": "passportIssued",
            "Label": "When was your passport issued?",
            "Hint": "For example, 12 11 2007",
            "RestrictFutureDate": true,
            "RestrictCurrentDate": true,
            "CustomValidationMessage": "A date is required, please enter a date",
            "ValidationMessageRestrictFutureDate": "Date in the future not allowed, please enter a date in the past",
            "ValidationMessageRestrictCurrentDate": "Today's date not allowed, please enter a date in the past",
            "UpperLimitValidationMessage": "Year upper limit set to 2120"
        }
    }
```
#
  * <a name="DatePicker">**DatePicker**</a>
    * Label (*string*) __*__
    * QuestionId (*string*) __*__
    * Hint (*string*)
    * RestrictFutureDate (*boolean*) (Defaults to false. If true, it will prevent users entering a date in the future)
    * RestrictPastDate (*boolean*) (Defaults to false. If true, it will prevent users entering a date in the past)
    * RestrictCurrentDate (*boolean*) (Defaults to false. If true, it will prevent users entering today's date)
    * Max (*integer*) (The maximum year the user can enter, e.g. if max is set 2500, the maximum date a user can enter is today's date  in the year 2500
    * CustomValidationMessage (*string*) (Set a custom validation message for when a user doesn't input a date)
    * UpperLimitValidationMessage (*string*) (Set a custom validation message for when a user enters a date greater than today's date 100 years from now OR if a max value has been specified, the validation message when a user enters a date greater than today's date in the year stated by the max value)
    * TargetMapping (*string*)  

Date Picker example:
```json
    {
          "Type": "DatePicker",
          "Properties": {
            "QuestionId": "passportIssued1",
            "Label": "When was your passport issued?",
            "Hint": "For example, 12 11 2007",
            "RestrictFutureDate": false,
            "RestrictPastDate": true,
            "RestrictCurrentDate": false,
            "CustomValidationMessage": "This field is required"
          }
    }
```

#
  * <a name="Address">**Address**</a>
    * QuestionId (*string*) __*__
    * AddressProvider (*string*) __*__
    * PostcodeLabel (*string*) __*__
    * AddressLabel (*string*) __*__
    * AddressManualLabel (*string*) __*__
    * Hint (*string*) (hint which appears above the postcode input)
    * SelectHint (*string*) (hint which appears above the select input)
    * MaxLength (*string*)
    * Optional (*boolean*)
    * CustomValidationMessage (*string*) (Set a custom validation message for when user does not enter a postode) There is automatic validation there is not a valid postcode.
    * SelectCustomValidationMessage (*string*) (Set a custom validation message for when user does not select an address from the             dropdown)
    * TargetMapping (*string*)

Address example:
```json
    {
        "Type": "Address",
            "Properties": {
                "QuestionId": "customers-address",
                "AddressProvider": "Fake",
                "PostcodeLabel": "Postcode",
                "AddressLabel": "Address",
                "Hint": "This is an additional hint",
                "SelectHint": "Select the address below",
                "MaxLength": "10",
                "Optional": false,
                "CustomValidationMessage": "This is postcode custom validation message",
                "SelectCustomValidationMessage": "This is select address custom validation message"
            }
    }
```
#
  * <a name="Street">**Street**</a>
    * QuestionId (*string*) __*__
    * StreetProvider (*string*) __*__ (Fake or CRM)
    * StreetLabel (*string*) (Default: Search for a street)
    * Hint (*string*) (Hint message when a user is searching for a street)
    * SelectLabel (*string*) (Default: Street)
    * MaxLength (*string*) (Default: 200)
    * Optional (*boolean*) (Default: false)
    * TargetMapping (*string*)  

Street example:
```json
    {
          "Type": "Street",
          "Properties": {
            "QuestionId": "street-address",
            "StreetLabel": "Search for a street by name",
            "Hint": "e.g. 'Hibbert' or 'Hibbert Lane'",
            "SelectLabel": "Street",
            "StreetProvider": "CRM",
            "MaxLength": "20"
          }
    }
```


#
  * <a name="Organisation">**Organisation**</a>
    * QuestionId (*string*) __*__
    * OrganisationProvider (*string*) __*__ (Fake or CRM)
    * Label (*string*) (Default: Search for a organisation)
    * SelectLabel (*string*) (Default: Organisation)
    * Hint (*string*) (Hint message when a user is searching for a organisation)
    * SelectHint (*string*) (Hint message when a user is selecting an organisation from the dropdown)
    * MaxLength (*string*) (Default: 200)
    * Optional (*boolean*) (Default: false)
    * TargetMapping (*string*)  

Street example:
```json
    {
          "Type": "Organisation",
          "Properties": {
            "QuestionId": "organisation",
            "OrganisationProvider": "Fake",
            "Label": "Enter the organisation",
            "SelectLabel": "Selecte the organisation",
            "Hint": "This is an additional hint",
            "SelectHint": "Select the organisation below",
            "MaxLength": 60
          }
    }
```

#
* <a name="Time">**Time**</a>
  * QuestionId (*string*) __*__
  * Label (*string*) __*__
  * Hint (*string*) 
  * ValidationMessageInvalidTime (*string*) 
  * CustomValidationMessage (*string*)
  * CustomValidationMessageAmPm (*string*)
  * Optional (*boolean*) defaults to false
  * TargetMapping (*string*)  
  

Time example

```json
{
          "Type": "TimeInput",
          "Properties": {
            "QuestionId": "timeid",
            "Label": "What time did it happen?",
            "Hint": "For example, 10:30 AM",
            "CustomValidationMessage": "This field is required",
            "ValidationMessageInvalidTime": "Enter a valid time",
            "CustomValidationMessageAmPm": "You must choose AM or PM"
          }

```

#
* <a name="FileUpload">**FileUpload**</a>
  * QuestionId (*string*) __*__
  * Label (*string*) __*__
  * TargetMapping (*string*)
  * AllowedFileTypes (*Array[string]*) (Default: .png", .jpg", .jpeg, .pdf, .docx, .doc, .odt)
  * MaxFileSize (*string*) in Mb
  

FileUpload example

```json
        {
          "Type": "FileUpload",
          "Properties": {
            "QuestionId": "fileUpload",
            "Label": "Upload",
            "TargetMapping": "customer.file",
            "AllowedFileTypes": [ ".jpg", ".png" ],
            "MaxFileSize": "50"
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

**Conditions**[*object*]
* QuestionID (*string*) - The name of the Radio/Checkbox list to evaluate
* EqualTo (*string*) - The value it must equal to for the behaviour to happen
* CheckboxContains (*string*) - The value will be checked against the list supplied by the checkbox. If it is found it will go to the behaviour 

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
