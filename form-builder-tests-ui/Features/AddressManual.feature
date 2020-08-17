﻿@addressManual
Feature: AddressManual
	In order to collect address information I have to navigate to Page1

Scenario: Address Manual element standard use
	Given I navigate to "/address/page1/manual"
	Then I should see the header
	Then I should see the "customersaddress-AddressManualAddressLine1" input
	Then I should see the "customersaddress-AddressManualAddressLine2" input
	Then I should see the "customersaddress-AddressManualAddressTown" input
	Then I should see the "customersaddress-AddressManualAddressPostcode" input
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
	And I should see that "customersaddress-AddressManualAddressLine1" input has value "test"
	And I should see that "customersaddress-AddressManualAddressTown" input has value "town"
	Then I fill in postcode
	Then I fill in postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I click the "nextStep" button
	Then I should see a "th" element with "customersaddress-AddressManualAddressLine1" text
	And I should see a "th" element with "customersaddress-AddressManualAddressTown" text
	And I should see a "th" element with "customersaddress-AddressManualAddressPostcode" text
