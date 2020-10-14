describe('Address', () => {
  it('Address search', () => {
    cy.visit('https://localhost:5000/ui-address');
    cy.matchImageSnapshot('addressSearch');
  });

  it('Address Select', () => {
    cy.visit('https://localhost:5000/ui-address');
    cy.get('.govuk-input').type('sk11aa');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('addressSelect');
  });

  it('Address Manual', () => {
    cy.visit('https://localhost:5000/ui-address');
    cy.get('.govuk-input').type('sk11zz');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('addressManual');
  });
});