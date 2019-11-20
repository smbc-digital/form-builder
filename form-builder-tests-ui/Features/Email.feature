@email
Feature: Email
	In order to fill in my details I have to navigate to Page1

Scenario: Renders HTML tags on the page
	Given I navigate to "/email/page1"
	Then I should see the header
	And I should see the "name" input
	And I should see the "email" input
	And I should see the "nextStep" button

Scenario: I enter an incorrect email i see the error
	Given I navigate to "/email/page1"
	Then I fill in page1
	Then I click the "nextStep" button
	Then I should see a validation message for "email" input

