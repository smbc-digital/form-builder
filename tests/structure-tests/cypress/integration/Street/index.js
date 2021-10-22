describe('Street', () => {
    it('Street Search', () => {
      cy.visit('ui-street')
        .toMatchingDOM()
    });
  
    it('Street Select', () => {
      cy.visit('ui-street')
        .get('.govuk-input').type('street name')
        .get('.govuk-button').click()
        .toMatchingDOM()
    });
  
    it('Optional Street Search', () => {
      cy.visit('ui-street')
        .get('.govuk-input').type('street name')
        .get('.govuk-button').click()
        .get('.govuk-select').select("Green street")
        .get('.govuk-button').click()
        .toMatchingDOM()
    });  
  });