describe('Summary', () => {
    it('Summary', () => {
      cy.visit('ui-summary');
      cy.get('#firstName').type('firstname');
      cy.get('input[name=dob-day]').type('12')
      cy.get('input[name=dob-month]').type('04')
      cy.get('input[name=dob-year]').type('1991')
      cy.get('#applesOrPears-0').check()
      cy.get('#whyApples').type('this is a lot of text in this textbox which is being entered. Stockport Council ui test textbox');
      cy.get('.govuk-button').click()
      cy.get('#address-postcode').type('sk13xe')
      cy.get('.govuk-button').click()
      cy.get('#address-address').select('address 1')
      cy.get('.govuk-button').click()
        .toMatchingDOM('govuk-summary-list')
    });
  });