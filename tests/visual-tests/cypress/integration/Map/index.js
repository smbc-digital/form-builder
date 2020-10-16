describe('Map', () => {
    it('Map', () => {
      cy.visit('ui-map');
      cy.matchImageSnapshot('map');
    });
  });