using KaggleAutomation.src.models;
using src.functions.utils;
using System.Text.RegularExpressions;

namespace KaggleAutomation.src.functions.utils
{
    public class CsvOperations
    {
        public static void ProcessRuntime(Movie row)
        {
            row.Runtime = row.Runtime.ToString().Replace(" min", "").Trim();
            row.Runtime = Utils.EnsureThreeDigitRuntime(row.Runtime);
            row.Runtime ??= CsvDefaults.AVG_RUNTIME;
        }

        public static void ProcessGross(Movie row)
        {
            row.Gross = row.Gross.Replace(",", "");
            row.Gross = Utils.EnsureOnlyDigitsOrNull(row.Gross);
            row.Gross ??= CsvDefaults.DEFAULT_MISSING_NUMBER_VALUE;
        }

        public static void ProcessNoOfVotes(Movie row)
        {
            row.No_of_Votes = Utils.EnsureOnlyDigitsOrNull(row.No_of_Votes);
            row.No_of_Votes ??= CsvDefaults.DEFAULT_MISSING_NUMBER_VALUE;
        }

        public static void ProcessMetaScore(Movie row)
        {
            row.Meta_score = Utils.EnsureOnlyDigitsOrNull(row.Meta_score);
            row.Meta_score ??= CsvDefaults.DEFAULT_MISSING_NUMBER_VALUE;
        }

        public static void ProcessReleasedYear(Movie row)
        {
            if (!Regex.IsMatch(row.Released_Year, @"^(19|20)\d{2}$"))
            {
                row.Released_Year = null;
            }

            row.Released_Year ??= CsvDefaults.AVG_RELEASEDYEAR;
        }

        public static void ProcessIMDBRating(Movie row)
        {
            row.IMDB_Rating = Utils.EnsureDoubleOrNull(row.IMDB_Rating);
            row.IMDB_Rating ??= CsvDefaults.AVG_IMDBRATING;
        }

        public static void ProcessCertificate(Movie row)
        {
            row.Certificate = string.IsNullOrWhiteSpace(row.Certificate)
                ? CsvDefaults.DEFAULT_MISSING_STRING_VALUE
                : row.Certificate.Trim();
        }
    }
}