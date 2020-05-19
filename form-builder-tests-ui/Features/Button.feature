Feature: Button
	In order to verify buttons are renderd correctly

Scenario: Render buttons correctly and show and hide the previous button.
	Given I navigate to "/button/page1"
	Then I should see a "button" element with "Next step" text
	Then I should see a "button" element with "Secondary Button" text
	Then I should find an element with class ".govuk-button--secondary"
    #Then I should not see any ".govuk-back-link" html element
	When I click the "nextStep1" button
	#Then I should see a "div.a" element with "Back" text
	Then I should see a ".govuk-back-link" html element
	#When I click the "nextStep1" button
	#Then I should not see any ".govuk-back-link" html element
	
	