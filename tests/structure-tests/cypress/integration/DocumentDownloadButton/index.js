describe('DocumentDownloadButton', () => {
    it('Declaration', () => {
      cy.visit('ui-document-download')
        .toMatchingDOM('govuk-form-group', 0)
    });

    // it('Declaration no label', () => {
    //   cy.visit('ui-declaration')
    //     .toMatchingDOM('govuk-form-group', 1)
    // });
  });