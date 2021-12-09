describe('DocumentDownload', () => {
    it('DocumentDownload', () => {
      cy.visit('ui-document-download')
      .toMatchingDOM('govuk-grid-column-two-thirds', 0)
    });
  });