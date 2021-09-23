describe('Success', () => {
    it('Success', () => {
      cy.visit('UI-Success-Page');
      cy.get('.govuk-button').click()
        .toMatchingDOM()
    });
  });