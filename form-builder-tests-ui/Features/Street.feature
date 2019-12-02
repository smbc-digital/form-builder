@street
Feature: Street
	In order to collect street information I have to navigate to Page1

Scenario: Renders HTML tags on the page
	Given I navigate to "/street/page1"
	Then I should see the header
	And I should see the "street-address-street" input
	And I should see the "nextStep" button

Scenario: Triggers empty validation when street empty
	Given I navigate to "/street/page1"
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element

Scenario: I navigate to the selection page when street entered
	Given I navigate to "/street/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see a ".back-button" html element
	Then I should see the "street-address-street" input
	Then I should see "3 streets found" is selected in "street-address-street" dropdown with the value ""

Scenario: Trigger select validation on no choice made in dropdown
	Given I navigate to "/street/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "street-address-street" input
	Then I should see "3 streets found" is selected in "street-address-street" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element

Scenario: Selecting a street in the dropdown should display the success page
	Given I navigate to "/street/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "street-address-street" input
	Then I select "Green lane" in "street-address-street" dropdown
	Then I should see "Green lane" is selected in "street-address-street" dropdown with the value "123456789012"