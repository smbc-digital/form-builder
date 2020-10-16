describe('DatePicker', () => {
    it('DatePicker', () => {
      cy.visit('ui-datepicker')
        .toMatchingDOM()
    });
  });