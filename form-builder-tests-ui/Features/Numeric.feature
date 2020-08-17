@numeric
Feature: Numeric
	In order to enter numeric data
	As a mobile device user
	I want to be have a number input

Scenario: number field validates max and min properties
	Given I navigate to "/numeric/page1"
	When I fill in in page 1 with incorrect numeric values
	Then I click the "nextStep" button
	Then I should see a validation message for "positiveInteger" numeric input
	And I should see a validation message for "negativeInteger" numeric input
	And I should see a validation message for "rangedInteger" numeric input
	And I should see a validation message for "rangedIntegerOptional" numeric input
