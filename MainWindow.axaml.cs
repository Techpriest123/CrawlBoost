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

            LoadingImage.IsVisible = false;
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
                    int metrics = Parser.GetMetrics(html, URL.Text);
                    SecondWindow.Text = "Рейтинг GEO: " + metrics;
                    LoadingImage.IsVisible = false;
                }
            }
            catch
            {
                SecondWindow.Text = "Invalid URL";
            }
            if (Parser.h1Tags.Count == 1)
            {
                ThirdWindow.Text = "Главных заголовков больше чем один, это ухудшает читаемость кода нейросеть";
            }
            if (Parser.otherHTags.Count < 1)
            {
                FourthWindow.Text = "Других заголовков меньше одного! Использвоание разных заголовков помогает нейросети лучше понять содержание";
            }
            if (Parser.metaDescription.Length > 160 || Parser.metaDescription.Length < 120)
            {
                FifthWindow.Text = "Тег с описанием должен быть не больше 160 символов, но не меньше 120";
            }
            if (Parser.metaTitle.Length > 60 || Parser.metaTitle.Length < 50) 
            {
                SixthWindow.Text = "Заголовок не должен быть больше 60 символов, но и не меньше 50";
            }
            if (!Parser.containsLang)
            {
                SeventhWindow.Text = "Не найдена пометка о языке, без нее нейросети сложнее найти сайт на нужном языке";
            }
            if (Parser.imageWithNoAlt)
            {
                EighthWindow.Text = "На сайте есть картинка без подписи";
            }
        if (Parser.noCanonicalTag)
            {
                NinethWindow.Text = "Google рекомендует, чтобы у сайта был у страницы был тег Canonical";
            }
        if (!Parser.hasRobots)
            {
                TenthWindow.Text = "Боты не могут читать сайт";
            }
            if (!Parser.hasJsonLd)
            {
                EleventhWindow.Text = "Сайт не предоставляет контекстных тегов для нейросетей";
            }
            {
                
            }
        }
    }
}