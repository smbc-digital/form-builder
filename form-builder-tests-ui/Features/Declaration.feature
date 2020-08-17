@declaration
Feature: Declaration
	In order to fill in my details I have to navigate to Page1

Scenario: User uses a declaration page.
	Given I navigate to "/ui-declaration/page1"
	When I click the "Declaration" checkbox
	Then I click the "nextStep" button
	Then I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	When I click the "DeclarationWithHiddenTitle" checkbox
	Then The "DeclarationWithHiddenTitle" checkbox should be checked
	
Scenario: User uses a page next step and back.
	Given I navigate to "/ui-declaration/page1"
	When I click the "Declaration" checkbox
	Then I click the "nextStep" button
	Then The the previous link is clicked
	Then The "Declaration" checkbox should be checked