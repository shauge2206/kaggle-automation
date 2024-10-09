using System;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO.Compression;

namespace src.functions.csv
{
  public class HandlerFunction
  {
    private static readonly string S3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET");
    private static readonly string ZipFileExtract = Environment.GetEnvironmentVariable("ORIGINAL_FILE_NAME");
    private static readonly string InputFileName;
    private static readonly string OutputFileName;
    private static readonly string newCsvFileName;
    private static readonly AmazonS3Client s3Client;

    // Static constructor for initializing static resources once, when the class is first loaded
    static HandlerFunction()
    {
      string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
      InputFileName = $"{BaseFileKey}.zip";
      OutputFileName = $"{BaseFileKey}-cleaned.zip";
      newCsvFileName = $"cleaned-{ZipFileExtract}";
      s3Client = new AmazonS3Client();
    }


    public async Task<string> DataCleansing()
    {
      GetObjectResponse s3Response = await GetFileS3(InputFileName);

      MemoryStream s3ZipAsStream = await S3ToMemoryStream(s3Response);
      MemoryStream extractedZipFileStream = await ExtractZipToStream(s3ZipAsStream, ZipFileExtract);
      MemoryStream modifiedCsvStream = await ModifyCsv(extractedZipFileStream);
      MemoryStream outputFile = await CompressStreamToZip(modifiedCsvStream, newCsvFileName);

      return await PutFileS3(outputFile, OutputFileName);
    }

    
    // Proabably able to combine with CopyContent.. method in kaggle Lambda
    public async Task<MemoryStream> S3ToMemoryStream(GetObjectResponse response)
    {
      MemoryStream memoryStream = new();
      await response.ResponseStream.CopyToAsync(memoryStream);
      memoryStream.Position = 0;
      return memoryStream;
    }


    public async Task<MemoryStream> ExtractZipToStream(Stream zipStream, string fileToBeExtracted)
    {
      zipStream.Position = 0;
      MemoryStream extractedFileStream = new();

      using (ZipArchive archive = new(zipStream, ZipArchiveMode.Read, leaveOpen: true))
      {
        var entry = archive.GetEntry(fileToBeExtracted) ?? throw new FileNotFoundException($"File '{ZipFileExtract}' not found in the ZIP archive.");

        using var entryStream = entry.Open();
        await entryStream.CopyToAsync(extractedFileStream);
      }

      extractedFileStream.Position = 0;
      return extractedFileStream;
    }


    private static async Task<MemoryStream> CompressStreamToZip(Stream inputStream, string csvFileName)
    {
      inputStream.Position = 0;
      MemoryStream zipStream = new();

      using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
      {
        var zipEntry = archive.CreateEntry(csvFileName, CompressionLevel.Optimal);
        await using var zipEntryStream = zipEntry.Open();
        await inputStream.CopyToAsync(zipEntryStream);
      }

      zipStream.Position = 0;
      return zipStream;
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
      }

      MemoryStream modifiedStream = new();
      using StreamWriter writer = new(modifiedStream, Encoding.UTF8, leaveOpen: true);
      using CsvWriter csvWriter = new(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));
      csvWriter.WriteRecords(movies);

      await writer.FlushAsync();

      modifiedStream.Position = 0;

      return modifiedStream;
    }


    // Move to service folder later
    public async Task<GetObjectResponse> GetFileS3(string fileName)
    {
      GetObjectRequest request = new() { BucketName = S3BucketName, Key = fileName };

      return await s3Client.GetObjectAsync(request);
    }


    // Move to service folder later
    private async Task<string> PutFileS3(Stream stream, string fileName)
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
  }
}