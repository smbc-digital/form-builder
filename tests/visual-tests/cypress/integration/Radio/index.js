describe('Radio', () => {
  it('snapshot test', () => {
    cy.visit('https://localhost:5000/ui-radio');
    cy.matchImageSnapshot('radio');
  });
});