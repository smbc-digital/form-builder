describe('DocumentDownloadButton', () => {
    it('DocumentDownloadButton', () => {
      cy.visit('ui-document-download')
        .toMatchingDOM('govuk-form-group', 0)
    });
  });