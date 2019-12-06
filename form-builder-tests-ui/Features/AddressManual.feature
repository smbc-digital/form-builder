@addressManual
Feature: AddressManual
	In order to collect address information I have to navigate to Page1


Scenario: Renders HTML tags on the page
	Given I navigate to "/address/page1"
	Then I should see the header
	And I should see the "customers-address-postcode" input
	And I should see the "manual" link
	And I should see the "nextStep" button


Scenario: Renders HTML tags on the manual page
	Given I navigate to "/address/page1"
	Then  I click the manual link
	Then I should see the "customers-address-AddressManualAddressLine1" input
	Then I should see the "customers-address-AddressManualAddressLine2" input
	Then I should see the "customers-address-AddressManualAddressTown" input
	Then I should see the "customers-address-AddressManualAddressPostcode" input


Scenario: Shows Error Messages on the manual page when I dont enter anything
	Given I navigate to "/address/page1"
	Then  I click the manual link
	When I click the "nextStep" button
	Then I should see a "p" element with "Please enter Address Line 1" text
	And I should see a "p" element with "Please enter Town" text
	And I should see a "p" element with "Please enter a Postcode" text

Scenario: Shows Error Message when Postcode is not entered
	Given I navigate to "/address/page1"
	Then  I click the manual link
	Then I fill in address line one
	And I fill in town
	When I click the "nextStep" button
	Then I should see a "p" element with "Please enter a Postcode" text

Scenario: Shows Error Message when invalid Postcode is entered
	Given I navigate to "/address/page1"
	Then  I click the manual link
	Then I wait one second
	Then I fill in address line one
	Then I fill in town
	Then I fill in invalid postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a "p" element with "Please enter a valid Postcode" text
	And I should see that "customers-address-AddressManualAddressLine1" input has value "test"
	And I should see that "customers-address-AddressManualAddressTown" input has value "town"

Scenario: Submits the page when everything is filled in
	Given I navigate to "/address/page1"
	When  I click the manual link
	Then I wait one second
	Then I fill in address line one
	Then I fill in town
	Then I fill in postcode
	When I click the "nextStep" button
	Then I wait one second
	Then I should see a "th" element with "customers-address-AddressManualAddressLine1" text
	And I should see a "th" element with "customers-address-AddressManualAddressTown" text
	And I should see a "th" element with "customers-address-AddressManualAddressPostcode" text
