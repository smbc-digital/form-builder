describe('Multiple file upload', () => {
    it('Multiple file upload', () => {
      cy.visit('ui-multiple-file-upload')
        .toMatchingDOM()
    });
  });