require('@cypress/snapshot').register()

const REGEX = /<input name="__RequestVerificationToken" type="hidden" value=".*">/g

Cypress.Commands.add('toMatchingDOM', (className = 'govuk-grid-column-two-thirds') => {
    cy.document().then((win) => {
        const html = win.activeElement.getElementsByClassName(className)[0].innerHTML.replace(REGEX, '')
        cy.wrap(html).snapshot({
          json: false
        })
      })
  })