@radiobutton
Feature: RadioButton
	In order to enter options I need to add option buttons

Scenario: Radio button standard use
	Given I navigate to "/radiobutton/page1"
	When I click the "submit" button
	Then I should see a validation message for "radButton-error" input
	And I should not see a validation message for "radButtonOpt-error" input
	When I click the "radButton-0" radiobutton
	Then The "radButton-0" radiobutton should be checked
	When I click the "radButton-1" radiobutton
	Then The "radButton-1" radiobutton should be checked
	Then The "radButton-0" radiobutton should be unchecked
	
Scenario: User selects from second list and not the first
	Given I navigate to "/radiobutton/page1"
	When I click the "radButtonOpt-0" radiobutton
	Then The "radButtonOpt-0" radiobutton should be checked
	Then I click the "submit" button
	Then I should see a validation message for "radButton-error" input 
	Then The "radButtonOpt-0" radiobutton should be checked
	
