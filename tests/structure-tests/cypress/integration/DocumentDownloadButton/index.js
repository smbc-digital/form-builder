describe('DocumentDownload', () => {
    it('DocumentDownload', () => {
      cy.visit('ui-document-download')
      .get('.govuk-input').type('test')
      .get('.govuk-button').click()
      .toMatchingDOM('govuk-grid-column-two-thirds', 0)
    });
  });