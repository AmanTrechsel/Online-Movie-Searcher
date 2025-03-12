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

        public static Movie FromJSON(string json)
        {
            JsonObject jsonObject = JsonNode.Parse(json).AsObject();
            Movie movie = new Movie(jsonObject["Title"].ToString());
            return movie;
        }

        public Movie(string title)
        {
            this.title = title;
        }

        public string GetTitle()
        {
            return this.title;
        }

        public void SetTitle(string title)
        {
            this.title = title;
        }
    }
}
