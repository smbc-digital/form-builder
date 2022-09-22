describe('Checkbox', () => {
  it('Checkbox', () => {
    cy.visit('ui-checkbox')
      .toMatchingDOM('govuk-form-group', 0)
  });

  it('Checkbox optional', () => {
    cy.visit('ui-checkbox')
      .toMatchingDOM('govuk-form-group', 1)
  });

  it('Checkbox validation', () => {
    cy.visit('ui-checkbox')
    cy.get('.govuk-button').click()
      .toMatchingDOM()
  });
});
  
describe('Checkbox with conditional element', () => {
  it('Checkbox with conditional element', () => {
    cy.visit('ui-checkbox-conditional')
      .toMatchingDOM()
  });
});

describe('Checkbox with select exact number of options', () => {
  it('Checkbox with select exact number of options', () => {
    cy.visit('ui-checkbox-select-exactly')
      .toMatchingDOM()
  })

  it('Checkbox with select exact number of options validation', () => {
    cy.visit('ui-checkbox-select-exactly')
    cy.get('#optionalHobbies-0').click()
    cy.get('.govuk-button').click()
      .toMatchingDOM()
  })
})
