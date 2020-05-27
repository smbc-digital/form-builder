@address
Feature: Address
	In order to collect address information I have to navigate to Page1

Scenario: Address element standard use
	Given I navigate to "/address/page1"
	Then I should see the header
	And I should see the "customersaddresswithtitle-postcode" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I fill in page1
	When I click the "nextStep" button
	Then I should see a ".govuk-back-link" html element
	Then I should see the "customersaddresswithtitle-address" input
	Then I should see "3 addresses found" is selected in "customersaddresswithtitle-address" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I select "address 2" in "customersaddresswithtitle-address" dropdown
	Then I should see "address 2" is selected in "customersaddresswithtitle-address" dropdown with the value "098765432109|address 2"

Scenario: Address element optional no address selected
	Given I navigate to "/address/page1"
	Then  I fill in page1
	When I click the "nextStep" button
	Then I should see the "customersaddresswithtitle-address" input
	Then I select "address 2" in "customersaddresswithtitle-address" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I fill in page2
	And I click the "nextStep" button
	And I wait one second
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "dt" element with "customersaddress-address" text
	And I should see a "dt" element with "customersaddress-address-description" text
	And I should see a "dt" element with "optionaladdress-address" text

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
	Then I should see a "dt" element with "customersaddress-address" text
	And I should see a "dt" element with "customersaddress-address-description" text
	And I should see a "dt" element with "optionaladdress-address" text
	And I should see a "dt" element with "optionaladdress-address-description" text

Scenario: Validation message should apear if I Enter Invalid Postcode
	Given I navigate to "/address/page1"
	When I fill in page1 with invalid postcode
	Then I click the "nextStep" button
<<<<<<< HEAD
	Then I should see a validation message for "customersaddresswithtitle-postcode-error" input
=======
	Then I should see a validation message for "customersaddress-postcode-error" input
>>>>>>> design-system
