@breadcrumbs
Feature: Breadcrumbs
	In order to fill in my details I have to navigate to Page0

Scenario: Textbox standard use
	Given I navigate to "/ui-textbox/page0"
	Then I should see the header
	And I should find an element with class "govuk-breadcrumbs"
	And I should see the "firstQuestion" input
	Then I enter "test" in "firstQuestion"
	Then I click the "nextStep" button
	Then I should see the header
	And I should not find an element with class "govuk-breadcrumbs"
	And I should see the "firstName" input
	And I should see the "middleName" input
	And I should see a "span" element with "(optional)" text
	