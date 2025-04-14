using Online_Movie_Searcher.Classes.Movie;
using Online_Movie_Searcher.Services;

namespace Online_Movie_Searcher.View
{
    public partial class MovieDetailPage : ContentPage
    {
        public MovieDetailPage(string imdbID)
        {
            InitializeComponent();
            LoadMovieDetails(imdbID);
        }

        private async void LoadMovieDetails(string imdbID)
        {
            try
            {
                ActivityIndicatorLayout.IsVisible = true;

                string apiKey = await MovieService.GetKey();
                Movie movie = await MovieService.GetMovieDetailsAsync(apiKey, imdbID);

                LabelTitle.Text = movie.GetTitle();
                LabelYear.Text = movie.GetYear();
                LabelRuntime.Text = movie.GetRuntime();
                LabelActors.Text = movie.GetActors();
                LabelPlot.Text = movie.GetPlot();
                LabelRating.Text = movie.GetRating();
                ImagePoster.Source = movie.GetPoster();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Fout", "Details laden mislukt: " + ex.Message, "OK");
            }
            finally
            {
                ActivityIndicatorLayout.IsVisible = false;
            }
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            ActivityIndicatorLayout.IsVisible = true;
            await Navigation.PopAsync();
            ActivityIndicatorLayout.IsVisible = false;
        }
    }
}