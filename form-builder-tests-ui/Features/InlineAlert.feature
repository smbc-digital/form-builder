Feature: InlineAlert

@inlinealert
Scenario: Render inline alert on the page correctly
	Given I navigate to "/inlinealert/page1"
	Then I should see the header
	And I should see a "h2" element with "This is a label" text
	And I should see a "p" element with "This is some text" text