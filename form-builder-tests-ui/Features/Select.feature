@select
Feature: Select
	In order to enter options I need to add option buttons

Scenario: Renders Select on the page
	Given I navigate to "/ui-select/page1"
	Then I should see the header
	And I should see the "select" input
	And I should see the "submit" button
	When I click the "submit" button
	Then I should see a validation message for "select-error" input
	Then I should see "Select an option..." is selected in "select" dropdown with the value ""
	When I select "tuesday" in "select" dropdown
	Then I should see "Tuesday" is selected in "select" dropdown with the value "tuesday"
	When I click the "submit" button