describe('Button', () => {
    it('Button', () => {
      cy.visit('ui-button');
      cy.matchImageSnapshot('button');
    });
  });