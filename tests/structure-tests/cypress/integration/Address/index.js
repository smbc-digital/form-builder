describe('Address', () => {
  it('Address Search', () => {
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

  it('Address Search Validation', () => {
    cy.visit('ui-address')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });

  it('Address Select Validation', () => {
    cy.visit('ui-address')
      .get('.govuk-input').type('sk11aa')
      .get('.govuk-button').click()
      .get('.govuk-button').click()
      .toMatchingDOM()
  });

  it('Address Manual Validation', () => {
    cy.visit('ui-address')
      .get('.govuk-input').type('sk11zz')
      .get('.govuk-button').click()
      .get('#address-AddressLine1').type('Address line 1')
      .get('#address-AddressTown').type('Town or city')
      .get('.govuk-button').click()
      .toMatchingDOM()
  });  
});