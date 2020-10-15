describe('Declaration', () => {
    it('Declaration', () => {
      cy.visit('ui-declaration');
      cy.matchImageSnapshot('declaration');
    });
  });