@addressManual
Feature: AddressManual
	In order to collect address information I have to navigate to Page1

Scenario: Address Manual element standard use
	Given I navigate to "/address/page1/manual"
	Then I should see the header
	Then I should see the "customersaddress-AddressLine1" input
	Then I should see the "customersaddress-AddressLine2" input
	Then I should see the "customersaddress-AddressTown" input
	Then I should see the "customersaddress-ManualPostcode" input
	When I click the "nextStep" button
	Then I should see a validation error with an id "customersaddress-AddressLine1" with "Please enter Address Line 1" text
	Then I should see a validation error with an id "customersaddress-AddressTown" with "Please enter Town" text
	Then I should see a validation error with an id "customersaddress-ManualPostcode" with "Please enter a Postcode" text
	Then I fill in address line one
	And I fill in town
	When I click the "nextStep" button
	Then I should see a validation error with an id "customersaddress-ManualPostcode" with "Please enter a Postcode" text
	Then I fill in invalid postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a validation error with an id "customersaddress-ManualPostcode" with "Please enter a valid Postcode" text
	And I should see that "customersaddress-AddressLine1" input has value "test"
	And I should see that "customersaddress-AddressTown" input has value "town"
	Then I fill in postcode
	Then I fill in postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I click the "nextStep" button
	Then I should see a "dt" element with "customersaddress-AddressLine1" text
	And I should see a "dt" element with "customersaddress-AddressTown" text
	And I should see a "dt" element with "customersaddress-ManualPostcode" text
