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
                    "ID": "first-name"
                }
            }
        ]
    }

* **Type** (*string*) (HTML element)
    * [H1-H6](#headingprops) (Heading levels)
    * [P](#ptextprops) (Paragraph text)
    * [Textbox](#textboxprops)
    * [Textarea](#textareaprops) (Large text box)
    * [Radio](#radioprops)
    * [Select](#selectprops)
    * [Checkbox](#checkboxprops)
    * [Link](#linkprops) (Anchor styled as button)
    * [Alert](#alertprops)
    * [Button](#buttonprops)
    * [Ul (Unordered List)](#Ulprops)
    * [Ol (Ordered List)](#Olprops)
    


* **Properties** (*object*) (Prop types of an element - * = Mandatory)
    * <a name="headingprops">**H1-H6** (Heading levels)</a>
        * Text (*string*) __*__

    * <a name="ptextprops">**P** (Paragraph text)</a>
        * Text (*string*) __*__

    * <a name="textboxprops">**Textbox**</a>
        * Label (*string*) __*__
        * Name (*string*) __*__
        * ID (*string*) __*__
        * AriaLabel (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*)
        * Optional (*boolean*)

    * <a name="textareaprops">**Textarea**</a>
        * Label (*string*) __*__
        * Name (*string*) __*__
        * ID (*string*) __*__
        * AriaLabel (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * MaxLength (*int*)
        * Optional (*boolean*)

    * <a name="radioprops">**Radio**</a>
        * Label (*string*) __*__
        * Name (*string*) __*__
        * ID (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)

    * <a name="selectprops">**Select**</a>
        * Label (*string*) __*__
        * Name (*string*) __*__
        * ID (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)
        * Placeholder (*string*)

    * <a name="checkboxprops">**Checkbox**</a>
        * Label (*string*) __*__
        * Name (*string*) __*__
        * ID (*string*) __*__
        * Options[*object*] __*__
            * Text (*string*) __*__
            * Value (*string*) __*__
            * Hint (*string*)
        * CustomValidationMessage (*string*)
        * Hint (*string*)
        * Optional (*boolean*)

    * <a name="linkprops">**Link**</a>
        * Text (*string*) __*__
        * Name (*string*) __*__
        * URL (*string*) __*__
        * ClassName (*string*)
        * ID (*string*)

    * <a name="alertprops">**Alert**</a>
        * Text (*string*) __*__
        * Type (*string*) __*__
            * Info
            * Warning
        * ClassName (*string*)

    * <a name="buttonprops">**Button**</a>
        * Text (*string*) __*__
        * Type (*string*) __*__
        * ClassName (*string*)
        * ID (*string*)
   
    * <a name="ulprops">**Ul**</a>
      * ListItems (*[string]*) __*__
      * ClassName (*string*)
      
    * <a name="olprops">**Ol**</a>
      * ListItems (*[string]*) __*__
      * ClassName (*string*)

## <a name="pagebehaviours">PageBehaviours[*object*]</a>
Example where if a user selects yes they will continue on with the form, otherwise they will submit their answer:

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
