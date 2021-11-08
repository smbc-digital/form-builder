describe('DocumentDownload', () => {
    it('DocumentDownload', () => {
      cy.visit('ui-document-download')
        .toMatchingDOM('govuk-form-group', 0)
    });
  });