Feature: Timeinput
	In order to enter time
	As a user
	I want to be enter time in am or pm

@timeinput
Scenario: Renders HTML tags on the page
	Given I navigate to "/time/page1"
	Then I should see the header
	And I should see the "timeid-hours" input
	And I should see the "timeid-minutes" input
	