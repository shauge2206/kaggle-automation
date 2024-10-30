using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using src.services;
using Amazon.S3.Model;
using src.functions.utils;
using KaggleAutomation.src.models;
using KaggleAutomation.src.functions.utils;

namespace src.functions.csv
{
  public class HandlerFunction
  {
    private static readonly string ZipFileExtract = Environment.GetEnvironmentVariable("ORIGINAL_FILE_NAME");
    private static readonly string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
    private static readonly string InputFileName;
    private static readonly string OutputFileName;
    private static readonly string newCsvFileName;

    static HandlerFunction()
    {
      InputFileName = $"{BaseFileKey}.zip";
      OutputFileName = $"{BaseFileKey}-cleaned.zip";
      newCsvFileName = $"cleaned-{ZipFileExtract}";
    }

    public async Task<string> DataCleansing()
    {
      GetObjectResponse s3Response = await Aws.GetFileS3(InputFileName);
      MemoryStream s3ZipAsStream = await Utils.S3ToMemoryStream(s3Response);
      MemoryStream extractedZipFileStream = await Utils.ExtractZipToStream(s3ZipAsStream, ZipFileExtract);
      List<Movie> modifiedMovies = CleanMovies(extractedZipFileStream);
      MemoryStream modifiedStream = await MoviesToStream(modifiedMovies);
      MemoryStream outputFile = await Utils.CompressStreamToZip(modifiedStream, newCsvFileName);

      return await Aws.PutFileS3(outputFile, OutputFileName);
    }

    public static List<Movie> CleanMovies(MemoryStream stream)
    {
      stream.Position = 0;

      using StreamReader reader = new(stream);
      using CsvReader csvReader = new(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

      List<Movie> movies = csvReader.GetRecords<Movie>().ToList();

      foreach (var row in movies)
      {
        CsvOperations.ProcessRuntime(row);
        CsvOperations.ProcessGross(row);
        CsvOperations.ProcessNoOfVotes(row);
        CsvOperations.ProcessMetaScore(row);
        CsvOperations.ProcessReleasedYear(row);
        CsvOperations.ProcessIMDBRating(row);
      }

      return movies;
    }

    public static async Task<MemoryStream> MoviesToStream(List<Movie> movies)
    {
      MemoryStream modifiedStream = new();
      using StreamWriter writer = new(modifiedStream, Encoding.UTF8, leaveOpen: true);
      using CsvWriter csvWriter = new(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));
      csvWriter.WriteRecords(movies);

      await writer.FlushAsync();

      modifiedStream.Position = 0;

      return modifiedStream;
    }
  }
}