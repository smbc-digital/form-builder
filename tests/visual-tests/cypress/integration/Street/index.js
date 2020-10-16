describe('Street', () => {
    it('Street search', () => {
      cy.visit('ui-street');
      cy.matchImageSnapshot('streetSearch');
    });
  
    it('Street Select', () => {
      cy.visit('ui-street');
      cy.get('.govuk-input').type('street name');
      cy.get('.govuk-button').click();
      cy.matchImageSnapshot('streetSelect');
    });
    
    it('Street Select', () => {
      cy.visit('ui-street');
      cy.get('.govuk-input').type('nodata');
      cy.get('.govuk-button').click();
      cy.matchImageSnapshot('streetSelectNoResults');
    });
    
  });