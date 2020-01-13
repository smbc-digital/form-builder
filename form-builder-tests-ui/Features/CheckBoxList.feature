@checkboxlist
Feature: Checkbox
	In order to fill in my details I have to navigate to Page1

Scenario: User enters nothing on page1
	Given I navigate to "/checkbox/page1"
	When I click the "nextStep" button
	Then I should see a validation message for "CheckBoxList-error" input

Scenario: User enters Apples on page1
	Given I navigate to "/checkbox/page1"
	When I click the "CheckBoxList-0" checkbox
	Then The "CheckBoxList-0" checkbox should be checked
	
Scenario: User enters Oranges on page1
	Given I navigate to "/checkbox/page1"
	When I click the "CheckBoxList-1" checkbox
	Then The "CheckBoxList-1" checkbox should be checked

Scenario: User selects more than one checkbox input
	Given I navigate to "/checkbox/page1"
	When I click the "CheckBoxList-0" checkbox
	Then I click the "CheckBoxList-1" checkbox
	Then The "CheckBoxList-1" checkbox should be checked
	Then The "CheckBoxList-0" checkbox should be checked

Scenario: User selects declaration but does not select mandatory fruit
	Given I navigate to "/checkbox/page1"
	When  I click the "Declaration-0" checkbox
	Then I click the "nextStep" button
	Then I should see a validation message for "CheckBoxList-error" input

Scenario: User enters Apples on page1 and goes forward and then back
	Given I navigate to "/checkbox/page1"
	When I click the "CheckBoxList-0" checkbox
	Then The "CheckBoxList-0" checkbox should be checked
	When I click the "Declaration-0" checkbox
	Then The "Declaration-0" checkbox should be checked
	Then I click the "nextStep" button
	Then The the previous link is clicked
	When I click the "CheckBoxList-0" checkbox
	When I click the "Declaration-0" checkbox
	Then I click the "nextStep" button
	Then I should see a validation message for "CheckBoxList-error" input
	Then The "CheckBoxList-0" checkbox should be unchecked
	Then The "Declaration-0" checkbox should be unchecked
