const AWS = require('aws-sdk')
const https = require('https');

const key = ''
const secret = ''
const formbuildersecret = ''

const getFormSchemas = (env, applicationVersion) =>
    new Promise((resolve, reject) => {
        const s3 = new AWS.S3({
            region: 'eu-west-1',
            credentials: new AWS.Credentials(key, secret)
        })

        const bucketParams = {
            Bucket: 'forms-storage',
            Prefix: `${env}/${applicationVersion}`
        }

        s3.listObjectsV2(bucketParams, (err, data) => {
            if (err) {
                reject(new Error('Unable to fetch list of files within forms-storage'))
            } else if (data) {
                resolve(data.Contents.map(_ => _.Key))
            }
        })
    })

const updateCacheEntryWithinApplication = () => {
    var options = {
        host: 'localhost',
        port: '5000',
        path: `/system/index-schemas?api_key=${formbuildersecret}`, //path needs /v1/v2 appended on int/qa/stage/prod
        method: 'PATCH',
        rejectUnauthorized: false //only required for localhost testing
      };
      
      var req = https.request(options, function(res) {
        console.log("Status code: ", res.statusCode);
        res.on('data', function(d) {
            if(res.statusCode !== 200){
                throw new Error(d.toString())
            }
        });
      });
      
      req.end();
      
      req.on('error', function(e) {
        throw new Error(e)
      });
}

async function init(env, applicationVersion) {
    var results = await getFormSchemas(env, applicationVersion)
    await updateCacheEntryWithinApplication(results)
}

init("Int", "v1")