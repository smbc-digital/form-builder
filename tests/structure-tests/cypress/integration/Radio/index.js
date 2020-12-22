describe('Radio', () => {
  it('snapshot test', () => {
    cy.visit('ui-radio')
      .toMatchingDOM('govuk-form-group')
  });
  it('snapshot conditional test', () => {
    cy.visit('ui-radio-conditional')
      .toMatchingDOM('govuk-form-group')
  });
});