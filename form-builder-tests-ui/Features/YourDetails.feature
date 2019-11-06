@yourdetails
Feature: YourDetails
	In order to fill in my details I have to navigate to Your Details


Scenario: User navigates to ReportAnIssue/Your-Details
	Given I navigate to "/ReportAnIssue/Your-Details"
	Then I should see the header
	And I should see the "firstName" input
	And I should see the "lastName" input
	And I should see the "middleName" input
	And I should see the "address" input
	And I should see the "nextStep" button


Scenario: User fills in data and clicks next
	Given I navigate to "/ReportAnIssue/Your-Details"
	Then I fill in your details
	Then I click the "nextStep" button
	And I should see the header
	Then I should see the "issueDetails" input
	Then I fill in issue details
	Then I click the "nextStep" button
	Then I should see the header
	And I should see the "Do you have more details?" fieldset
	