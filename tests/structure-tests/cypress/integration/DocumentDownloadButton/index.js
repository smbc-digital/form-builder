describe('DocumentDownload', () => {
    it('DocumentDownload', () => {
      cy.visit('ui-document-download')
      .toMatchingDOM()
    });
  });