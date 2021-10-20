describe('DatePicker', () => {
    it('DateInpDatePickerut', () => {
      cy.visit('ui-datepicker')
        .toMatchingDOM('govuk-form-group')
    });
  });