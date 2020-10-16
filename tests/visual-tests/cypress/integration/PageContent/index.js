describe('PageContent', () => {
    it('PageContent', () => {
      cy.visit('ui-page-content');
      cy.matchImageSnapshot('page-content');
    });
  });