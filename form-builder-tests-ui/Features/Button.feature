Feature: Button
	In order to verify buttons are renderd correctly

Scenario: Render buttons correctly and show and hide the previous button.
	Given I navigate to "/button/page1"
	Then I should see a "button" element with "Next step" text
	Then I should see a "button" element with "Custom Text" text
	Then I should find an element with class ".button-primary"
	Then I should find an element with class ".button-inverted"
    Then I should not see any "back-button" html element
	When I click the "nextStep1" button
	Then I should see a "a" element with "Previous" text
	When I click the "nextStep1" button
	Then I should not see any "back-button" html element
	
	