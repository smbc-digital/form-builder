describe('Radio', () => {
  it('snapshot test', () => {
    cy.visit('https://localhost:44360/radiobutton');
    cy.matchImageSnapshot('radio');
  });
});