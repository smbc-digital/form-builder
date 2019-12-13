@address
Feature: Address
	In order to collect address information I have to navigate to Page1

Scenario: Renders HTML tags on the page
	Given I navigate to "/address/page1"
	Then I should see the header
	And I should see the "customers-address-postcode" input
	And I should see the "nextStep" button

Scenario: Triggers empty validation when postcode empty
	Given I navigate to "/address/page1"
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element

Scenario: I navigate to the selection page when postcode entered
	Given I navigate to "/address/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see a ".back-button" html element
	Then I should see the "customers-address-address" input
	Then I should see "3 addresses found" is selected in "customers-address-address" dropdown with the value ""

Scenario: Trigger select validation on no choice made in dropdown
	Given I navigate to "/address/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "customers-address-address" input
	Then I should see "3 addresses found" is selected in "customers-address-address" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element

Scenario: Selecting an address in the dropdown should display the success page
	Given I navigate to "/address/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "customers-address-address" input
	Then I select "address 2" in "customers-address-address" dropdown
	Then I should see "address 2" is selected in "customers-address-address" dropdown with the value "098765432109|address 2"

Scenario: I enter postcode in optional and I don't select an address in the dropdown, should display the success page
	Given I navigate to "/address/page1"
	Then  I fill in page1
	When I click the "nextStep" button
	Then I should see the "customers-address-address" input
	Then I select "address 2" in "customers-address-address" dropdown
	And I click the "nextStep" button
	Then I fill in page2
	And I click the "nextStep" button
	And I wait one second
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "th" element with "customers-address-address" text
	And I should see a "th" element with "customers-address-address-description" text
	And I should see a "th" element with "optional-address-address" text

Scenario: I enter postcode in optional and select an address in the dropdown should display the success page
	Given I navigate to "/address/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "customers-address-address" input
	Then I select "address 2" in "customers-address-address" dropdown
	And I click the "nextStep" button
	Then I fill in page2
	And I click the "nextStep" button
	And I wait one second
	And I select "address 2" in "optional-address-address" dropdown
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "th" element with "customers-address-address" text
	And I should see a "th" element with "customers-address-address-description" text
	And I should see a "th" element with "optional-address-address" text
	And I should see a "th" element with "optional-address-address-description" text