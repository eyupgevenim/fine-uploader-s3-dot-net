using System.IO;
using System.Threading.Tasks;

namespace WebApp.Services
{
    public interface IAwsS3Service
    {
        /// <summary>
        /// Stream as policy document
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>string</returns>
        Task<string> GetPolicyDocumentAsync(Stream stream);

        /// <summary>
        /// policy document convert to Base64
        /// </summary>
        /// <param name="policyDocument"></param>
        /// <returns>policy document Base64</returns>
        string GetBase64Policy(string policyDocument);
            
        /// <summary>
        /// get aws signature from policy document Base64
        /// </summary>
        /// <param name="policyDocument"></param>
        /// <returns>signature</returns>
        string GetSignature(string policyDocument);

        /// <summary>
        /// delete aws s3 buckut object
        /// </summary>
        /// <param name="key">s3 bucket object key</param>
        /// <returns>bool</returns>
        Task<bool> DeleteObjectAsync(string key);
    }
}
