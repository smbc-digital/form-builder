describe('Textarea', () => {
    it('Textarea', () => {
      cy.visit('ui-textarea');
      cy.matchImageSnapshot('textarea');
    });
  });