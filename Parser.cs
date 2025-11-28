using Aspose.Html;
using Avalonia.Controls;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace CrawlBoost
{
    internal static class Parser
    {
        internal static int onPageGEO = 100;
        internal static int links = 100;
        internal static int usability = 100;
        internal static int performance = 100;
        internal static int social = 100;

        internal static List<HtmlNode> h1Tags = new();
        internal static List<HtmlNode> otherHTags = new();
        internal static string metaDescription = "";
        internal static string metaTitle = "";
        internal static bool containsLang = false;
        internal static List<string> keywords = new();
        internal static string content = "";
        internal static bool imageWithNoAlt = false;
        internal static bool noCanonicalTag = true;
        internal static bool noindexTag = true;
        internal static List<string> visitedLinks = new();

        internal static int[] GetMetrics(HtmlDocument html, string url)
        {
            Parse(html);
            Validate(url);
            return [onPageGEO, links, usability, performance, social];
        }

        internal static void Validate(string url)
        {
            onPageGEO -= containsLang ? 0 : 11;
            onPageGEO -= imageWithNoAlt ? 0 : 11;
            onPageGEO -= noCanonicalTag ? 11 : 0;
            onPageGEO -= noindexTag ? 0 : 11;
            if (h1Tags.Count() > 1) onPageGEO -= 11;
            if (otherHTags.Count() < 1) onPageGEO -= 11;
            if (metaDescription.Length > 160 && metaDescription.Length < 120) onPageGEO -= 11;
            if (metaTitle.Length > 60 && metaTitle.Length < 50) onPageGEO -= 11;
            if (content.Split(" ").Length < 300) onPageGEO -= 11; 

            performance = 100 - LoadingSpeedValidation(FormatUrl(url));
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

        internal static bool ValidateUrl(string? url)
        {
            if (url == null || url == "") return false;
            var urlRegex = new Regex(
                @"(?:[a-zA-Z0-9]" +
                        @"(?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}" +
                        @"(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}" +
                        @"|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?" +
                        @"(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$",
                RegexOptions.IgnoreCase);

            urlRegex.Matches(url);

            return urlRegex.IsMatch(url);
        }

        internal static string FormatUrl(string url)
        {
            var urlRegex = new Regex(
                @"^(https?):\/\/(?:[a-zA-Z0-9]" +
                        @"(?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}" +
                        @"(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}" +
                        @"|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?" +
                        @"(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$",
                RegexOptions.IgnoreCase);
            urlRegex.Matches(url);
            if (!urlRegex.IsMatch(url))
            {
                url = "https://" + url;
            }
            return url;
        }

        internal static int LoadingSpeedValidation(string address)
        {
            System.Diagnostics.Stopwatch timer = new Stopwatch();

            timer.Start();

            new HttpClient().GetAsync(new Uri(address));

            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;

            return Math.Clamp((int)(timeTaken.Milliseconds * 0.02f), 0, 100);
        }
    }
}
