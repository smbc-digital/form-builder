describe('Footer', () => {
    it('Footer', () => {
      cy.visit('ui-footer')
        .toMatchingDOM('smbc-footer')
    });
  });