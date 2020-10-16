describe('Radio', () => {
  it('snapshot test', () => {
    cy.visit('ui-radio')
      .toMatchingDOM()
  });
});