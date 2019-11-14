@radiobutton
Feature: DateInput
	In order to collect a date users enter a day, month and year


Scenario: User does not fill in any fields
	Given I navigate to "/dateinput/page1"
	When I click the "submit" button
	Then I should see a validation message for "passportIssued-error" input
	Then I should not see a validation message for "dob-error" input

Scenario: User enters strings in the day, month and year
	Given I navigate to "/dateinput/page1"
	Then I click
	Then I should see a validation message for "passportIssued-error" input