describe('FileUpload', () => {
    it('FileUpload', () => {
      cy.visit('ui-file-upload')
        .toMatchingDOM()
    });
  });