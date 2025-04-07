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

        public static async Task<List<MovieSearchResult>> GetMovieDataAsync(string key, string title, int page = 1, string sortBy = "Title")
        {
            string uri = $"{REQUEST_URI}?s={Uri.EscapeDataString(title)}&apikey={key}&page={page}";
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
                    Title = item["Title"]?.ToString() ?? "Unknown",
                    Year = item["Year"]?.ToString() ?? "Unknown",
                    imdbID = item["imdbID"]?.ToString(),
                    Poster = item["Poster"]?.ToString()
                })
                .AsParallel();

            movies = sortBy switch
            {
                "Year" => movies.OrderBy(m => m.Year),
                _ => movies.OrderBy(m => m.Title)
            };

            return movies.ToList();



        public static async Task<Movie> GetMovieDetailsAsync(string key, string imdbID)
        {
            string uri = $"{REQUEST_URI}?i={imdbID}&apikey={key}";
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            return Movie.FromJSON(json);
        }
    }
}
