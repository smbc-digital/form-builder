Feature: PageContent

Scenario: Render information on the page correctly
	Given I navigate to "/pagecontent/page1"
	Then I should see a "h1" element with "Page content" text
	And I should see a "h2" element with "This is a H2" text
	And I should see a "h3" element with "This is a H3" text
	And I should see a "h4" element with "This is a H4" text
	And I should see a "h5" element with "This is a H5" text
	And I should see a "h6" element with "This is a H6" text
	And I should see a "p" element with "This is a paragraph" text

@pagecontent
Scenario: Render HTML within P tag elements
	Given I navigate to "/pagecontent/page1"
	Then I should see a strong element within a p tag