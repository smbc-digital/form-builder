describe('Declaration', () => {
    it('Declaration', () => {
      cy.visit('ui-declaration')
        .toMatchingDOM('govuk-form-group')
    });
  });