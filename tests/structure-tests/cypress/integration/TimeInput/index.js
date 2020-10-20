describe('TimeInput', () => {
    it('TimeInput', () => {
      cy.visit('ui-time-input')
        .toMatchingDOM('govuk-form-group')
    });
  });