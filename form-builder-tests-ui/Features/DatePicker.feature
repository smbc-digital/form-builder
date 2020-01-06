@datePicker
Feature: DatePicker
	In order to collect a date users enter a date in the date picker


Scenario: Goes to page1 if they attempt to go straight to page2
	Given I navigate to "/datepicker/page1"
	Then I should see the header
	