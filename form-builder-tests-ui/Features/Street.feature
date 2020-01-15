@street
Feature: Street
	In order to collect street information I have to navigate to Page1

Scenario: Street lookup standard use
	Given I navigate to "/street/page1"
	Then I should see the header
	And I should see the "customers-street-street" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see a ".back-button" html element
	Then I should see the "customers-street-streetaddress" input
	Then I should see "3 streets found" is selected in "customers-street-streetaddress" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element
	Then I should see a "strong" element with "Green" text
	Then I select "Green lane" in "customers-street-streetaddress" dropdown
	Then I should see "Green lane" is selected in "customers-street-streetaddress" dropdown with the value "123456789012|Green lane"
	When I click the "nextStep" button
	Then I click the "submit" button
    Then I should see a "h1" element with "Submit" text