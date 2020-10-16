describe('Street', () => {
    it('Street search', () => {
      cy.visit('ui-street')
        .toMatchingDOM()
    });
  
    it('Street Select', () => {
      cy.visit('ui-street')
        .get('.govuk-input').type('street name')
        .get('.govuk-button').click()
        .toMatchingDOM()
    });
    
    it('Street Select', () => {
      cy.visit('ui-street')
        .get('.govuk-input').type('nodata')
        .get('.govuk-button').click()
        .toMatchingDOM()
    });
    
  });