using Aspose.Html;
using Avalonia.Controls;
using HtmlAgilityPack;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace CrawlBoost;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
                int[] metrics = Parser.GetMetrics(html);
                myTextBlock.Text = "On Page SEO: " + metrics[0] + "\n" +
                                   "Links: " + metrics[1] + "\n" +
                                   "Usability: " + metrics[2] + "\n" +
                                   "Performance: " + metrics[3] + "\n" +
                                   "Social: " + metrics[4] + "\n";
            }
        }
        catch
        {
            myTextBlock.Text = "Invalid URL";
        }
    }
}