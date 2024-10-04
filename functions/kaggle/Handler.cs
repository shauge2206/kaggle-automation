using System.Net.Http.Headers;
using System.Text;
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AwsDotnetCsharp;
public class HandlerFunction
{
  private static readonly string DatasetSlug = Environment.GetEnvironmentVariable("KAGGLE_USER");
  private static readonly string UserSlug = Environment.GetEnvironmentVariable("KAGGLE_DATASET");
  private static readonly string BaseUrl = Environment.GetEnvironmentVariable("KAGGLE_BASE_URL");
  public async Task<string> KaggleDownloadAsync()
  {

    string credentials = $"{username}:{key}";
    string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
    string url = $"{BaseUrl}/datasets/download/{UserSlug}/{DatasetSlug}";

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);


  }
}

/*
https://www.kaggle.com/api/v1/
/datasets/download/{ownerSlug}/{datasetSlug}

 string dataset = "https://www.kaggle.com/datasets/harshitshankhdhar/imdb-dataset-of-top-1000-movies-and-tv-shows";
*/


/*
    // Make the request to download the dataset
    try
    {
      HttpResponseMessage response = await client.GetAsync(url);

      if (response.IsSuccessStatusCode)
      {
        // Download the content to a local file
        string filePath = Path.Combine(Environment.CurrentDirectory, "dataset.zip");

        //Garabge disposal, automatically calls Dispose() on fileStream
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
          await response.Content.CopyToAsync(fileStream);
        }

        return $"Dataset successfully downloaded to {filePath}";
      }
      else
      {
        return $"Failed to download dataset. HTTP Status: {response.StatusCode}, Reason: {response.ReasonPhrase}";
      }
    }
    catch (Exception ex)
    {
      return $"An error occurred while downloading the dataset: {ex.Message}";
    }
*/