@addressManual
Feature: AddressManual
	In order to collect address information I have to navigate to Page1

Scenario: Address Manual element standard use
	Given I navigate to "/address/page1/manual"
	Then I should see the header
	Then I should see the "customersaddresswithtitle-AddressLine1" input
	Then I should see the "customersaddresswithtitle-AddressLine2" input
	Then I should see the "customersaddresswithtitle-AddressTown" input
	Then I should see the "customersaddresswithtitle-ManualPostcode" input
	When I click the "nextStep" button
	Then I should see a validation error with an id "customersaddresswithtitle-AddressLine1-error" with "Please enter Address Line 1" text
	Then I should see a validation error with an id "customersaddresswithtitle-AddressTown-error" with "Please enter Town" text
	Then I should see a validation error with an id "customersaddresswithtitle-ManualPostcode-error" with "Please enter a Postcode" text
	Then I fill in address line one
	And I fill in town
	And I fill in invalid postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a validation error with an id "customersaddresswithtitle-ManualPostcode-error" with "Please enter a valid Postcode" text
	And I should see that "customersaddresswithtitle-AddressLine1" input has value "test"
	And I should see that "customersaddresswithtitle-AddressTown" input has value "town"
	Then I fill in postcode
	When I click the "nextStep" button
	Then I wait one second
	Then  I fill in page2
	When I click the "nextStep" button
	Then I select "address 2" in "customersaddressnotitle-address" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page3
	And I click the "nextStep" button
	And I wait one second
	And I select "address 2" in "optionaladdress-address" dropdown
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "dt" element with "customersaddresswithtitle-AddressLine1" text
	And I should see a "dt" element with "customersaddresswithtitle-AddressTown" text
	And I should see a "dt" element with "customersaddresswithtitle-ManualPostcode" text
