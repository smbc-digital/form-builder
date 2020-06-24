@checkboxlist
Feature: Checkbox
	In order to fill in my details I have to navigate to Page1

Scenario: User uses a single checkbox on a page
	Given I navigate to "/ui-checkbox/your-hobbies"
	When I click the "continue" button
	Then I should see a validation message for "whatHobbies-error" input
	When I click the "whatHobbies-0" checkbox
	Then The "whatHobbies-0" checkbox should be checked
	When I click the "whatHobbies-1" checkbox
	Then The "whatHobbies-1" checkbox should be checked
	
Scenario: User uses a checkbox page next step and back.
	Given I navigate to "/ui-checkbox/your-hobbies"
	When I click the "whatHobbies-0" checkbox
	When I click the "whatHobbies-1" checkbox
	Then I click the "continue" button
	Then The previous link is clicked
	Then The "whatHobbies-0" checkbox should be checked
	Then The "whatHobbies-1" checkbox should be checked
	When I click the "whatHobbies-0" checkbox
	Then The "whatHobbies-0" checkbox should be unchecked
