describe('Textbox', () => {
    it('Textbox', () => {
      cy.visit('ui-textbox')
        .toMatchingDOM('govuk-form-group', 0)
    });

    it('Textbox optional', () => {
      cy.visit('ui-textbox')
        .toMatchingDOM('govuk-form-group', 1)
    });

    it('Textbox validation', () => {
      cy.visit('ui-textbox')
        .get('.govuk-button').click()
        .toMatchingDOM('govuk-form-group', 2)
    });

    it('Textbox with prefix', () => {
      cy.visit('ui-textbox')
        .toMatchingDOM('govuk-form-group', 3)
    });
  });