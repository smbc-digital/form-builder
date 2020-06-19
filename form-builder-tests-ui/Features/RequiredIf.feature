@requiredif
Feature: RequiredIf
	Checking the path looks at both questions for next page

Scenario: Validation user enters cat and yes on page1
	Given I navigate to "/ui-required-if/first-question"
	When I click the "firstQuestion-0" radiobutton
	Then The "firstQuestion-0" radiobutton should be checked
	Then I click the "nextStep" button
	Then I sleep "1000"
	Then I should see a validation message for "secondQuestion-error" input
	When I click the "secondQuestion-0" radiobutton
	Then I sleep "1000"
	Then The "secondQuestion-0" radiobutton should be checked
	Then I click the "nextStep" button
	Then I sleep "1000"
	And I should see a "h2" element with "This is a label Required If" text

Scenario: User enters no for first question on page1
	Given I navigate to "/ui-required-if/first-question"
	When I click the "firstQuestion-1" radiobutton
	Then The "firstQuestion-1" radiobutton should be checked
	Then I click the "nextStep" button
	Then I sleep "1000"
	Then I should see a validation message for "thirdQuestion-error" input
	Then I fill in page1
	Then I click the "nextStep" button
	Then I sleep "1000"
	And I should see a "h2" element with "This is a label Required If" text
	
	