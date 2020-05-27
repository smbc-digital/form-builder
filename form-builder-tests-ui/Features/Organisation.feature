@organisation
Feature: Organisation
	To allow the collection of an organisation information

Scenario: Organisation standard use
	Given I navigate to "/organisation/page-one"
	Then I should see the header
	And I should see the "organisation" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see "3 organisations found" is selected in "organisation-organisation" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I select "Organisation 2" in "organisation-organisation" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page2
	Then I click the "nextStep" button
	Then I wait one second
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "dt" element with "organisation" text
	And I should see a "dt" element with "optorganisation" text

Scenario: I enter organisation in optional and select an organisation in the dropdown should display the success page
	Given I navigate to "/organisation/page-one"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "organisation" input
	Then I select "Organisation 2" in "organisation-organisation" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page2
	Then I click the "nextStep" button
	And I wait one second
	And I select "Organisation 3" in "optorganisation-organisation" dropdown
	And I click the "nextStep" button
	Then I wait one second
	Then I should see a "dt" element with "organisation-organisation" text
	And I should see a "dt" element with "organisation" text
	And I should see a "dt" element with "optorganisation-organisation" text
	And I should see a "dt" element with "optorganisation" text