@semicomplexpath
Feature: SemiComplexPath
	Checking the path looks at both questions for next page

Scenario: User enters cat and no on page1
	Given I navigate to "/semicomplexpath/first-question"
	When I click the "firstQuestion-0" radiobutton
	Then The "firstQuestion-0" radiobutton should be checked
	When I click the "secondQuestion-1" radiobutton
	Then The "secondQuestion-1" radiobutton should be checked
	Then I click the "nextStep" button
	And I should see the header
	And I should see a "h2" element with "You answered Cat No" text

Scenario: User enters dog and no on page1
	Given I navigate to "/semicomplexpath/first-question"
	When I click the "firstQuestion-1" radiobutton
	When I click the "secondQuestion-1" radiobutton
	Then I click the "nextStep" button
	And I should see a "h2" element with "You answered Dog No" text
