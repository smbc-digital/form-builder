describe('Radio', () => {
  it('snapshot test', () => {
    cy.visit('https://localhost:5000/radiobutton');
    cy.matchImageSnapshot('radio');
  });
});