@dateinput
Feature: DateInput
	In order to collect a date users enter a day, month and year


Scenario: User does not fill in any fields
	Given I navigate to "/dateinput/page1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued1-error" input
	Then I should not see a validation message for "dob1-error" input
	Then I fill the day with "34" value, month with "33" value and year with "1234" value on "passportIssued1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued1-error" input
	Then I should see the values "34", "33" and "1234" in the date input for "passportIssued1" blah
	Then I fill the date input with today's date in "passportIssued1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued1-error" input
	Then I should see todays date refilled in the date input in for "passportIssued1" blah

Scenario: User enters a date in the past
	Given I navigate to "/dateinput/page1"
	Then I fill the day with "01" value, month with "01" value and year with "2012" value on "passportIssued1"
	When I click the "nextPage" button
	Given I navigate to "/dateinput/page2"
	Then I fill the day with "01" value, month with "01" value and year with "2010" value on "passportIssued2"
	When I click the "nextPage2" button
	Then I should see a validation message for "passportIssued2-error" input
	Then I should see the values "01", "01" and "2010" in the date input for "passportIssued2" blah

Scenario: User enters a date in the future
	Given I navigate to "/dateinput/page1"
	Then I fill the day with "01" value, month with "01" value and year with "2012" value on "passportIssued1"
	When I click the "nextPage" button
	Given I navigate to "/dateinput/page2"
	Then I fill the day with "01" value, month with "01" value and year with "2110" value on "passportIssued2"
	Given I navigate to "/dateinput/page3"
	Then I fill the day with "01" value, month with "01" value and year with "2022" value on "passportIssued3"
	When I click the "nextPage3" button
	Then I should see a validation message for "passportIssued3-error" input
	Then I should see the values "01", "01" and "2022" in the date input for "passportIssued3" blah