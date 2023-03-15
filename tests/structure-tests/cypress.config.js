const { defineConfig } = require('cypress')

module.exports = defineConfig({
  e2e: {
    setupNodeEvents(on, config) {},
    baseUrl: 'https://localhost:5001/',
    defaultCommandTimeout: 20000,
    pageLoadTimeout: 90000,
    responseTimeout: 60000
  },
})
