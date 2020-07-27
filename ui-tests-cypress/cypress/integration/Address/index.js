describe('Address', () => {
  it('Address search', () => {
    cy.visit('https://localhost:44360/address');
    cy.matchImageSnapshot('addressSearch');
  });

  it('Address Select', () => {
    cy.visit('https://localhost:44360/address');
    cy.get('.govuk-input').type('sk11aa');
    cy.get('.govuk-button').click();
    cy.matchImageSnapshot('addressSelect');
  });
});




// it('Address Select', async () => {
//   const page = await browser.newPage();
//   await page.goto('https://localhost:5000');

//   const elementHandle = await page.$('.govuk-input');
//   await elementHandle.type('sk11aa');

//   await Promise.all([
//     page.click("button"),
//     page.waitForNavigation({ waitUntil: 'networkidle0' }),
//   ]);

//   const image = await page.screenshot({ fullPage: true});

//   expect(image).toMatchImageSnapshot();
// });

// it('Address Manual', async () => {
//   const page = await browser.newPage();
//   await page.goto('https://localhost:5000');

//   const elementHandle = await page.$('.govuk-input');
//   await elementHandle.type('sk22aa');

//   await Promise.all([
//     page.click("button"),
//     page.waitForNavigation({ waitUntil: 'networkidle0' }),
//   ]);

//   const image = await page.screenshot({ fullPage: true });

//   expect(image).toMatchImageSnapshot();
// });