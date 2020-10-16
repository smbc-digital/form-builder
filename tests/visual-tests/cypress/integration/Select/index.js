describe('Select', () => {
    it('Select', () => {
      cy.visit('ui-select');
      cy.matchImageSnapshot('select');
    });
  });