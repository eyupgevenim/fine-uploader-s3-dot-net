using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApp.Infrastructure.Options;

namespace WebApp.Services
{
    public class AwsS3Service : IAwsS3Service
    {
        /// <summary>
        /// SCHEME : AWS4
        /// </summary>
        public const string SCHEME = "AWS4";
        /// <summary>
        /// TERMINATOR : aws4_request
        /// </summary>
        public const string TERMINATOR = "aws4_request";
        /// <summary>
        /// SERVICE :s3
        /// </summary>
        public const string SERVICE = "s3";
        /// <summary>
        /// HMACSHA256 : HMACSHA256
        /// </summary>
        public const string HMACSHA256 = "HMACSHA256";

        public readonly IOptions<AwsOptions> awsOptions;
        public AwsS3Service(IOptions<AwsOptions> awsOptions)
        {
            this.awsOptions = awsOptions;
        }

        /// <summary>
        /// Stream as policy document
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>string</returns>
        public async Task<string> GetPolicyDocumentAsync(Stream stream)
        {
            using (var bodyStream = new StreamReader(stream))
            {
                var policy_document = bodyStream.ReadToEndAsync();
                return await policy_document;
            }
        }

        /// <summary>
        /// policy document convert to Base64
        /// </summary>
        /// <param name="policyDocument"></param>
        /// <returns>policy document Base64</returns>
        public string GetBase64Policy(string policyDocument)
        {
            var base64Policy = Convert.ToBase64String(Encoding.UTF8.GetBytes(policyDocument));
            return base64Policy;
        }

        /// <summary>
        /// get aws signature from policy document Base64
        /// </summary>
        /// <param name="policyDocument"></param>
        /// <returns>signature</returns>
        public string GetSignature(string policyDocument)
        {
            dynamic policy_document_dyn = JsonConvert.DeserializeObject(policyDocument);
            var credential = string.Empty;
            foreach (var condition in policy_document_dyn.conditions)
            {
                string value = condition["x-amz-credential"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    credential = value;
                    break;
                }
            }

            Regex rgx = new Regex(".+\\/(.+)\\/(.+)\\/s3\\/aws4_request", RegexOptions.IgnoreCase);
            Match matcher = rgx.Match(credential);

            var signature = string.Empty;
            if (matcher.Success)
            {
                Group date = matcher.Groups[1];
                Group region = matcher.Groups[2];
                signature = DeriveSigningKey(region.ToString(), date.ToString(), GetBase64Policy(policyDocument));
            }

            return signature;
        }

        /// <summary>
        /// Constructs AmazonS3Client with AWS Access Key ID and AWS Secret Key
        /// </summary>
        /// <returns>AmazonS3Client</returns>
        private AmazonS3Client GetS3Client()
        {
            return new AmazonS3Client(awsOptions.Value.AccessKey, awsOptions.Value.SecretAccessKey);
        }

        /// <summary>
        /// delete aws s3 buckut object
        /// </summary>
        /// <param name="key">s3 bucket object key</param>
        /// <returns>bool</returns>
        public async Task<bool> DeleteObjectAsync(string key)
        {
            using (var client = GetS3Client())
            {
                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = awsOptions.Value.Bucket,
                    Key = key
                };
                var getObjectResponse = await client.GetObjectAsync(getObjectRequest);

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = awsOptions.Value.Bucket,
                    Key = key,
                    VersionId = getObjectResponse.VersionId
                };
                var deleteObjectResponse = await client.DeleteObjectAsync(deleteObjectRequest);

                return deleteObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Compute and return the multi-stage signing key for the request.
        /// </summary>
        /// <param name="region">The region in which the service request will be processed</param>
        /// <param name="date">Date of the request, in yyyyMMdd format</param>
        /// <returns>Computed signing key</returns>
        private string DeriveSigningKey(string region, string date, string stringToSign)
        {
            const string algorithm = HMACSHA256;
            const string ksecretPrefix = SCHEME;

            string awsSecretAccessKey = awsOptions.Value.SecretAccessKey;
            char[] ksecret = (ksecretPrefix + awsSecretAccessKey).ToCharArray();
            byte[] hashDate = ComputeKeyedHash(algorithm, Encoding.UTF8.GetBytes(ksecret), Encoding.UTF8.GetBytes(date));
            byte[] hashRegion = ComputeKeyedHash(algorithm, hashDate, Encoding.UTF8.GetBytes(region));
            byte[] hashService = ComputeKeyedHash(algorithm, hashRegion, Encoding.UTF8.GetBytes(SERVICE));
            byte[] hashSigning = ComputeKeyedHash(algorithm, hashService, Encoding.UTF8.GetBytes(TERMINATOR));
            byte[] hashSignature = ComputeKeyedHash(algorithm, hashSigning, Encoding.UTF8.GetBytes(stringToSign));

            return ToHexString(hashSignature, true);
        }

        /// <summary>
        /// Compute and return the hash of a data blob using the specified algorithm
        /// and key
        /// </summary>
        /// <param name="algorithm">Algorithm to use for hashing</param>
        /// <param name="key">Hash key</param>
        /// <param name="data">Data blob</param>
        /// <returns>Hash of the data</returns>
        private byte[] ComputeKeyedHash(string algorithm, byte[] key, byte[] data)
        {
            var kha = KeyedHashAlgorithm.Create(algorithm);
            kha.Key = key;
            return kha.ComputeHash(data);
        }

        /// <summary>
        /// Helper to format a byte array into string
        /// </summary>
        /// <param name="data">The data blob to process</param>
        /// <param name="lowercase">If true, returns hex digits in lower case form</param>
        /// <returns>String version of the data</returns>
        private static string ToHexString(byte[] data, bool lowercase)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
            }
            return sb.ToString();
        }
    }
}
