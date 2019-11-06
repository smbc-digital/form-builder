@textarea
Feature: Textarea
	In order to fill in my details I have to navigate to Page1

Scenario: Renders HTML tags on the page
	Given I navigate to "/Textarea/page1"
	Then I should see the header
	And I should see the "issueOne" input
	And I should see a "p" element with "(optional)" text
	And I should see a "p" element with "Your description can be up to 2000 characters" text
	And I should see the "nextStep" button

Scenario: User fills in data and clicks next
	Given I navigate to "/Textarea/page1"
	Then I fill in page1
	Then I click the "nextStep" button
	Then I fill in page2
	And I should see the "submit" button

Scenario: User enters nothing on page2
	Given I navigate to "/Textarea/page1"
	Then I fill in page1
	Then I click the "nextStep" button
	When I click the "submit" button
	Then I should see a validation message for "issueTwo" input
