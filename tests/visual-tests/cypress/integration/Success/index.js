describe('Success', () => {
    it('Success', () => {
      cy.visit('ui-success-page');
      cy.get('.govuk-button').click();
      cy.matchImageSnapshot('success');
    });
  });