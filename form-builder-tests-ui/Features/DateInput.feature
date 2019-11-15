@dateinput
Feature: DateInput
	In order to collect a date users enter a day, month and year


Scenario: User does not fill in any fields
	Given I navigate to "/dateinput/page1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued-error" input
	Then I should not see a validation message for "dob-error" input

Scenario: User enters strings in the day, month and year
	Given I navigate to "/dateinput/page1"
	Then I fill the day with "aa" value, month with "bb" value and year with "cccc" value
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued-error" input

Scenario: User enters today's date
	Given I navigate to "/dateinput/page1"
	Then I fill the date input with today's date
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued-error" input

Scenario: User enters a date in the past
	Given I navigate to "/dateinput/page2"
	Then I fill the day with "01" value, month with "01" value and year with "2010" value
	When I click the "nextPage2" button
	Then I should see a validation message for "passportIssued-error" input

Scenario: User enters a date in the future
	Given I navigate to "/dateinput/page3"
	Then I fill the day with "01" value, month with "01" value and year with "2022" value
	When I click the "submit" button
	Then I should see a validation message for "passportIssued-error" input