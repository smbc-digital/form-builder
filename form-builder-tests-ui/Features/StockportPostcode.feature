@stockportpostcode
Feature: StockportPostcode
	In order to fill in my details I have to navigate to Page1

Scenario: Postcode validation standard use
	Given I navigate to "/stockportpostcode/page1"
	Then I should see the header
	Then I click the "nextStep" button
	Then I should see a validation error with an id "stockportpostcode-error" with "Check the stockport postcode and try again" text
	Then I fill in page1 with wrong postcode
	Then I click the "nextStep" button
	Then I should see a validation error with an id "stockportpostcode-error" with "Stockport postcode must be a valid postcode" text
	Then I fill in page1 with wrong postcode
	Then I fill in page1 with good postcode
	Then I click the "nextStep" button
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	#And I should see a "h2" element with "You are on the second page" text