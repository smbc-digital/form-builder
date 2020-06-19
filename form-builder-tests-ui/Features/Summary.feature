@summary
Feature: Summary
In order to see summaryIhave to enter data;

Scenario: I enterer details I should see them in the summary
	Given I navigate to "summary-1860/page-one"
	When I fill in page1
	And I click the "continue" button
	Then I fill in page3
	And I click the "continue" button
	Then I click the "applesOrPears-0" radiobutton
	And I click the "continue" button
	Then I enter "test" in "whyApples"
	And I click the "continue" button
	Then I should see a "dd" element with "first" text
	Then I should see a "dd" element with "last" text
	Then I should see a "dd" element with "test@mail.com" text
	Then I should see a "dd" element with "01614451234" text
	Then I should see a "dd" element with "01/02/1990" text
	Then I should see a "dd" element with "test" text
	Then I click the "Back" link
	Then I click the "Back" link
	Then I click the "applesOrPears-1" radiobutton
	And I click the "continue" button
	Then I enter "shapely" in "whyPears"
	And I click the "continue" button
	Then I should see a "dd" element with "first" text
	Then I should see a "dd" element with "last" text
	Then I should see a "dd" element with "test@mail.com" text
	Then I should see a "dd" element with "01614451234" text
	Then I should see a "dd" element with "01/02/1990" text
	Then I should see a "dd" element with "shapely" text
	
	

	



