@postcode
Feature: Postcode
	In order to fill in my details I have to navigate to Page1

Scenario: Renders HTML tags on the page
	Given I navigate to "/postcode/page1"
	Then I should see the header
	And I should see the "postcode" input
	And I should see the "nextStep" button
	Then I click the "nextStep" button
	And I should see a "p" element with "Check the postcode and try again" text
	Then I fill in page1 with bad postcode
	Then I click the "nextStep" button
	And I should see a "p" element with "Postcode must be a valid postcode" text
	Then I fill in page1 with good postcode
	Then I click the "nextStep" button
	And I should see a "h2" element with "You are on the second page" text