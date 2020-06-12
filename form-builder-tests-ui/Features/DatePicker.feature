@datepicker
Feature: DatePicker
	In order to collect a date users enter a date in the date picker

Scenario: Datepicker standard use
	Given I navigate to "/signoffgroup5datepicker/date-picker"
	When I click the "continue" button
	Then I should see a validation message for "passportIssued-error" date picker
	Then I should see a validation message for "dob-error" date picker
	And I should not see a validation message for "futureYear-error" date picker
	When I select "02082042" on "passportIssued" date picker
	Then I click the "continue" button
	Then I should see a validation error with an id "passportIssued-error" with "Check the date and try again" text
	Then I wait five seconds
	Then I select "12052018" on "passportIssued" date picker
	Then I select "08112005" on "dob" date picker
	Then I click the "continue" button
	Then I wait five seconds