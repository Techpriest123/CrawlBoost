using Aspose.Html;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using HtmlAgilityPack;
using System;
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

            MainScreen.IsVisible = false;
            SecondScreen.IsVisible = true;

            LoadingImage.IsVisible = true;


            GetWebsite(sender, e);

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
        }

        private void GetWebsite(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SecondWindow.Text = "Prossesing";
            if (URL.Text == null)
            {
                SecondWindow.Text = "Invaild URL";
                return;
            }
            if (!Parser.ValidateUrl(URL.Text))
            {
                SecondWindow.Text = "Invalid URL";
                return;
            }
            try
            {
                using (var document = new HTMLDocument(Parser.FormatUrl(URL.Text)))
                {
                    var html = new HtmlDocument();
                    html.LoadHtml(document.DocumentElement.OuterHTML);
                    int[] metrics = Parser.GetMetrics(html, URL.Text);
                    SecondWindow.Text = "On Page GEO: " + metrics[0] + "\n" +
                                       "Performance: " + metrics[3] + "\n";
                    LoadingImage.IsVisible = false;
                }
            }
            catch (Exception exception)
            {
                SecondWindow.Text = exception.ToString();
            }

        }
    }
}