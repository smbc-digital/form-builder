@button
Feature: Button
	In order to verify buttons are renderd correctly

Scenario: Render buttons correctly and show and hide the previous button.
	Given I navigate to "/button/page1"
	Then I wait one second
	Then I should see an element with ID of "nextStep1"
	Then I should see an element with ID of "nextStep2"
	Then I should find an element with class ".govuk-button--secondary"
	Then I should see an element with ID of "nextStep3"
	Then I should find an element with class ".govuk-button--warning"
    Then I should not see any ".govuk-back-link" html element
	When I click the "nextStep1" button
	Then I should see a ".govuk-back-link" html element
	When I click the "nextStep1" button