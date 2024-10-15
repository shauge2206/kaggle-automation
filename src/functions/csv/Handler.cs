using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using src.services;
using Amazon.S3.Model;
using src.functions.utils;
using System.Text.RegularExpressions;

namespace src.functions.csv
{
  public class HandlerFunction
  {
    private static readonly string ZipFileExtract = Environment.GetEnvironmentVariable("ORIGINAL_FILE_NAME");
    private static readonly string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
    private static readonly string InputFileName;
    private static readonly string OutputFileName;
    private static readonly string newCsvFileName;

    // Static constructor for initializing static resources once, when the class is first loaded
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
      MemoryStream modifiedCsvStream = await ModifyCsv(extractedZipFileStream);
      MemoryStream outputFile = await Utils.CompressStreamToZip(modifiedCsvStream, newCsvFileName);

      return await Aws.PutFileS3(outputFile, OutputFileName);
    }



    public async Task<MemoryStream> ModifyCsv(MemoryStream stream)
    {
      stream.Position = 0;

      using StreamReader reader = new(stream);
      using CsvReader csvReader = new(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

      List<dynamic> movies = csvReader.GetRecords<dynamic>().ToList();

      foreach (var row in movies)
      {
        row.Runtime = row.Runtime.ToString().Replace(" min", "").Trim();
        row.Runtime = EnsureThreeDigitRuntime(row.Runtime);

        row.Gross = row.Gross.Replace(",", "");
        row.Gross = EnsureIntegerOrNull(row.Gross);

        row.No_of_Votes = EnsureIntegerOrNull(row.No_of_Votes);

        row.Meta_score = EnsureIntegerOrNull(row.Meta_score);

        if (!Regex.IsMatch(row.Released_Year, @"^(19|20)\d{2}$"))
        {
          row.Released_Year = null;
        }

        row.IMDB_Rating = EnsureDoubleOrNull(row.IMDB_Rating);
      }

      MemoryStream modifiedStream = new();
      using StreamWriter writer = new(modifiedStream, Encoding.UTF8, leaveOpen: true);
      using CsvWriter csvWriter = new(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));
      csvWriter.WriteRecords(movies);

      await writer.FlushAsync();

      modifiedStream.Position = 0;

      return modifiedStream;
    }


    // nullable double
    public static double? EnsureDoubleOrNull(string input)
    {
      if (double.TryParse(input, out double parsedValue))
      {
        return parsedValue;
      }
      else
      {
        return null;
      }
    }

    //nullable int
    public static int? EnsureThreeDigitRuntime(object input)
    {
      if (input is string runtimeString && Regex.IsMatch(runtimeString, @"^\d{3}$"))
      {
        return int.Parse(runtimeString); // Convert the valid 3-digit string to an integer.
      }

      return null;
    }

    //nullable int
    public static int? EnsureIntegerOrNull(string input)
    {
      if (int.TryParse(input, out int result))
      {
        return result;
      }
      else
      {
        return null;
      }


      /*
      Poster_Link
      Series_Title
      Released_Year
      Certificate
      Runtime
      Genre
      IMDB_Rating
      Overview
      Meta_score
      Director
      Star1
      Star2
      Star3
      Star4
      No_of_Votes
      Gross
      */

    }
  }
}