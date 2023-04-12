describe('DateInput', () => {
    it('DateInput Required', () => {
      cy.visit('ui-date-input')
        .toMatchingDOM('govuk-form-group',0)
    });
  
    it('DateInput Optional', () => {
      cy.visit('ui-date-input')
        .toMatchingDOM('govuk-form-group',1)
    });
  
    it('DateInput Limitation', () => {
      cy.visit('ui-date-input')
        .toMatchingDOM('govuk-form-group',2)
    });

    it('DateInput Validation', () => {
      cy.visit('ui-date-input')
        .get('.govuk-button').click()        
        .toMatchingDOM('govuk-form-group')
    });
});