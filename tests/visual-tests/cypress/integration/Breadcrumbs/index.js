describe('Address', () => {
    it('Address search', () => {
      cy.visit('ui-breadcrumbs');
      cy.matchImageSnapshot('breadcrumbs');
    });
  });