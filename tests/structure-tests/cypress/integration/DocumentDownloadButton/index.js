describe('DocumentDownloadButton', () => {
    it('Declaration', () => {
      cy.visit('ui-document-download')
        .toMatchingDOM('govuk-form-group', 0)
    });
  });