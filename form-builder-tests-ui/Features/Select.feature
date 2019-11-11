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
	When I select "tuesday" in "select" dropdown
	Then I should see "Tuesday" is selected in dropdown

Scenario: User enters nothing on page1
	Given I navigate to "/select/page1"
	When I click the "submit" button
	Then I should see a validation message for "select-error" input
