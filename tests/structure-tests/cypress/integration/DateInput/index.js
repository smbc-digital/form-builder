describe('DateInput', () => {
    it('DateInput Required', () => {
      cy.visit('ui-date-input')
      .toMatchingDOM('govuk-form-group')
    });
  
    it('DateInput Optional', () => {
      cy.visit('ui-date-input')
      .toMatchingDOM('govuk-form-group')
    });
  
    it('DateInput Limitation', () => {
      cy.visit('ui-date-input')
      .toMatchingDOM('govuk-form-group')
    });

    it('DateInput Validation', () => {
      cy.visit('ui-date-input')
      .get('.govuk-button').click()
      .toMatchingDOM('govuk-form-group')
    });
});