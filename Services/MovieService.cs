using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Online_Movie_Searcher.Classes.Movie;

namespace Online_Movie_Searcher.Services
{
    internal class MovieService
    {
        public const string REQUEST_URI = "http://www.omdbapi.com/";

        // Stores the API_KEY, only set when GetKey() is called.
        private static string api_key = "";

        // Retreives the API_KEY from resources if not already stored.
        public static async Task<string> GetKey()
        {
            // No api key is stored, retrieve it from resources.
            if (api_key.Length == 0)
            {
                // Retreive stream reader from API_KEY file in resources.
                Stream stream = await FileSystem.OpenAppPackageFileAsync("API_KEY");
                StreamReader reader = new StreamReader(stream);

                // Read file data and assign to api key.
                api_key = await reader.ReadToEndAsync();
            }

            return api_key;
        }

        public static async Task<List<MovieSearchResult>> GetMovieDataAsync(string key, string title)
        {
            // Build uri using parameters.
            string uri = $"{REQUEST_URI}?s={Uri.EscapeDataString(title)}&apikey={key}";
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            JsonObject jsonObject = JsonNode.Parse(json).AsObject();

            if (jsonObject["Response"]?.ToString() == "False")
                throw new Exception(jsonObject["Error"]?.ToString());

            var moviesJson = jsonObject["Search"].AsArray();

            var movies = moviesJson
                .Select(item => new MovieSearchResult
                {
                    Title = item["Title"]?.ToString() ?? "Onbekend",
                    Year = item["Year"]?.ToString() ?? "Onbekend",
                    imdbID = item["imdbID"]?.ToString(),
                    Poster = item["Poster"]?.ToString()
                })
                .OrderBy(m => m.Title)
                .ToList();


            return movies;
        }

        public static void GetMovieDetails(string key, string imdbID)
        {
            // Get full details of a single movie
        }
    }
}
