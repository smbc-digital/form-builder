const { defineConfig } = require('cypress')

module.exports = defineConfig({
  e2e: {
    setupNodeEvents(on, config) {},
    baseUrl: 'https://localhost:5001/',
  },
})
