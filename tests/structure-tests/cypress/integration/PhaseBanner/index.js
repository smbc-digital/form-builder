describe('PhaseBanner', () => {
    it('Phase banner', () => {
      cy.visit('ui-phasebanner')
        .toMatchingDOM()
    });
  });