describe('Booking', () => {
    it('Date selection', () => {
      cy.visit('ui-booking')
      FillOutRequiredUserDetailsPages()

      cy.toMatchingDOM("govuk-form-group")
    });
  
    it('Confirm your booking', () => {
      cy.visit('ui-booking')
      FillOutRequiredUserDetailsPages()

        cy.get('[type="radio"]').first().check()
        .get('.govuk-button').click()
        .toMatchingDOM()
    });
  });

  const FillOutRequiredUserDetailsPages = () => {
    cy.get('#firstName').type('firstname')
        .get('#lastName').type('lastname')
        .get('#email').type('test@stockport.com')
        .get('.govuk-button').click()
        .get('.govuk-input').type('sk11aa')
        .get('.govuk-button').click()
        .get('.govuk-select').select('123456789012|address 1')
        .get('.govuk-button').click()
  }