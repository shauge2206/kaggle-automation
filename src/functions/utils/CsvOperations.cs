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
        }

        public static void ProcessGross(Movie row)
        {
            row.Gross = row.Gross.Replace(",", "");
            row.Gross = Utils.EnsureOnlyDigitsOrNull(row.Gross);
        }

        public static void ProcessNoOfVotes(Movie row)
        {
            row.No_of_Votes = Utils.EnsureOnlyDigitsOrNull(row.No_of_Votes);
        }

        public static void ProcessMetaScore(Movie row)
        {
            row.Meta_score = Utils.EnsureOnlyDigitsOrNull(row.Meta_score);
        }

        public static void ProcessReleasedYear(Movie row)
        {
            if (!Regex.IsMatch(row.Released_Year, @"^(19|20)\d{2}$"))
            {
                row.Released_Year = null;
            }
        }

        public static void ProcessIMDBRating(Movie row)
        {
            row.IMDB_Rating = Utils.EnsureDoubleOrNull(row.IMDB_Rating);
        }
    }
}