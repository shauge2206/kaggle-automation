using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaggleAutomation.src.models
{
    public class Movie
    {
        public string Poster_Link { get; set; }
        public string Series_Title { get; set; }
        public string Released_Year { get; set; }
        public string Certificate { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string IMDB_Rating { get; set; }
        public string Overview { get; set; }
        public string Meta_score { get; set; }
        public string Director { get; set; }
        public string Star1 { get; set; }
        public string Star2 { get; set; }
        public string Star3 { get; set; }
        public string Star4 { get; set; }
        public string No_of_Votes { get; set; }
        public string Gross { get; set; }
    }
}