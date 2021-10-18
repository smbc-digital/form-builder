describe('Header', () => {
    it('Header', () => {
      cy.visit('ui-button')
        .toMatchingDOM('smbc-header')
    });
  });