describe('Footer', () => {
    it('Footer', () => {
      cy.visit('ui-button')
        .toMatchingDOM('smbc-footer')
    });
  });