Feature: PageContent

@pagecontent
Scenario: Renders HTML tags correctly
	Given I navigate to "/pagecontent/page1"
	Then I should see a "h1" element with "Page content" text
	Then I should see a "h3" element with "This is a H3" text