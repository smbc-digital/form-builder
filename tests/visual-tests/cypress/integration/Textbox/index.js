describe('Textbox', () => {
    it('Textbox', () => {
      cy.visit('ui-textbox');
      cy.matchImageSnapshot('textbox');
    });
  });