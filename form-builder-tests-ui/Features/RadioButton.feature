@radiobutton
Feature: RadioButton
	In order to enter options I need to add option buttons

Scenario: Radio button standard use
	Given I navigate to "/signoffgroup5radio/radio"
	When I click the "continue" button
	Then I should see a validation message for "radButton-error" input
	And I should not see a validation message for "optionalRadio-error" input
	When I click the "radButton-0" radiobutton
	Then The "radButton-0" radiobutton should be checked
	When I click the "radButton-1" radiobutton
	Then The "radButton-1" radiobutton should be checked
	Then The "radButton-0" radiobutton should be unchecked
	
Scenario: User selects from second list and not the first
	Given I navigate to "/signoffgroup5radio/radios"
	When I click the "optionalRadio-0" radiobutton
	Then The "optionalRadio-0" radiobutton should be checked
	Then I click the "continue" button
	Then I should see a validation message for "radButton-error" input 
	Then The "optionalRadio-0" radiobutton should be checked