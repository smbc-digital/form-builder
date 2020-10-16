describe('Breadcrumbs', () => {
    it('Breadcrumbs', () => {
      cy.visit('ui-breadcrumbs')
        .toMatchingDOM()
    });
  });