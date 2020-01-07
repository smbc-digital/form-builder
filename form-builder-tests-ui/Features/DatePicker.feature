@datepicker
Feature: DatePicker
	In order to collect a date users enter a date in the date picker

Scenario: User does not enter a date
	Given I navigate to "/datepicker/page1"
	When I click the "nextPage" button
	Then I should see a validation message for "passportIssued1-error" date picker
	And I should not see a validation message for "dob1-error" date picker
	
Scenario: Can collect date as single field
	Given I navigate to "/datepicker/page1"
	Then I select "2022-05-12" on "passportIssued1" date picker
	When I click the "nextPage" button
	Then I wait five seconds
	Then I select "2018-05-12" on "passportIssued2" date picker
	Then I click the "nextPage3" button
	Then I wait five seconds
	Then I should see a "th" element with "passportIssued1" text
	And I should see a "th" element with "passportIssued2" text
