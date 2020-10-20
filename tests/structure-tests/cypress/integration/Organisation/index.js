describe('Organisation', () => {
  it('Organisation search', () => {
    cy.visit('ui-organisation')
      .toMatchingDOM()
  });

  it('Organisation Select', () => {
    cy.visit('ui-organisation')
      .get('.govuk-input').type('org name')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });
  
  it('Organisation Select', () => {
    cy.visit('ui-organisation')
      .get('.govuk-input').type('nodata')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });
  
});