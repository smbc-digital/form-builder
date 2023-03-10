describe('Organisation', () => {
  it('Organisation Search', () => {
    cy.visit('ui-organisation')
      .toMatchingDOM()
  });

  it('Organisation Select', () => {
    cy.visit('ui-organisation')
      .get('.govuk-input').type('org name')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });

  it('Organisation Search Validation', () => {
    cy.visit('ui-organisation')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });

  it('Organisation Select Validation', () => {
    cy.visit('ui-organisation')
      .get('.govuk-input').type('org name')
      .get('.govuk-button').click()
      .get('.govuk-button').click()
      .toMatchingDOM()
  });
});