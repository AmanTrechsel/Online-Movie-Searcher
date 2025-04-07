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
        private string _currentSortOption = "Title";
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private double _currentScrollPosition = 0;
        private List<string> searchHistory = new();

        public MainPage()
        {
            InitializeComponent();
            SearchHistoryList.ItemsSource = searchHistory;
        }

        private void SearchBar_Focused(object sender, FocusEventArgs e)
        {
            // Show search history when focusing the search bar
            SearchHistoryList.IsVisible = searchHistory.Any();
        }

        private void SearchBar_Unfocused(object sender, FocusEventArgs e)
        {
            // Optionally hide search history when focus is lost
            SearchHistoryList.IsVisible = false;
        }

        private async void SearchMovies(object sender, EventArgs e)
        {
            string searchTerm = SearchEntry.Text?.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                await DisplayAlert("Error", "Please enter a movie title to search.", "OK");
                return;
            }

            try
            {
                _currentSearchTerm = searchTerm;
                _currentPage = 1;
                _allMovies.Clear();

                string apiKey = await MovieService.GetKey();
                List<MovieSearchResult> movies = await MovieService.GetMovieDataAsync(apiKey, searchTerm, _currentPage, _currentSortOption);

                foreach (var movie in movies)
                    _allMovies.Add(movie);

                MovieCollectionView.ItemsSource = _allMovies;
                LoadMoreButton.IsVisible = movies.Count == 10;

                if (!searchHistory.Contains(searchTerm))
                    searchHistory.Insert(0, searchTerm);

                SearchHistoryList.ItemsSource = null;
                SearchHistoryList.ItemsSource = searchHistory;
                SearchHistoryList.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Search failed: " + ex.Message, "OK");
            }
        }

        private async void LoadMoreResults(object sender, EventArgs e)
        {
            if (!_semaphore.Wait(0))
                return;

            try
            {
                _currentScrollPosition = ScrollView.ScrollY;
                _currentPage++;
                LoadMoreButton.IsEnabled = false;

                string apiKey = await MovieService.GetKey();
                var newMovies = await Task.Run(() =>
                    MovieService.GetMovieDataAsync(apiKey, _currentSearchTerm, _currentPage, _currentSortOption)
                );

                foreach (var movie in newMovies)
                    _allMovies.Add(movie);

                LoadMoreButton.IsVisible = newMovies.Count == 10;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Loading more results failed: " + ex.Message, "OK");
            }
            finally
            {
                _semaphore.Release();
                LoadMoreButton.IsEnabled = true;
                await ScrollView.ScrollToAsync(0, _currentScrollPosition, false);
            }
        }

        private async void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SortPicker.SelectedItem is string selectedSort)
            {
                _currentSortOption = selectedSort;

                if (!string.IsNullOrEmpty(_currentSearchTerm))
                {
                    _currentPage = 1;
                    _allMovies.Clear();

                    string apiKey = await MovieService.GetKey();
                    var movies = await MovieService.GetMovieDataAsync(apiKey, _currentSearchTerm, _currentPage, _currentSortOption);

                    foreach (var movie in movies)
                        _allMovies.Add(movie);

                    MovieCollectionView.ItemsSource = _allMovies;
                    LoadMoreButton.IsVisible = movies.Count == 10;
                }
            }
        }

        private async void OnMovieTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame &&
                frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tapGesture &&
                tapGesture.CommandParameter is string imdbID)
            {
                await Navigation.PushAsync(new MovieDetailPage(imdbID));
            }
        }

        private void SearchHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is string selectedQuery)
            {
                SearchEntry.Text = selectedQuery;
                SearchHistoryList.IsVisible = false;
            }
        }
    }
}
