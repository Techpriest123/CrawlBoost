using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml.MarkupExtensions;
using HtmlAgilityPack;

namespace CrawlBoost
{
    internal static partial class Parser
    {
        private const int margin = 10;

        internal static int onPageGEO = 100;

        internal static List<HtmlNode> h1Tags = [];
        internal static List<HtmlNode> otherHTags = [];
        internal static string metaDescription = "";
        internal static string metaTitle = "";
        internal static bool containsLang = false;
        internal static List<string> keywords = [];
        internal static string content = "";
        internal static bool imageWithNoAlt = false;
        internal static bool noCanonicalTag = true;
        internal static bool noindexTag = true;
        internal static bool hasJsonLd = false;

        internal static int GetMetrics(HtmlDocument html, string url)
        {
            Parse(html);
            Validate(url);
            return Math.Clamp(onPageGEO, 0, 100);
        }

        internal static void Validate(string url)
        {
            if (!containsLang) onPageGEO -= margin;
            if (imageWithNoAlt) onPageGEO -= margin;
            if (noCanonicalTag) onPageGEO -= margin;
            if (!noindexTag) onPageGEO -= margin;
            if (h1Tags.Count > 1) onPageGEO -= margin;
            if (otherHTags.Count < 1) onPageGEO -= margin;
            if (metaDescription.Length > 160 && metaDescription.Length < 120) onPageGEO -= margin;
            if (metaTitle.Length > 60 && metaTitle.Length < 50) onPageGEO -= margin;
            if (content.Split(" ").Length < 300) onPageGEO -= margin;
            if (ValidateRobots(url)) onPageGEO -= margin;
            if (!hasJsonLd) onPageGEO -= margin;
        }

        private static void Parse(HtmlDocument html)
        {
            foreach (var node in html.DocumentNode.Descendants())
            {
                if (node.Name == "head" && node.Attributes.Contains("lang")) containsLang = true;
                if (node.Name == "h1") h1Tags.Add(node);
                if (node.Name == "h2" ||
                    node.Name == "h3" ||
                    node.Name == "h4" ||
                    node.Name == "h5" ||
                    node.Name == "h6"
                    ) otherHTags.Add(node);
                if (node.Name == "meta") ParseMetaTag(node);
                if (node.Name == "img" && node.GetAttributeValue("alt", "") == "") imageWithNoAlt = true;
                if (node.GetAttributeValue("rel", "") == "canonical") noCanonicalTag = false;
                string text = node.InnerText;
                    Regex wordRegex = new Regex(
                    @"^\w+$", RegexOptions.IgnoreCase);
                    wordRegex.Matches(text);
                    if (wordRegex.IsMatch(text))
                    {
                        content += text + " ";
                    }
                if (node.Name == "script") hasJsonLd = node.GetAttributeValue("type", "").Contains("ld+json");
            }
        }

        private static void ParseMetaTag(HtmlNode meta)
        {
            foreach (var atr in meta.Attributes)
            {
                if (meta.GetAttributeValue("name", "") == "description") metaDescription = meta.GetAttributeValue("content", "");
                if (meta.GetAttributeValue("name", "") == "title" && metaTitle == "") metaTitle = meta.GetAttributeValue("content", "");
                if (meta.GetAttributeValue("name", "") == "keywords") keywords = meta.GetAttributeValue("content", "").Replace(",", "").Split(" ").ToList();
                if (meta.GetAttributeValue("name", "") == "robots") noindexTag = meta.GetAttributeValue("content", "") != "noindex";
            }
        }

        private static bool ValidateRobots(string url)
        {
            try
            {
                HttpWebRequest? request = WebRequest.Create(url + "/robots.txt") as HttpWebRequest ?? throw new();
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse ?? throw new();
                response.Close();
                string? robots = "";
                if (response.StatusCode == HttpStatusCode.OK) 
                {
                    using (var client = new WebClient())
                    {
                        robots = client.DownloadData(url + "/robots.txt").ToString();
                    }
                }
                return robots != null ? robots.Split("\n").Contains("Disallow: /") : throw new();
            }
            catch
            {
                return false;
            }
        }

        internal static bool ValidateUrl(string? url)
        {
            if (url == null || url == "") return false;
            var urlRegex = UrlRegex();

            urlRegex.Matches(url);

            return urlRegex.IsMatch(url);
        }

        internal static string FormatUrl(string url)
        {
            var urlRegex = UriRegex();
            urlRegex.Matches(url);
            if (!urlRegex.IsMatch(url))
            {
                url = "https://" + url;
            }
            return url;
        }

        [GeneratedRegex(@"^(https?):\/\/(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$", RegexOptions.IgnoreCase, "ru-RU")]
        private static partial Regex UriRegex();
        [GeneratedRegex(@"(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$", RegexOptions.IgnoreCase, "ru-RU")]
        private static partial Regex UrlRegex();
    }
}
