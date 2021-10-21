describe('Radio', () => {
  it('Radio', () => {
    cy.visit('ui-radio')
      .toMatchingDOM('govuk-form-group', 0)
  });

  it('Radio optional', () => {
    cy.visit('ui-radio')
      .toMatchingDOM('govuk-form-group', 1)
  });

  it('Radio validation', () => {
    cy.visit('ui-radio')
    cy.get('.govuk-button').click()
      .toMatchingDOM()
  });
});

describe('Radio with conditional element', () => {
  it('Radio with conditional element', () => {
    cy.visit('ui-radio-conditional')
      .toMatchingDOM()
  });
});