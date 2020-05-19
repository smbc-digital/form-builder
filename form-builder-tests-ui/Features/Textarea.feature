@textarea
Feature: Textarea
	In order to fill in my details I have to navigate to Page1

Scenario: Textarea standard use
	Given I navigate to "/textarea/page1"
	Then I should see the header
	And I should see the "issueOne" input
	#And I should see a "p" element with "Your description can be up to 2000 characters" text
	And I should see the "nextStep" button
	Then I click the "nextStep" button
	Then I should see a validation message for "issueOne" input
	Then I fill in page1
	Then I click the "nextStep" button
	Then I fill in page2
	And I should see the "submit" button