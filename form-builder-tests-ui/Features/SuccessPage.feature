@successpage
Feature: SuccessPage
	In order to submit a form
	as a User
	I should see a success page

Scenario: Success page standard use
	Given I navigate to "/successpage/page-one"
	Then I should see the "submit" button
	When I click the "submit" button
	Then I should see a ".smbc-panel" html element
	Then I should see a ".govuk-panel__title" html element
	Then I should see a ".govuk-panel__body" html element
    And I should see a "h1" element with "Application complete" text
	And I should see a "div" element with "Some additional information" text
	Then I should see a "h2" element with "What happens next" text
    Then I should see a "p" element with "We will contact you shortly to confirm if your request has been accepted." text