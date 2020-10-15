describe('DateInput', () => {
    it('DateInput', () => {
      cy.visit('ui-date-input');
      cy.matchImageSnapshot('breadcrumbs');
    });
  });