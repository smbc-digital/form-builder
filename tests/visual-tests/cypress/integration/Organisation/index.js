describe('Organisation', () => {
  it('Organisation search', () => {
    cy.visit('ui-organisation');
    cy.matchImageSnapshot('organisationSearch');
  });

  it('Organisation Select', () => {
    cy.visit('ui-organisation');
    cy.get('.govuk-input').type('org name');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('organisationSelect');
  });
  
  it('Organisation Select', () => {
    cy.visit('ui-organisation');
    cy.get('.govuk-input').type('nodata');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('organisationSelectNoResults');
  });
  
});