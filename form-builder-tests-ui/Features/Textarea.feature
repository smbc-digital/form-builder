@textarea
Feature: Textarea
	In order to fill in my details I have to navigate to Page1

Scenario: Textarea standard use
	Given I navigate to "/textarea/page1"
	Then I should see the header
	And I should see the "issueOne" input
	And I should see a "span" element with "Please provide as much information about the issue you are reporting as possible" text
	And I should see a "span" element with "You have 2000 characters remaining" text
	And I should see the "nextStep" button
	Then I click the "nextStep" button
	Then I should see a validation error with an id "issueOne-error" with "Check your answer and try again" text
	Then I enter "test data" in "issueOne"
	Then I click the "nextStep" button
	Then I should see the "issueTwo" input
	And I should see the "nextStep" button
	Then I click the "nextStep" button
	Then I should see the "submit" button