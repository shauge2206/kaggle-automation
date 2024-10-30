

using System.Text.Json;
using Amazon.S3.Model;
using Npgsql;
using src.functions.utils;
using src.models;
using src.services;

namespace src.functions.heroku;
public class HandlerFunction
{
    private static readonly string BaseFileKey = Environment.GetEnvironmentVariable("BASE_UPLOAD_FILE_KEY");
    private static readonly string ZipFileExtract = Environment.GetEnvironmentVariable("ORIGINAL_FILE_NAME");
    private static readonly string InputFileName;
    private static readonly string csvFileName;
    private static string CopyCommand;
    static HandlerFunction()
    {
        InputFileName = $"{BaseFileKey}-cleaned.zip";
        csvFileName = $"cleaned-{ZipFileExtract}";
        CopyCommand = @"COPY movie (poster_link, title, released_year, certificate, runtime, genre, 
                        imdb_rating, overview, meta_score, director, actor1, actor2, actor3, actor4, 
                        number_of_votes, gross_income) FROM STDIN WITH (FORMAT csv, HEADER true)";
    }
    public async Task<string> HerokuDataInsert()
    {

        GetObjectResponse s3Response = await Aws.GetFileS3(InputFileName);

        MemoryStream s3ZipAsStream = await Utils.S3ToMemoryStream(s3Response);
        MemoryStream csv = await Utils.ExtractZipToStream(s3ZipAsStream, csvFileName);

        string jsonString = await Aws.GetSSM();
        DatabaseConfig config = JsonSerializer.Deserialize<DatabaseConfig>(jsonString);

        NpgsqlConnection connection = new(config.GetConnectionString());
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {

            await ClearMovieTable(connection, transaction);

            await ImportCsvToDatabase(connection, CopyCommand, csv);

            await transaction.CommitAsync();

            return "Data successfully inserted!";
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    public static async Task ImportCsvToDatabase(NpgsqlConnection connection, string copyCommand, MemoryStream csv)
    {
        await using var writer = connection.BeginTextImport(copyCommand);
        using var reader = new StreamReader(csv);

        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            await writer.WriteLineAsync(line);
        }
    }

    private static async Task ClearMovieTable(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        using var deleteCommand = new NpgsqlCommand("DELETE FROM movie", connection, transaction);
        await deleteCommand.ExecuteNonQueryAsync();
    }

}