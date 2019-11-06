@radiobutton
Feature: RadioButton
	In order to enter options I need to add option buttons


Scenario: User enters nothing on page1
	Given I navigate to "/RadioButton/page1"
	When I click the "submit" button
	Then I should see a validation message for "radButton-error" input
	And I should not see a validation message for "radButtonOpt-error" input

Scenario: User enters yes on page1
	Given I navigate to "/RadioButton/page1"
	When I click the "radButton-0" radiobutton
	Then The "radButton-0" radiobutton should be checked
	
Scenario: User enters no on page1
	Given I navigate to "/RadioButton/page1"
	When I click the "radButton-1" radiobutton
	Then The "radButton-1" radiobutton should be checked

Scenario: User selects more than one radio input
	Given I navigate to "/RadioButton/page1"
	When I click the "radButton-0" radiobutton
	Then I click the "radButton-1" radiobutton
	Then The "radButton-1" radiobutton should be checked
	Then The "radButton-0" radiobutton should be unchecked