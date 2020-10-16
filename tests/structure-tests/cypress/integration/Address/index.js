describe('Address', () => {
  it('Address search', () => {
    cy.visit('ui-address')
      .toMatchingDOM()
  });

  it('Address Select', () => {
    cy.visit('ui-address')
      .get('.govuk-input').type('sk11aa')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });

  it('Address Manual', () => {
    cy.visit('ui-address')
      .get('.govuk-input').type('sk11zz')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });
});