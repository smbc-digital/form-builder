describe('Multiple file upload', () => {
  it('Mandatory multiple file upload', () => {
    cy.visit('ui-multiple-file-upload')
    cy.get('#FileOptional-fileupload-SubmitButton').click()
      .toMatchingDOM()
  });

  it('Optional multiple file upload', () => {
    cy.visit('ui-multiple-file-upload')
      .toMatchingDOM()
  });

  it('File uploaded', () => {
    const fileName = 'files/testImage.jpg'

    cy.visit('ui-multiple-file-upload')
    cy.get('#FileOptional-fileupload-SubmitButton').click()
    cy.get('#File-fileupload').attachFile(fileName)
    cy.wait(1000)
    cy.get('#upload').click()
      .toMatchingDOM()
  });

  it('No file uploaded validation', () => {
    cy.visit('ui-multiple-file-upload')
    cy.get('#FileOptional-fileupload-SubmitButton').click()
    cy.get('#upload').click()
      .toMatchingDOM()
  });
});