describe('DatePicker', () => {
    it('DatePicker', () => {
      cy.visit('ui-datepicker');
      cy.matchImageSnapshot('datepicker');
    });
  });