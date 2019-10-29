@my-details
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
