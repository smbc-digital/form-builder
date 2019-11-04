Feature: Button
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Render information on the page correctly
	Given I navigate to "/button/page1"
	Then I should see a "button" element with "Next step" text
	Then I should see a "button" element with "Custom Text" text
	Then I should find an element with class ".button-primary"
	Then I should find an element with class ".button-inverted"
