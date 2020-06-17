@complexpath
Feature: ComplexPath
	Checking the path looks at both questions for next page

Scenario: Complex path different questions on different pages are referred to for behaviour
	Given I navigate to "/ui-complex-path/first-question"
	When I click the "firstQuestion-0" radiobutton
	Then The "firstQuestion-0" radiobutton should be checked
	Then I click the "nextStep" button
	Then I sleep "1000"
	And I should see the header
	Then I click the "nextStep" button
	Then I sleep "1000"
	Then I should see a validation message for "secondQuestion-error" input
	When I click the "secondQuestion-0" radiobutton
	Then The "secondQuestion-0" radiobutton should be checked
	Then I click the "nextStep" button
	Then I sleep "1000"
	Then I should see the header
	And I should see a "h2" element with "This is cat-yes" text

	
