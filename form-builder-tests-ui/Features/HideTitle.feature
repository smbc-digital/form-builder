@HideTitle
Feature: hideTitle
	To render a title if the hideTitle property is set to false

Scenario: Render information on the page correctly
	Given I navigate to "/hidetitle/page1"
	Then I should see a "H1" element with "Page title page 1" text
	When I click the "radButtonhidetitle-0" radiobutton
	When I click the "nextStep" button
	Then I should not see the h1 element with page title text

	
	
	