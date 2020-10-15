describe('Address', () => {
  it('Address search', () => {
    cy.visit('ui-address');
    cy.matchImageSnapshot('addressSearch');
  });

  it('Address Select', () => {
    cy.visit('ui-address');
    cy.get('.govuk-input').type('sk11aa');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('addressSelect');
  });

  it('Address Manual', () => {
    cy.visit('ui-address');
    cy.get('.govuk-input').type('sk11zz');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('addressManual');
  });
});