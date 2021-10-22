describe('Checkbox', () => {
  it('Checkbox', () => {
    cy.visit('ui-checkbox')
      .toMatchingDOM('govuk-form-group', 0)
  });

  it('Checkbox optional', () => {
    cy.visit('ui-checkbox')
      .toMatchingDOM('govuk-form-group', 1)
  });

  it('Checkbox validation', () => {
    cy.visit('ui-checkbox')
    cy.get('.govuk-button').click()
      .toMatchingDOM()
  });
});
  
describe('Checkbox with conditional element', () => {
  it('Checkbox with conditional element', () => {
    cy.visit('ui-checkbox-conditional')
      .toMatchingDOM()
  });
});