describe('FileUpload', () => {
    it('FileUpload', () => {
      cy.visit('ui-file-upload');
      cy.matchImageSnapshot('fileUpload');
    });
  });