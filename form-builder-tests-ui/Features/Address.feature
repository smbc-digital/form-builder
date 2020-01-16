@address
Feature: Address
	In order to collect address information I have to navigate to Page1

Scenario: Address element standard use
	Given I navigate to "/address/page1"
	Then I should see the header
	And I should see the "customersaddress-postcode" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see a ".back-button" html element
	Then I should see the "customersaddress-address" input
	Then I should see "3 addresses found" is selected in "customersaddress-address" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".input-error-content" html element
	Then I select "address 2" in "customersaddress-address" dropdown
	Then I should see "address 2" is selected in "customersaddress-address" dropdown with the value "098765432109|address 2"

Scenario: Address element optional no address selected
	Given I navigate to "/address/page1"
	Then  I fill in page1
	When I click the "nextStep" button
	Then I should see the "customersaddress-address" input
	Then I select "address 2" in "customersaddress-address" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page2
	And I click the "nextStep" button
	And I wait one second
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "th" element with "customersaddress-address" text
	And I should see a "th" element with "customersaddress-address-description" text
	And I should see a "th" element with "optionaladdress-address" text

Scenario: Address element optional address selected
	Given I navigate to "/address/page1"
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see the "customersaddress-address" input
	Then I select "address 2" in "customersaddress-address" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page2
	And I click the "nextStep" button
	And I wait one second
	And I select "address 2" in "optionaladdress-address" dropdown
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "th" element with "customersaddress-address" text
	And I should see a "th" element with "customersaddress-address-description" text
	And I should see a "th" element with "optionaladdress-address" text
	And I should see a "th" element with "optionaladdress-address-description" text