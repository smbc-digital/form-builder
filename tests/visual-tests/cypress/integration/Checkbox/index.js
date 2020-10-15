describe('Checkbox', () => {
    it('Checkbox', () => {
      cy.visit('ui-checkbox');
      cy.matchImageSnapshot('checkbox');
    });
  });