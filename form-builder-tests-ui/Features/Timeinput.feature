﻿@timeinput
Feature: Timeinput
	In order to enter time
	As a user
	I want to be enter time in am or pm


Scenario: Renders HTML tags on the page
	Given I navigate to "/time/page1"
	Then I should see the header
	And I should see the "timeid-hours" input
	And I should see the "timeid-minutes" input
	

Scenario: User enters strings in the day, month and year
	Given I navigate to "/time/page1"
	Then I fill the hours with "aa" value, minutes with "bb" value and ampm with "am" value on "timeid"
	When I click the "nextPage" button
	Then I should see a validation message for "timeid-error" input
	Then I should see time refilled in the time input with "aa" value, minutes with "bb" value and ampm with "am"  in for "timeid" blah
	