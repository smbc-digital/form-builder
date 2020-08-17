@address
Feature: ChangeSearch
	In order to collect address information I have to navigate to Page1

Scenario: Address element standard use
	Given I navigate to "/ui-address/page1"
	Then I should see the header
	And I should see the "address-postcode" input
	Then I enter "sk11aa" in "address-postcode"
	When I click the "nextStep" button
	Then I should see a "a" element with "Change" text
	When I click the "Change" link
	Then I wait one second
	Then I should see the "address-postcode" input