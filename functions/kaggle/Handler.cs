using System.Net.Http.Headers;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace functions.kaggle;
public class HandlerFunction
{
  private static readonly string S3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET");
  private static readonly string KaggleUserSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET_USER");
  private static readonly string KaggleDatasetSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET");
  private static readonly string BaseUrl = Environment.GetEnvironmentVariable("KAGGLE_BASE_URL");
  private static readonly string KagglePersonalUser = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_USERNAME");
  private static readonly string KagglePersonalKey = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_KEY");
  private static readonly string OutputFileName;
  private static readonly HttpClient client;
  private static readonly AmazonS3Client s3Client;

  // Static constructor for initializing static resources once, when the class is first loaded
  static HandlerFunction()
  {
    string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
    OutputFileName = $"{BaseFileKey}.zip";
    client = new HttpClient();
    s3Client = new AmazonS3Client();
  }


  // Actual instance constructor
  public HandlerFunction()
  {
    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{KagglePersonalUser}:{KagglePersonalKey}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
  }


  public async Task<string> KaggleDownload()
  {
    string url = $"{BaseUrl}/datasets/download/{KaggleUserSlug}/{KaggleDatasetSlug}";
    HttpResponseMessage response = await client.GetAsync(url);
    using MemoryStream memoryStream = await CopyReponseToMemoryStream(response);

    return await PutFileS3(memoryStream, OutputFileName);
  }


  private static async Task<MemoryStream> CopyReponseToMemoryStream(HttpResponseMessage response)
  {
    MemoryStream memoryStream = new();
    await response.Content.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    return memoryStream;
  }


  private async Task<string> PutFileS3(Stream stream, string fileName)
  {
    var request = new PutObjectRequest
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
}