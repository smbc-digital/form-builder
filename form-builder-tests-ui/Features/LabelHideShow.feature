@LabelHideShow
Feature: LabelHideShow
	To render a pagetitle depending on showTitle and LabelAsH1

Scenario: Render information on the page correctly
	Given I navigate to "/textbox/page0"
	Then I should see a label element within a h1 tag with text "Question label"
	Then I enter "test" in "firstQuestion"
	Then I click the "nextStep" button
	Then I should see a "h1" element with "Textbox example" text