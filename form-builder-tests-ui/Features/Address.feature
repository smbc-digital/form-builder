@address
Feature: Address
	In order to collect address information I have to navigate to Page1

Scenario: Address element standard use
	Given I navigate to "/ui-address/page1"
	Then I should see the header
	And I should see the "address-postcode" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I enter "sk11aa" in "address-postcode"
	When I click the "nextStep" button
	Then I should see a ".govuk-back-link" html element
	Then I should see the "address-address" input
	Then I should see "3 addresses found" is selected in "address-address" dropdown with the value ""
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element
	Then I select "address 2" in "address-address" dropdown
	Then I should see "address 2" is selected in "address-address" dropdown with the value "098765432109|address 2"

Scenario: Address element optional address selected
	Given I navigate to "/ui-address/page1"
	Then I enter "sk11aa" in "address-postcode"
	When I click the "nextStep" button
	Then I should see the "address-address" input
	Then I select "address 2" in "address-address" dropdown
	And I click the "nextStep" button
	And I wait one second
	Then I enter "sk11aa" in "addressopt-postcode"
	And I click the "nextStep" button
	And I wait one second
	And I select "address 2" in "addressopt-address" dropdown
	Then I click the "nextStep" button
	And I wait one second
	Then I should see a "dt" element with "address-address" text
	And I should see a "dt" element with "address-address-description" text
	And I should see a "dt" element with "addressopt-address" text
	And I should see a "dt" element with "addressopt-address-description" text