@textbox
Feature: Textbox
	In order to fill in my details I have to navigate to Page1


Scenario: Renders HTML tags on the page
	Given I navigate to "/Textbox/page1"
	Then I should see the header
	And I should see the "firstName" input
	And I should see the "lastName" input
	And I should see the "nextStep" button


Scenario: User fills in data and clicks next
	Given I navigate to "/Textbox/page1"
	Then I fill in page1
	Then I press the next step button
	Then I should see the "otherNames" input
