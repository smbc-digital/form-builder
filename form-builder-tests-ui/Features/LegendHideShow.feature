@LegendHideShow
Feature: legendHideShow
	To render a pagetitle depending on showTitle and LegendAsH1

Scenario: Render information on the page correctly
	Given I navigate to "/ui-radio/page1"
	Then I should see a "H1" element with "Radio" text
	When I click the "radButton-0" radiobutton
	When I click the "continue" button
	Then I should not see the h1 element with page title text
	Then I should see a h1 element within a legend tag with text "Do you like things? (optional)"