describe('DateInput', () => {
    it('DateInput', () => {
      cy.visit('ui-date-input')
        .toMatchingDOM('govuk-form-group')
    });
  });