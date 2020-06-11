@summary
Feature: Summary
In order to see summaryIhave to enter data;

Scenario: I enterer details I should see them in the summary
	Given I navigate to "summary-1860/page-one"
	When I fill in page1
	And I click the "nextStep" button
	Then I enter "sk11aa" in "address-postcode"
	When I click the "nextStep" button
	Then I should see the "address-address" input
	Then I select "address 2" in "address-address" dropdown
	And I click the "nextStep" button
	Then I fill in page3
	And I click the "nextStep" button
	Then I should see a "dd" element with "first" text
	Then I should see a "dd" element with "last" text
	Then I should see a "dd" element with "test@mail.com" text
	Then I should see a "dd" element with "01614451234" text
	Then I should see a "dd" element with "address 2" text
	Then I should see a "dd" element with "01/02/1990" text



