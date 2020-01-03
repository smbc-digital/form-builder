@requiredif
Feature: RequiredIf
	Checking the path looks at both questions for next page

Scenario: User enters cat and yes on page1
	Given I navigate to "/requiredif/first-question"
	When I click the "firstQuestion-0" radiobutton
	Then The "firstQuestion-0" radiobutton should be checked
	
	
