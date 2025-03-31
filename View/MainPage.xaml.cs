using System.Collections.ObjectModel;
using Online_Movie_Searcher.Classes.Movie;
using Online_Movie_Searcher.Services;

namespace Online_Movie_Searcher
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void SearchMovie(object sender, EventArgs e)
        {
            string searchTerm = SearchEntry.Text?.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                await DisplayAlert("Fout", "Voer een titel in om te zoeken.", "OK");
                return;
            }

            try
            {
                string apiKey = await MovieService.GetKey();
                List<MovieSearchResult> movies = await MovieService.GetMovieDataAsync(apiKey, searchTerm);
                ShowSearchResults(movies);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", "Zoeken mislukt: " + ex.Message, "OK");
            }
        }

        private void ShowSearchResults(List<MovieSearchResult> movies)
        {
            MovieCollectionView.ItemsSource = movies;
        }

    }
}
