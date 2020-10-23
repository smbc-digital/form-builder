describe('Select', () => {
    it('Select', () => {
      cy.visit('ui-select')
        .toMatchingDOM('govuk-form-group')
    });
  });