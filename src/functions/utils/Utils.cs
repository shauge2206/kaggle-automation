using System.IO.Compression;
using Amazon.S3.Model;

namespace src.functions.utils
{
    public class Utils
    {

        public static async Task<MemoryStream> CopyReponseToMemoryStream(HttpResponseMessage response)
        {
            MemoryStream memoryStream = new();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        public static async Task<MemoryStream> S3ToMemoryStream(GetObjectResponse response)
        {
            MemoryStream memoryStream = new();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }


        public static async Task<MemoryStream> ExtractZipToStream(Stream zipStream, string fileToBeExtracted)
        {
            zipStream.Position = 0;
            MemoryStream extractedFileStream = new();

            using (ZipArchive archive = new(zipStream, ZipArchiveMode.Read, leaveOpen: true))
            {
                var entry = archive.GetEntry(fileToBeExtracted) ?? throw new FileNotFoundException($"File '{fileToBeExtracted}' not found in the ZIP archive.");

                using var entryStream = entry.Open();
                await entryStream.CopyToAsync(extractedFileStream);
            }

            extractedFileStream.Position = 0;
            return extractedFileStream;
        }


        public static async Task<MemoryStream> CompressStreamToZip(Stream inputStream, string csvFileName)
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
    }
}