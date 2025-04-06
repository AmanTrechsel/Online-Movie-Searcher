using System.Collections.ObjectModel;
using Online_Movie_Searcher.Classes.Movie;
using Online_Movie_Searcher.Services;
using Online_Movie_Searcher.View;

namespace Online_Movie_Searcher
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<MovieSearchResult> _allMovies = new();
        private int _currentPage = 1;
        private string _currentSearchTerm = "";
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private double _currentScrollPosition = 0;


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
                _currentSearchTerm = searchTerm;
                _currentPage = 1;
                _allMovies.Clear();
                string apiKey = await MovieService.GetKey();
                List<MovieSearchResult> movies = await MovieService.GetMovieDataAsync(apiKey, searchTerm, _currentPage);

                foreach (var movie in movies)
                    _allMovies.Add(movie);

                MovieCollectionView.ItemsSource = _allMovies;
                LoadMoreButton.IsVisible = movies.Count == 10;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", "Zoeken mislukt: " + ex.Message, "OK");
            }
        }

        private async void LoadMoreResults(object sender, EventArgs e)
        {
            // Avoid double clicks
            if (!_semaphore.Wait(0)) {
                return;
            }

            try
            {
                _currentScrollPosition = ScrollView.ScrollY;
                _currentPage++;
                LoadMoreButton.IsEnabled = false;

                string apiKey = await MovieService.GetKey();

                // Use tpl to load without blocking the ui
                var newMovies = await Task.Run(() =>
                    MovieService.GetMovieDataAsync(apiKey, _currentSearchTerm, _currentPage)
                );

                foreach (var movie in newMovies)
                    _allMovies.Add(movie);

                LoadMoreButton.IsVisible = newMovies.Count == 10;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", "Laden mislukt: " + ex.Message, "OK");
            }
            finally
            {
                _semaphore.Release();
                LoadMoreButton.IsEnabled = true;
                ScrollView.ScrollToAsync(0, _currentScrollPosition, false);
            }
        }


        private void ShowSearchResults(List<MovieSearchResult> movies)
        {
            MovieCollectionView.ItemsSource = movies;
        }

        private async void OnMovieTapped(object sender, EventArgs e)
        {
            string imdbID = ((TapGestureRecognizer)((Frame)sender).GestureRecognizers[0]).CommandParameter.ToString();
            await Navigation.PushAsync(new MovieDetailPage(imdbID));
        }
    }
}
