@checkboxlist
Feature: Checkbox
	In order to fill in my details I have to navigate to Page1

Scenario: User uses a checkbox page.
	Given I navigate to "/checkbox/page1"
	When I click the "nextStep" button
	Then I should see a validation message for "CheckBoxList-error" input
	When I click the "Declaration-0" checkbox
	Then I click the "nextStep" button
	Then I should see a validation message for "CheckBoxList-error" input
	When I click the "CheckBoxList-0" checkbox
	Then The "CheckBoxList-0" checkbox should be checked
	When I click the "CheckBoxList-1" checkbox
	Then The "CheckBoxList-1" checkbox should be checked
	
Scenario: User uses a checkbox page next step and back.
	Given I navigate to "/checkbox/page1"
	When I click the "CheckBoxList-0" checkbox
	When I click the "CheckBoxList-1" checkbox
	When I click the "Declaration-0" checkbox
	Then I click the "nextStep" button
	Then The the previous link is clicked
	Then The "CheckBoxList-0" checkbox should be checked
	Then The "CheckBoxList-1" checkbox should be checked
	Then The "Declaration-0" checkbox should be checked
	When I click the "CheckBoxList-0" checkbox
	Then The "CheckBoxList-0" checkbox should be unchecked
