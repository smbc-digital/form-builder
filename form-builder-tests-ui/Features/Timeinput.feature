@timeinput
Feature: Timeinput
	In order to enter time
	As a user
	I want to be enter time in am or pm

Scenario: Time input standard use.
	Given I navigate to "/time/page1"
	Then I should see the header
	And I should see the "timeid-hours" input
	And I should see the "timeid-minutes" input
	Then I fill the hours with "23" value, minutes with "34" value and ampm with "am" value on "timeid"
	When I click the "nextPage" button
	Then I should see a validation message for "timeid-error" input
	Then I should see time refilled in the time input with "23" value, minutes with "34" value and ampm with "am"  in for "timeid" blah
	