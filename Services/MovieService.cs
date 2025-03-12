using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<string> GetMovieJSON(string key, string movie_id = "tt3896198")
        {
            // Build uri using parameters.
            string uri = $"{REQUEST_URI}?i={movie_id}&apikey={key}";

            // Request, if not success throws error.
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            // Read GET data.
            return await response.Content.ReadAsStringAsync();
        }
    }
}
