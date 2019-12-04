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