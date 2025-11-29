using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace CrawlBoost
{
    public partial class MainWindow : Window
    {
        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void OpenButton_Click(object? sender, RoutedEventArgs e)
        {
            OpenButton.IsEnabled = false;
            LoadingImage.IsVisible = true;

            try
            {
                MainScreen.IsVisible = false;
                SecondScreen.IsVisible = true;

                ProgressBarsContainer.IsVisible = false;
                ErrorMessageContainer.IsVisible = false;
                BackButton.IsVisible = false;
                SchemaScore.IsVisible = false;
                AICitationProgress.IsVisible = false;

                SchemaGradeText.Text = "";
                AICitationGradeText.Text = "";

                await GetWebsiteAsync();
            }
            finally
            {
                LoadingImage.IsVisible = false;
            }
        }
        private void URL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenButton_Click(sender, e);
            }
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            SecondScreen.IsVisible = false;
            MainScreen.IsVisible = true;
            OpenButton.IsEnabled = true;
            SchemaScore.IsVisible = false;
            AICitationProgress.IsVisible = false;
            ProgressBarsContainer.IsVisible = false;
            ErrorMessageContainer.IsVisible = false;
            BackButton.IsVisible = false;
            SchemaGradeText.Text = "";
            AICitationGradeText.Text = "";
        }

        private async Task GetWebsiteAsync()
        {

            if (string.IsNullOrWhiteSpace(URL.Text))
            {
                ShowErrorMessage("Please enter a URL to analyze.");
                return;
            }

            var url = URL.Text.Trim();

            if (!WebsiteParser.ValidateUrl(url))
            {
                ShowErrorMessage("Invalid URL format. Please enter a valid URL starting with http:// or https://");
                return;
            }

            try
            {
                var formattedUrl = WebsiteParser.FormatUrl(url);


                var parser = new WebsiteParser();
                int metrics = await parser.GetMetricsAsync(formattedUrl);

                string grade = ConvertToLetterGrade(metrics);

                ProgressBarsContainer.IsVisible = true;
                SchemaScore.IsVisible = true;
                AICitationProgress.IsVisible = true;

                ProgressBarsContainer.IsVisible = true;
                SchemaScore.IsVisible = true;
                AICitationProgress.IsVisible = true;

                SchemaScore.Value = metrics;
                SchemaGradeText.Text = grade;

                AICitationProgress.Value = 0;
                AICitationGradeText.Text = "";

                BackButton.IsVisible = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error analyzing website: {ex.Message}");

                BackButton.IsVisible = true;
            }
        }

        private string ConvertToLetterGrade(int percentage)
        {
            return percentage >= 80 ? "A" :
                   percentage >= 60 ? "B" :
                   percentage >= 40 ? "C" :
                   percentage >= 20 ? "D" : "F";
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageContainer.IsVisible = true;
            ProgressBarsContainer.IsVisible = false;
            SchemaScore.IsVisible = false;
            AICitationProgress.IsVisible = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }
}