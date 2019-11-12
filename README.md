<h1 align="center">Form Builder👷</h1>
<div align="center">:page_facing_up: :pencil2:</div>
<div align="center">
  <sub>Built with ❤︎ by
  <a href="https://www.stockport.gov.uk">Stockport Council</a>
</div>

## Table of Contents
- [Base JSON Structure](##BaseJSONStructure)
- [UI Tests](#UI_Tests)

## Requirments
- dotnet core 2.2
- gpg key added to accepted contributors

## Base JSON Structure
```json
    {
        "FormName": "",
        "BaseURL": "",
        "StartPageSlug": "",
        "FeedbackForm": "https://stockportcouncil.typeform.com/to/yzIJEe",
        "Pages": [
          {
            "PageSlug": "",
            "PageElements": [],
            "PageBehaviours": []
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
* **PageElements**[*object*]- List of HTML elements to display on page
* [PageBehaviours[*object*]](#pagebehaviours) - List of conditionals & page redirects for the page


## Element Types & Properties
**PageElements[*object*]**:

Example JSON:
```json
    {
        "PageElements": [
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
* **Type** (*string*) (HTML element)
    * [H1-H6](#headingprops) (Heading levels)
    * [P](#ptextprops) (Paragraph text)
    * [Textbox](#textboxprops)
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
    


* **Properties** (*object*) (Prop types of an element - * = Mandatory)
#
   * <a name="headingprops">**H1-H6** (Heading levels)</a>
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
      "MaxLength": 60
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

Textarea JSON example:
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
        * Name (*string*) __*__
        * QuestionId (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
 
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
            * Hint (*string*) (ignored)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
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
        * Name (*string*) __*__
        * QuestionId (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
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

Button JSON example:
```json
  {
    "Type": "Button",
    "Properties": {
      "ButtonId": "nextStep",
      "Text": "Custom text",
      "className": "button-secondary"
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

## <a name="pagebehaviours">PageBehaviours[*object*]</a>
Example where if a user selects yes they will continue on with the form, otherwise they will submit their answer:
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
                "BehaviourType": 0,
                "PageSlug": "why-do-you-like-apples"
            },
            {
                "Conditions": [],
                "BehaviourType": 1,
                "PageSlug": "form-submission"
            }
        ]
    }
```
**Conditions**[*object*]
* QuestionID (*string*) - The name of the Radio/Checkbox list to evaluate
* EqualTo (*string*) - The value it must equal to for the behaviour to happen

**BehaviourType** (*enum*) __*__
* 0 = GoToPage
* 1 = SubmitForm
* 2 = GoToExternalPage

**PageSlug** (*string*) __*__ - The PageSlug the form will redirect to

## UI_Tests

The form builder app has UI tests to ensure that the UI is expected. Our UI Tests are written using SpecFlow

### Requirments
- chromdrivers.exe
- make (not required)

### How to run UI tests locally

```console
$ make ui-test
```

without make you can also run the UI Test locally by running the two commands listed below

```console
$ cd ./src && dotnet run
```
```console
$ dotnet test ./form-builder-tests-ui/form-builder-tests-ui.csproj
```

It is possible to change the default browser the UI Tests are run from, to do this you need to modify the BrowserConfiguration to run with firefox or another browser of your choice.
