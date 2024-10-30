using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace src.services
{
    public class Aws
    {
        private static readonly string S3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET");
        private static readonly string SSM_HEROKU_KEY = Environment.GetEnvironmentVariable("SSM_HEROKU_KEY");
        private static readonly AmazonSimpleSystemsManagementClient ssmClient;
        private static readonly AmazonS3Client s3Client;


        static Aws()
        {
            s3Client = new AmazonS3Client();
            ssmClient = new AmazonSimpleSystemsManagementClient();
        }

        public static async Task<string> PutFileS3(Stream stream, string fileName)
        {
            PutObjectRequest request = new()
            {
                BucketName = S3BucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = "application/zip"
            };

            await s3Client.PutObjectAsync(request);

            Console.WriteLine($"Successfully uploaded '{fileName}' to '{S3BucketName}'.");
            return $"Successfully uploaded '{fileName}' to '{S3BucketName}'.";
        }

        public static async Task<GetObjectResponse> GetFileS3(string fileName)
        {
            GetObjectRequest request = new() { BucketName = S3BucketName, Key = fileName };

            return await s3Client.GetObjectAsync(request);
        }

        public static async Task<string> GetSSM(string keyName = null)
        {
            keyName ??= SSM_HEROKU_KEY;
            var request = new GetParameterRequest
            {
                Name = keyName,
                WithDecryption = true
            };

            GetParameterResponse response = await ssmClient.GetParameterAsync(request);
            return response.Parameter.Value;
        }
    }
}