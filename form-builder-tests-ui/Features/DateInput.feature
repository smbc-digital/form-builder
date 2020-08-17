@dateinput
Feature: DateInput
	In order to collect a date users enter a day, month and year


Scenario: Dateinput standard use
	Given I navigate to "/ui-date-input/page1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssuedone-error" input
	Then I should not see a validation message for "dobone-error" input
	Then I fill the day with "12" value, month with "12" value and year with "4300" value on "passportIssuedone"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssuedone-error" input
	Then I should not see a validation message for "dobone-error" input
	Then I fill the day with "34" value, month with "33" value and year with "1234" value on "passportIssuedone"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssuedone-error" input
	Then I should see the values "34", "33" and "1234" in the date input for "passportIssuedone" blah
	Then I fill the date input with today's date in "passportIssuedone"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssuedone-error" input
	Then I should see todays date refilled in the date input in for "passportIssuedone" blah

Scenario: User enters a date in the past
	Given I navigate to "/ui-date-input/page1"
	Then I fill the day with "01" value, month with "01" value and year with "2012" value on "passportIssuedone"
	When I click the "nextPage" button
	Given I navigate to "/dateinput/page2"
	Then I fill the day with "01" value, month with "01" value and year with "2010" value on "passportIssuedtwo"
	When I click the "nextPage2" button
	Then I should see a validation message for "passportIssuedtwo-error" input
	Then I should see the values "01", "01" and "2010" in the date input for "passportIssuedtwo" blah

Scenario: User enters a date in the future
	Given I navigate to "/ui-date-input/page1"
	Then I fill the day with "01" value, month with "01" value and year with "2012" value on "passportIssuedone"
	When I click the "nextPage" button
	Given I navigate to "/dateinput/page2"
	Then I fill the day with "01" value, month with "01" value and year with "2110" value on "passportIssuedtwo"
	Given I navigate to "/dateinput/page3"
	Then I fill the day with "01" value, month with "01" value and year with "2022" value on "passportIssuedthree"
	When I click the "nextPage3" button
	Then I should see a validation message for "passportIssuedthree-error" input
	Then I should see the values "01", "01" and "2022" in the date input for "passportIssuedthree" blah