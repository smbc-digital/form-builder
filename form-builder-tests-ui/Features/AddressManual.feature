@addressManual
Feature: AddressManual
	In order to collect a user address via manual entry

Scenario: Address Manual element standard use
	Given I navigate to "/ui-address/page1/"
	Then I enter "sk11zz" in "address-postcode"
	When I click the "nextStep" button
	Then I should see the "address-AddressLine1" input
	Then I should see the "address-AddressLine2" input
	Then I should see the "address-AddressTown" input
	Then I should see the "address-ManualPostcode" input
	Then I should see a "label" element with "Address line 1" text
	Then I should see a "label" element with "Town or city" text
	Then I should see a "label" element with "Postcode" text
	When I click the "nextStep" button
	Then I should see a validation error with an id "address-AddressLine1-error" with "Please enter Address Line 1" text
	Then I should see a validation error with an id "address-AddressTown-error" with "Please enter Town" text
	Then I enter "testline1" in "address-AddressLine1"
	Then I enter "town" in "address-AddressTown"
	Then I enter "INVALID POSTCODE" in "address-ManualPostcode"
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a validation error with an id "address-ManualPostcode-error" with "Please enter a valid Postcode" text
	And I should see that "address-AddressLine1" input has value "testline1"
	And I should see that "address-AddressTown" input has value "town"