using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private SearchHistory searchHistory = new();
        private int _results = 0;

        private ObservableCollection<MovieSearchResult> _filteredMovies = new();
        private int _minAvailableYear = 1900;
        private int _maxAvailableYear = DateTime.Now.Year;

        public MainPage()
        {
            InitializeComponent();
            SearchHistoryList.ItemsSource = searchHistory.GetSearchHistory();
        }

        private void SearchBar_Focused(object sender, FocusEventArgs e)
        {
            SearchHistoryList.IsVisible = searchHistory.GetSearchHistory().Any();
        }

        private void SearchBar_Unfocused(object sender, FocusEventArgs e)
        {
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
                _results = 0;
                ActivityIndicatorLayout.IsVisible = true;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                _currentSearchTerm = searchTerm;
                _currentPage = 10;
                _allMovies.Clear();

                string apiKey = await MovieService.GetKey();

                // Create 10 parallel tasks to fetch pages 1/10
                var fetchTasks = Enumerable.Range(1, 10)
                    .Select(page => MovieService.GetMovieDataAsync(apiKey, searchTerm, page, _currentSortOption, suppressNotFound: true))
                    .ToArray();

                var allResults = await Task.WhenAll(fetchTasks);

                // Merge results into a single list
                var mergeList = allResults
                    .SelectMany(m => m)
                    .AsParallel();

                // Add movies to the _allMovies collection
                foreach (var movie in mergeList)
                {
                    _allMovies.Add(movie);
                }

                var yearList = _allMovies
                    .Where(m => int.TryParse(m.Year, out _))
                    .Select(m => int.Parse(m.Year))
                    .ToList();

                if (yearList.Any())
                {
                    _minAvailableYear = yearList.Min();
                    _maxAvailableYear = yearList.Max();

                    MinYearEntry.Text = _minAvailableYear.ToString();
                    MaxYearEntry.Text = _maxAvailableYear.ToString();
                    YearFilterLayout.IsVisible = true;
                }

                ApplyYearFilter();

                _results = _allMovies.Count;

                // Show load more button only if there are more results to load
                LoadMoreButton.IsVisible = _results > 0 && _results % 10 == 0;


                if (_results == 0)
                {
                    await DisplayAlert("No results", "No movies found for your search.", "OK");
                }

                HandleSearchHistory(searchTerm);

                timer.Stop();
                ElapsedLabel.Text = $"Found {_results} results in {Math.Round(timer.Elapsed.TotalMilliseconds)}ms";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Search failed: " + ex.Message, "OK");
            }
            finally
            {
                ActivityIndicatorLayout.IsVisible = false;
            }
        }

        private async void LoadMoreResults(object sender, EventArgs e)
        {
            if (!_semaphore.Wait(0))
                return;

            try
            {
                ActivityIndicatorLayout.IsVisible = true;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                _currentScrollPosition = ScrollView.ScrollY;
                _currentPage++;
                LoadMoreButton.IsEnabled = false;

                string apiKey = await MovieService.GetKey();
                var newMovies = await Task.Run(() =>
                    MovieService.GetMovieDataAsync(apiKey, _currentSearchTerm, _currentPage, _currentSortOption)
                );

                foreach (var movie in newMovies)
                {
                    _allMovies.Add(movie);
                }

                _results += newMovies.Count;
                LoadMoreButton.IsVisible = newMovies.Count == 10;
                timer.Stop();
                ElapsedLabel.Text = $"Found {_results} results in {Math.Round(timer.Elapsed.TotalMilliseconds)}ms";
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
                ActivityIndicatorLayout.IsVisible = false;
            }
        }

        private void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SortPicker.SelectedItem is string selectedSort)
            {
                _currentSortOption = selectedSort;

                if (_allMovies.Any())
                {
                    var sorted = _currentSortOption switch
                    {
                        "Year" => _allMovies.AsParallel().OrderBy(m => m.Year),
                        _ => _allMovies.AsParallel().OrderBy(m => m.Title)
                    };

                    _allMovies = new ObservableCollection<MovieSearchResult>(sorted.ToList());
                    ApplyYearFilter();
                }
            }
        }

        private async void OnMovieTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame &&
                frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tapGesture &&
                tapGesture.CommandParameter is string imdbID)
            {
                ActivityIndicatorLayout.IsVisible = true;
                await Navigation.PushAsync(new MovieDetailPage(imdbID));
                ActivityIndicatorLayout.IsVisible = false;
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

        private void OnYearFilterChanged(object sender, TextChangedEventArgs e)
        {
            if ((MinYearEntry.Text?.Length ?? 0) >= 4 && (MaxYearEntry.Text?.Length ?? 0) >= 4)
            {
                ApplyYearFilter();
            }
        }

        private void ApplyYearFilter()
        {
            if (!_allMovies.Any())
            {
                return;
            }

            bool minParsed = int.TryParse(MinYearEntry.Text, out int minYear);
            bool maxParsed = int.TryParse(MaxYearEntry.Text, out int maxYear);

            if (!minParsed)
            {
                minYear = _minAvailableYear;
            }
            if (!maxParsed)
            {
                maxYear = _maxAvailableYear;
            }

            var filtered = _allMovies
                .Where(m => int.TryParse(m.Year, out int year) && year >= minYear && year <= maxYear)
                .ToList();

            _filteredMovies = new ObservableCollection<MovieSearchResult>(filtered);
            MovieCollectionView.ItemsSource = _filteredMovies;
        }

        public void HandleSearchHistory(string searchTerm)
        {
            searchHistory.AddSearchToHistory(searchTerm);
            
            SearchHistoryList.ItemsSource = null;
            SearchHistoryList.ItemsSource = searchHistory.GetSearchHistory();
            SearchHistoryList.IsVisible = false;
        }
    }
}
