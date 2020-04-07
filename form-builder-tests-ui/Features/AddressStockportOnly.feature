Feature: AddressStockportOnly
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@addressstockport
Scenario: Validation message should apear if I Enter Invalid Postcode
	Given I navigate to "/address-stockport/page1"
	When I fill in page1 with invalid postcode
	Then I click the "nextStep" button
	Then I should see a validation message for "customersaddress-postcode-error" input

