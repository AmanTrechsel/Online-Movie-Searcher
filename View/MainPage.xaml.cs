using Online_Movie_Searcher.Classes.Movie;
using Online_Movie_Searcher.Services;

namespace Online_Movie_Searcher
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        // Retreives a default movie using the API.
        private async void OnGetMovie(object sender, EventArgs e)
        {
            // Update label
            GetMovieBtn.Text = "Retreiving Movie";

            // Get JSON data of the default movie
            string movie_data = await MovieService.GetMovieJSON(await MovieService.GetKey());

            // Update label
            GetMovieBtn.Text = "Parsing Movie";

            // Create a movie object with the JSON data.
            Movie movie = Movie.FromJSON(movie_data);

            // Update label
            GetMovieBtn.Text = $"Movie: {movie.GetTitle()}";
        }
    }

}
