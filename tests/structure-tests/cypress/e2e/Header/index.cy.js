describe('Header', () => {
    it('Header', () => {
      cy.visit('ui-header')
        .toMatchingDOM('smbc-header')
    });
  });