@select
Feature: Select
	In order to enter options I need to add option buttons

Scenario: Renders HTML tags on the page
	Given I navigate to "/select/page1"
	Then I should see the header
	And I should see the "select" input
	And I should see the "submit" button

Scenario: User selects Tuesday on page1
	Given I navigate to "/select/page1"
	Then I should see "Select an option..." is selected in dropdown with the value ""
	When I select "tuesday" in "select" dropdown
	Then I should see "Tuesday" is selected in dropdown with the value "tuesday"

Scenario: User enters nothing on page1
	Given I navigate to "/select/page1"
	When I click the "submit" button
	Then I should see a validation message for "select-error" input

Scenario: User enters nothing on second select on page1
	Given I navigate to "/select/page1"
	When I select "tuesday" in "select" dropdown
	Then I should see "Tuesday" is selected in dropdown with the value "tuesday"
	When I click the "submit" button
	Then I should see a validation message for "favFood-error" input
	Then I should see "Tuesday" is selected in dropdown with the value "tuesday"
