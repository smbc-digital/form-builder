@textbox
Feature: Textbox
	In order to fill in my details I have to navigate to Page1

Scenario: Textbox standard use
	Given I navigate to "/textbox/page2"
	Then I should see the header
	And I should see the "firstName" input
	And I should see the "middleName" input
	And I should see a "p" element with "(optional)" text
	And I should see the "lastName" input
	And I should see the "nextStep" button
	When I click the "nextStep" button
	Then I should see a validation message for "firstName" input
	Then I should see a validation message for "lastName" input
	Then I fill in page1
	Then I click the "nextStep" button
	Then I should see the "emailAddress" input
	And I should see a "p" element with "ie: someone@example.com" text
	And I should see the "phoneNumber" input
	And I should see a "p" element with "ie: 01615347890" text