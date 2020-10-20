const fs = require('fs')
   
module.exports = (on, config) => {
    on('task', {
      readFileMaybe (filename) {
        if (fs.existsSync(filename)) {
          return fs.readFileSync(filename, 'utf8')
        }
  
        return null
      }
    })
};