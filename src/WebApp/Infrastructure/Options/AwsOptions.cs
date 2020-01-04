namespace WebApp.Infrastructure.Options
{
    /// <summary>
    /// aws section on appsettings.json
    /// </summary>
    public class AwsOptions
    {
        /// <summary>
        /// AWS secret access key
        /// https://aws.amazon.com/tr/blogs/security/wheres-my-secret-access-key/
        /// </summary>
        public string SecretAccessKey { get; set; }
        /// <summary>
        /// AWS access key
        /// https://aws.amazon.com/tr/blogs/security/wheres-my-secret-access-key/
        /// </summary>
        public string AccessKey { get; set; }
        /// <summary>
        /// AWS region
        /// https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/Concepts.RegionsAndAvailabilityZones.html
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// AWS s3 bucket
        /// https://docs.aws.amazon.com/AmazonS3/latest/dev/UsingBucket.html
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// AWS s3 bucket endpoint
        /// https://docs.aws.amazon.com/vpc/latest/userguide/vpc-endpoints-s3.html
        /// </summary>
        public string Endpoint { get; set; }
    }
}
