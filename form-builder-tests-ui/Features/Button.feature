@button
Feature: Button
	In order to verify buttons are renderd correctly

Scenario: Render buttons correctly and show and hide the previous button.
	Given I navigate to "/ui-button/page1"
	Then I should see an element with ID of "continue"
	Then I should see an element with ID of "secondaryBtn"
	Then I should find an element with class ".govuk-button--secondary"
	Then I should see an element with ID of "warningBtn"
	Then I should find an element with class ".govuk-button--warning"
	Then I should see an element with ID of "disabledBtn"
	When I click the "continue" button
	Then I should see a ".govuk-back-link" html element