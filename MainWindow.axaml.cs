using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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

            H1TagsIcon.Text = "✗";
            H1TagsIcon.Foreground = Brushes.Red;
            OtherTagsIcon.Text = "✗";
            OtherTagsIcon.Foreground = Brushes.Red;
            DescriptionIcon.Text = "✗";
            DescriptionIcon.Foreground = Brushes.Red;
            TitleIcon.Text = "✗";
            TitleIcon.Foreground = Brushes.Red;
            ContentIcon.Text = "✗";
            ContentIcon.Foreground = Brushes.Red;
            NoAltIcon.Text = "✗";
            NoAltIcon.Foreground = Brushes.Red;
            NoCanonicalIcon.Text = "✗";
            NoCanonicalIcon.Foreground = Brushes.Red;
            NoindexIcon.Text = "✗";
            NoindexIcon.Foreground = Brushes.Red;
            JsonLdIcon.Text = "✗";
            JsonLdIcon.Foreground = Brushes.Red;
            RobotsIcon.Text = "✗";
            RobotsIcon.Foreground = Brushes.Red;


            try
            {
                MainScreen.IsVisible = false;
                SecondScreen.IsVisible = true;

                ProgressBarsContainer.IsVisible = false;
                AverageScoreContainer.IsVisible = false;
                ErrorMessageContainer.IsVisible = false;
                BackButton.IsVisible = false;
                SchemaScore.IsVisible = false;
                AICitationProgress.IsVisible = false;
                AverageScoreProgress.IsVisible = false;
                AuthorityProgress.IsVisible = false;


                SchemaGradeText.Text = "";
                AICitationGradeText.Text = "";
                AverageGradeText.Text = "";
                AuthorityGradeText.Text = "";
                SchemaGradeValue.Text = "";
                AICitationGradeValue.Text = "";
                AverageGradeValue.Text = "";
                AuthorityGradeValue.Text = "";

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
            AverageScoreProgress.IsVisible = false;
            ProgressBarsContainer.IsVisible = false;
            AverageScoreContainer.IsVisible = false;
            ErrorMessageContainer.IsVisible = false;
            BackButton.IsVisible = false;
            AuthorityProgress.IsVisible = false;
            SchemaGradeText.Text = "";
            AICitationGradeText.Text = "";
            AverageGradeText.Text = "";
            AuthorityGradeText.Text = "";
            SchemaGradeValue.Text = "";
            AICitationGradeValue.Text = "";
            AverageGradeValue.Text = "";
            AuthorityGradeValue.Text = "";
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
                (int onPageScore, int citationCount, double citationAuthority) = await parser.GetMetricsAsync(formattedUrl);

                int averageScore = (onPageScore + (int)(citationAuthority * 100) + (int)(citationCount / 25f * 100f)) / 3;
                string averageScoreGrade = ConvertToLetterGrade(averageScore);

                string onPageScoreGrade = ConvertToLetterGrade(onPageScore);

                ProgressBarsContainer.IsVisible = true;
                AverageScoreContainer.IsVisible = true;
                SchemaScore.IsVisible = true;
                AICitationProgress.IsVisible = true;
                AuthorityProgress.IsVisible = true;
                AverageScoreProgress.IsVisible = true;

                SchemaScore.Value = onPageScore;
                SchemaGradeText.Text = onPageScoreGrade;

                AICitationProgress.Value = citationCount;
                AICitationGradeText.Text = ConvertToLetterGrade((int)(citationCount / 25f * 100f));

                AuthorityProgress.Value = citationAuthority;
                AuthorityGradeText.Text = ConvertToLetterGrade((int)(citationAuthority * 100));
                AuthorityGradeValue.Text = ((int)(citationAuthority * 100)).ToString();

                AverageScoreProgress.Value = averageScore;
                AverageGradeText.Text = averageScoreGrade;
                DetailsListContainer.IsVisible = true;
                SchemaGradeValue.Text = onPageScore.ToString();
                AICitationGradeValue.Text = ((int)(citationCount / 25f * 100f)).ToString();
                AverageGradeValue.Text = averageScore.ToString();

                if (parser.h1Tags.Count == 1)
                {
                    H1TagsIcon.Text = "✓";
                    H1TagsIcon.Foreground = Brushes.Green;
                }
                if (parser.otherHTags.Count > 2)
                {
                    OtherTagsIcon.Text = "✓";
                    OtherTagsIcon.Foreground = Brushes.Green;
                }
                if (parser.metaDescription.Length <= 160 || parser.metaDescription.Length >= 120)
                {
                    DescriptionIcon.Text = "✓";
                    DescriptionIcon.Foreground = Brushes.Green;
                }
                if (parser.metaTitle.Length <= 60 || parser.metaTitle.Length >= 50)
                {
                    TitleIcon.Text = "✓";
                    TitleIcon.Foreground = Brushes.Green;
                }
                if (parser.wordCount > 300)
                {
                    ContentIcon.Text = "✓";
                    ContentIcon.Foreground = Brushes.Green;
                }
                if (!parser.imageWithNoAlt)
                {
                    NoAltIcon.Text = "✓";
                    NoAltIcon.Foreground = Brushes.Green;
                }
                if (!parser.noCanonicalTag)
                {
                    NoCanonicalIcon.Text = "✓";
                    NoCanonicalIcon.Foreground = Brushes.Green;
                }
                if (!parser.noindexTag)
                {
                    NoindexIcon.Text = "✓";
                    NoindexIcon.Foreground = Brushes.Green;
                }
                if (parser.hasJsonLd)
                {
                    JsonLdIcon.Text = "✓";
                    JsonLdIcon.Foreground = Brushes.Green;
                }
                if (parser.hasRobots)
                {
                    RobotsIcon.Text = "✓";
                    RobotsIcon.Foreground = Brushes.Green;
                }

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
            AverageScoreContainer.IsVisible = false;
            SchemaScore.IsVisible = false;
            AICitationProgress.IsVisible = false;
            AverageScoreProgress.IsVisible = false;
            AuthorityProgress.IsVisible = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }
}