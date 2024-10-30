using System.Net.Http.Headers;
using System.Text;
using src.functions.utils;
using src.services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace src.functions.kaggle;
public class HandlerFunction
{
  private static readonly string KaggleUserSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET_USER");
  private static readonly string KaggleDatasetSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET");
  private static readonly string BaseUrl = Environment.GetEnvironmentVariable("KAGGLE_BASE_URL");
  private static readonly string KagglePersonalUser = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_USERNAME");
  private static readonly string KagglePersonalKey = Environment.GetEnvironmentVariable("KAGGLE_PERSONAL_KEY");
  private static readonly string OutputFileName;
  private static readonly HttpClient client;

  static HandlerFunction()
  {
    string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
    OutputFileName = $"{BaseFileKey}.zip";
    client = new HttpClient();
  }

  public HandlerFunction()
  {
    var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{KagglePersonalUser}:{KagglePersonalKey}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
  }


  public async Task<string> KaggleDownload()
  {
    string url = $"{BaseUrl}/datasets/download/{KaggleUserSlug}/{KaggleDatasetSlug}";
    HttpResponseMessage response = await client.GetAsync(url);
    using MemoryStream memoryStream = await Utils.CopyReponseToMemoryStream(response);

    return await Aws.PutFileS3(memoryStream, OutputFileName);
  }
}