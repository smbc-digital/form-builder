describe('Checkbox', () => {
    it('Checkbox', () => {
      cy.visit('ui-checkbox')
        .toMatchingDOM('govuk-form-group')
    });
  });