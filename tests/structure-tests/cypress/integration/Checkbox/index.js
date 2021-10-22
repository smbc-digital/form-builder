describe('Checkbox', () => {
    it('Checkbox', () => {
      cy.visit('ui-checkbox')
        .toMatchingDOM('govuk-form-group', 0)
    });

    it('Checkbox validation', () => {
      cy.visit('ui-checkbox')
      cy.get('.govuk-button').click()
        .toMatchingDOM()
    });
    
    it('Checkbox optional', () => {
      cy.visit('ui-checkbox')
        .toMatchingDOM('govuk-form-group', 1)
    });

    it('Checkbox conditional', () => {
      cy.visit('ui-checkbox-conditional')
        .toMatchingDOM()
    });
  }); 