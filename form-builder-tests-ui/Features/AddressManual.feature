@addressManual
Feature: AddressManual
	In order to collect address information I have to navigate to Page1

Scenario: Address Manual element standard use
	Given I navigate to "/address/page1"
	Then I should see the header
	And I should see the "customers-address-postcode" input
	And I should see the "manual" link
	And I should see the "nextStep" button
	Then  I click the manual link
	Then I should see the "customers-address-AddressManualAddressLine1" input
	Then I should see the "customers-address-AddressManualAddressLine2" input
	Then I should see the "customers-address-AddressManualAddressTown" input
	Then I should see the "customers-address-AddressManualAddressPostcode" input
	When I click the "nextStep" button
	Then I should see a "p" element with "Please enter Address Line 1" text
	And I should see a "p" element with "Please enter Town" text
	And I should see a "p" element with "Please enter a Postcode" text
	Then I fill in address line one
	And I fill in town
	When I click the "nextStep" button
	Then I should see a "p" element with "Please enter a Postcode" text
	Then I fill in invalid postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a "p" element with "Please enter a valid Postcode" text
	And I should see that "customers-address-AddressManualAddressLine1" input has value "test"
	And I should see that "customers-address-AddressManualAddressTown" input has value "town"
	Then I fill in postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I click the "nextStep" button
	Then I should see a "th" element with "customers-address-AddressManualAddressLine1" text
	And I should see a "th" element with "customers-address-AddressManualAddressTown" text
	And I should see a "th" element with "customers-address-AddressManualAddressPostcode" text
