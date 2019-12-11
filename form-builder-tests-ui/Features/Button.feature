Feature: Button
	In order to verify buttons are renderd correctly

Scenario: Render buttons on the page correctly
	Given I navigate to "/button/page1"
	Then I should see a "button" element with "Next step" text
	Then I should see a "button" element with "Custom Text" text
	Then I should find an element with class ".button-primary"
	Then I should find an element with class ".button-inverted"

Scenario: Button not on firstpage should display back anchor
	Given I navigate to "/button/page1"
    Then I should not see any "back-button" html element
	When I click the "nextStep1" button
	Then I should see a "a" element with "Previous" text

Scenario: Button on 3nd page should not display previous link if json says not to
	Given I navigate to "/button/page1"
    When I click the "nextStep1" button
	Then I should see a "a" element with "Previous" text
	When I click the "nextStep1" button
	Then I should not see any "back-button" html element
	
	