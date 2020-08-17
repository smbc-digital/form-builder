@datepicker
Feature: DatePicker
	In order to collect a date users enter a date in the date picker

Scenario: Datepicker standard use
	Given I navigate to "/datepicker/page1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssuedone-error" date picker
	And I should not see a validation message for "dobone-error" date picker
	When I select "02082022" on "passportIssuedone" date picker
	Then I click the "nextPage" button
	Then I sleep "1000"
	Then I select "12052018" on "passportIssuedtwo" date picker
	Then I click the "nextPage3" button
	Then I sleep "1000"
	Then I should see a "th" element with "passportIssuedone" text
	And I should see a "th" element with "passportIssuedtwo" text