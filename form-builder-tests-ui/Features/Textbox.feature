@textbox
Feature: Textbox
	In order to fill in my details I have to navigate to Page0

Scenario: Textbox standard use
	Given I navigate to "/textbox/page0"
	Then I should see the header
	And I should see the "firstQuestion" input
	Then I enter "test" in "firstQuestion"
	Then I click the "nextStep" button
	Then I should see the header
	And I should see the "firstName" input
	And I should see the "middleName" input
	And I should see a "span" element with "(optional)" text
	And I should see the "lastName" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a validation error with an id "firstName-error" with "Check the first name and try again" text
	Then I should see a validation error with an id "lastName-error" with "Check the last name and try again" text
	Then I enter "test" in "firstName"
	And I enter "test" in "lastName"
	Then I click the "nextStep" button
	Then I should see the "emailAddress" input
	And I should see a "span" element with "ie: someone@example.com" text
	And I should see the "phoneNumber" input
	And I should see a "span" element with "ie: 01615347890" text