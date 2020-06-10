@street
Feature: Street
	In order to collect street information I have to navigate to Page1

Scenario: Street lookup standard use
	Given I navigate to "/street/page1"
	Then I should see the header
	And I should see the "street" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I enter "street name" in "street"
	When I click the "nextStep" button
	Then I should see a ".govuk-back-link" html element
	Then I should see the "street-street" input
	Then I should see "3 streets found" is selected in "street-street" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I select "Green lane" in "street-street" dropdown
	Then I should see "Green lane" is selected in "street-street" dropdown with the value "123456789012|Green lane"
	When I click the "nextStep" button