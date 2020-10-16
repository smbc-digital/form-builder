describe('Multiple file upload', () => {
    it('Multiple file upload', () => {
      cy.visit('ui-multiple-file-upload');
      cy.matchImageSnapshot('multiple-file-upload');
    });
  });