using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Online_Movie_Searcher.Classes.Movie
{
    internal class Movie
    {
        private string title;
        private string year;
        private string rating;
        private string runtime;
        private string actors;
        private string plot;
        private string poster;

        public static Movie FromJSON(string json)
        {
            JsonObject jsonObject = JsonNode.Parse(json).AsObject();
   
            Movie movie = new Movie(
                jsonObject["Title"]?.ToString() ?? "Onbekend",
                jsonObject["Year"]?.ToString() ?? "Onbekend",
                jsonObject["imdbRating"]?.ToString() ?? "Onbekend",
                jsonObject["Runtime"]?.ToString() ?? "Onbekend",
                jsonObject["Actors"]?.ToString() ?? "Onbekend",
                jsonObject["Plot"]?.ToString() ?? "Geen plot beschikbaar",
                jsonObject["Poster"]?.ToString() ?? "Onbekend"
            );

            return movie;
        }

        public Movie(string title, string year, string rating, string runtime, string actors, string plot, string poster)
        {
            this.title = title;
            this.year = year;
            this.rating = rating;
            this.runtime = runtime;
            this.actors = actors;
            this.plot = plot;
            this.poster = poster;
        }

        public string GetTitle()
        {
            return this.title;
        }

        public void SetTitle(string title)
        {
            this.title = title;
        }

        public string GetYear()
        {
            return this.year;
        }

        public void SetYear(string year)
        {
            this.year = year;
        }

        public string GetRating()
        {
            return this.rating;
        }

        public void SetRating(string rating)
        {
            this.rating = rating;
        }

        public string GetRuntime()
        {
            return this.runtime;
        }

        public void SetRuntime(string runtime)
        {
            this.runtime = runtime;
        }

        public string GetActors()
        {
            return this.actors;
        }

        public void SetActors(string actors)
        {
            this.actors = actors;
        }

        public string GetPlot()
        {
            return this.plot;
        }

        public void SetPlot(string plot)
        {
            this.plot = plot;
        }

        public string GetPoster()
        {
            return this.poster;
        }

        public void SetPoster(string poster)
        {
            this.poster = poster;
        }
    }
}
