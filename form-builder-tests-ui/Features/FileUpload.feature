Feature: FileUpload
	In order to upload a file

Scenario: File upload optional and validation only
	Given I navigate to "/ui-file-upload/pageone"
	Then I should see the header
	And I should see the "fileUploadOne-fileupload" input
	When I click the "nextStep" button
	Then I should see the header
	And I should see the "fileUploadTwo-fileupload" input
	When I click the "nextStep" button
	Then I should see a ".govuk-error-message" html element