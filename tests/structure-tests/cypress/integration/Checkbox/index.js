describe('Checkbox', () => {
    it('Checkbox', () => {
      cy.visit('ui-checkbox')
        .toMatchingDOM('govuk-form-group')
    });

    it('Checkbox validation', () => {
      cy.visit('ui-checkbox')
      cy.get('.govuk-button').click()
        .toMatchingDOM()
    });
    
    it('Checkbox optional', () => {
      cy.visit('ui-checkbox-conditional')
        .toMatchingDOM('govuk-form-group')
    });

    it('Checkbox conditional', () => {
      cy.visit('ui-checkbox-conditional')
        .toMatchingDOM('govuk-form-group')
    });
  }); 