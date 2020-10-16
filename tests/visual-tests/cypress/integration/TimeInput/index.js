describe('TimeInput', () => {
    it('TimeInput', () => {
      cy.visit('ui-time-input');
      cy.matchImageSnapshot('timeinput');
    });
  });