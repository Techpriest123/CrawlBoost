using Aspose.Html;
using Avalonia.Controls;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace CrawlBoost
{
    internal static class Parser
    {
        internal static int onPageSEO = 100;
        internal static int links = 100;
        internal static int usability = 100;
        internal static int performance = 100;
        internal static int social = 100;

        private static List<HtmlNode> h1Tags = new();
        private static List<HtmlNode> otherHTags = new();
        private static string metaDescription = "";
        private static string metaTitle = "";
        private static bool containsLang = false;
        private static List<string> keywords = new();
        private static string content = "";
        private static bool imageWithNoAlt = false;
        private static bool noCanonicalTag = true;
        private static bool noindexTag = true;
        private static List<string> visitedLinks = new();

        internal static int[] GetMetrics(HtmlDocument html)
        {
            Parse(html);
            Validate();
            social = content.Split(" ").Count();
            return [onPageSEO, links, usability, performance, social];
        }

        internal static void Validate()
        {
            links -= containsLang ? 0 : 10;
            links -= imageWithNoAlt ? 0 : 10;
            links -= noCanonicalTag ? 10 : 0;
            links -= noindexTag ? 0 : 10;
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
                if (node.InnerText != null || node.InnerText != "")
                {
                    string text = node.InnerText;
                    Regex wordRegex = new Regex(
                    @"^\w+$", RegexOptions.IgnoreCase);
                    wordRegex.Matches(text);
                    if (wordRegex.IsMatch(text))
                    {
                        content += text + " ";
                    }
                }
                if (node.Name == "img" && node.GetAttributeValue("alt", "") == "") imageWithNoAlt = true;
                if (node.GetAttributeValue("rel", "") == "canonical") noCanonicalTag = false;
                
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

    }
}
