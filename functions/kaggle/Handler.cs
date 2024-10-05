using System.Net.Http.Headers;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace functions.kaggle;
public class HandlerFunction
{
  private static readonly string S3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET");
  private static readonly string S3FileKey = Environment.GetEnvironmentVariable("CONSTANT_UPLOAD_FILE_KEY");
  private static readonly string KaggleUserSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET_USER");
  private static readonly string KaggleDatasetSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET");
  private static readonly string BaseUrl = Environment.GetEnvironmentVariable("KAGGLE_BASE_URL");
  private static readonly string KagglePersonalUser = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_USERNAME");
  private static readonly string KagglePersonalKey = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_KEY");
  private static readonly HttpClient client;
  private static readonly AmazonS3Client s3Client;

  public async Task<string> KaggleDownload()
  {
    string url = $"{BaseUrl}/datasets/download/{KaggleUserSlug}/{KaggleDatasetSlug}";

    HttpResponseMessage response = await client.GetAsync(url);
    await HandleResponse(response);

    using var memoryStream = await CopyContentToMemoryStream(response);

    return await UploadStreamToS3(memoryStream);
  }
  // Static constructor for initializing static resources once, when the class is first loaded
  static HandlerFunction()
  {

    client = new HttpClient();
    s3Client = new AmazonS3Client();
  }

  // Actual instance constructor
  public HandlerFunction()
  {
    if (!string.IsNullOrEmpty(KagglePersonalUser) && !string.IsNullOrEmpty(KagglePersonalKey))
    {
      string credentials = $"{KagglePersonalUser}:{KagglePersonalKey}";
      string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
    }
    else
    {
      Console.WriteLine("Instance constructor: Environment variables for Kaggle are missing.");
      throw new Exception("Environment variables for `KagglePersonalUser` or `KagglePersonalKey` are missing.");
    }
  }

  private static async Task HandleResponse(HttpResponseMessage response)
  {
    if (response.IsSuccessStatusCode) return;

    string errorContent = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"Error Response Content: {errorContent}");

    throw new HttpRequestException($"Request to Kaggle API failed with status code {response.StatusCode} for URL: {response.RequestMessage.RequestUri}. Error Content: {errorContent}");
  }

  private static async Task<MemoryStream> CopyContentToMemoryStream(HttpResponseMessage response)
  {
    var memoryStream = new MemoryStream();
    await response.Content.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    return memoryStream;
  }
  private async Task<string> UploadStreamToS3(Stream stream)
  {

    try
    {
      var request = new PutObjectRequest
      {
        BucketName = S3BucketName,
        Key = S3FileKey,
        InputStream = stream,
        ContentType = "application/zip"
      };

      PutObjectResponse response = await s3Client.PutObjectAsync(request);
      Console.WriteLine($"Successfully uploaded file to S3 with HTTP status code: {response.HttpStatusCode}");
      return $"Successfully uploaded dataset: '{KaggleDatasetSlug}' to S3 bucket: '{S3BucketName}' with the file key: '{S3FileKey}'.";
    }
    catch (AmazonS3Exception ex)
    {
      Console.WriteLine($"Error uploading file to S3: {ex.Message}");
      throw;
    }
  }
}