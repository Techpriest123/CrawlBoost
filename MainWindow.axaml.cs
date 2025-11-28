using Aspose.Html;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace CrawlBoost
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenButton_Click(object? sender, RoutedEventArgs e)
        {
            OpenButton.IsEnabled = false;
            LoadingBar.IsVisible = true;

            await Task.Delay(2000);

            MainScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            SecondScreen.IsVisible = false;
            MainScreen.IsVisible = true;

            LoadingBar.IsVisible = false;
            OpenButton.IsEnabled = true;
        }
    }

    private void GetWebsite(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        myTextBlock.Text = "Prossesing";
        if (URL.Text == null)
        {
            myTextBlock.Text = "Invalid url";
            return;
        }
        if (!Parser.ValidateUrl(URL.Text))
        {
            myTextBlock.Text = "Invalid url";
            return;
        }
        try
        {
            using (var document = new HTMLDocument(Parser.FormatUrl(URL.Text)))
            {
            var html = new HtmlDocument();
            html.LoadHtml(document.DocumentElement.OuterHTML);
            int[] metrics = Parser.GetMetrics(html, URL.Text);
            myTextBlock.Text = "On Page GEO: " + metrics[0] + "\n" +
                               "Links: " + metrics[1] + "\n" +
                               "Usability: " + metrics[2] + "\n" +
                               "Performance: " + metrics[3] + "\n" +
                               "Social: " + metrics[4] + "\n";
            }
        }
        catch (Exception exception)
        {
            myTextBlock.Text = exception.ToString();
        }
    }
}
