const https = require('https');

const formbuildersecret = ''

const updateCacheEntryWithinApplication2 = (host, applicationVersion) =>
    new Promise(function (resolve, reject) {
        var options = {
            host: host,
            port: '5000',
            path: `${applicationVersion}/system/index-schemas?api_key=${formbuildersecret}`,
            method: 'PATCH',
            rejectUnauthorized: false //only required for localhost testing
        };

        var req = https.request(options, function (res) {
            if (res.statusCode !== 200)
                return reject(new Error('Status Code:' + res.statusCode));

            res.on('data', function(chunk) {
                resolve('Status code: '+res.statusCode + '\n\rResponse: '+ chunk.toString())
            });
        });

        req.end();

        req.on('error', function(err) {
            reject(err);
        });
    });

async function init(host, applicationVersion) {
    var result = await updateCacheEntryWithinApplication2(host, applicationVersion)
    console.log(result)
}

//Host: host int/qa/stage/prod.stockport.gov.uk
//applicationVersion: empty string for local testing /v1 or /v2 - / required before version for path in http options above
init("localhost", "")