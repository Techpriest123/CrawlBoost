using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlBoost
{
    internal partial class WebsiteParser
    {
        private readonly string CacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), ".Cache");
        private const int WEIGHT_CRITICAL = 15;
        private const int WEIGHT_HIGH = 10;
        private const int WEIGHT_MEDIUM = 7;
        private const int WEIGHT_LOW = 5;
        private const int WEIGHT_MINOR = 3;

        private int _onPageGEO = 100;

        private List<HtmlNode> h1Tags = [];
        private List<HtmlNode> otherHTags = [];
        private string metaDescription = "";
        private string metaTitle = "";
        private bool containsLang = false;
        private List<string> keywords = [];
        private string content = "";
        private bool imageWithNoAlt = false;
        private int imagesWithAltCount = 0;
        private int imagesWithoutAltCount = 0;
        private bool noCanonicalTag = true;
        private bool noindexTag = true;
        private bool hasJsonLd = false;
        private bool hasRobots = true;
        private int wordCount = 0;
        private double textToHtmlRatio = 0;
        private int internalLinksCount = 0;
        private int externalLinksCount = 0;
        private bool hasViewportMeta = false;
        private double loadTimeMs = 0;

        internal async Task<int> GetMetricsAsync(string url)
        {
            ResetFields();
            HtmlDocument html = await FetchHtmlDocumentAsync(url);
            Parse(html);
            await ValidateRobotsAsync(url);
            Validate(url);
            return Math.Clamp(_onPageGEO, 0, 100);
        }

        private void ResetFields()
        {
            _onPageGEO = 100;
            h1Tags = [];
            otherHTags = [];
            metaDescription = "";
            metaTitle = "";
            containsLang = false;
            keywords = [];
            content = "";
            imageWithNoAlt = false;
            imagesWithAltCount = 0;
            imagesWithoutAltCount = 0;
            noCanonicalTag = true;
            noindexTag = true;
            hasJsonLd = false;
            hasRobots = true;
            wordCount = 0;
            textToHtmlRatio = 0;
            internalLinksCount = 0;
            externalLinksCount = 0;
            hasViewportMeta = false;
            loadTimeMs = 0;
        }

        private void Validate(string url)
        {
            if (noindexTag)
            {
                _onPageGEO -= WEIGHT_CRITICAL;
            }

            if (!hasRobots)
            {
                _onPageGEO -= WEIGHT_HIGH;
            }

            if (!containsLang)
            {
                _onPageGEO -= WEIGHT_HIGH;
            }

            if (noCanonicalTag)
            {
                _onPageGEO -= WEIGHT_HIGH;
            }

            var titleScore = CalculateTitleScore();
            _onPageGEO -= titleScore.penalty;
            if (titleScore.bonus > 0) _onPageGEO = Math.Min(100, _onPageGEO + titleScore.bonus);

            var metaScore = CalculateMetaDescriptionScore();
            _onPageGEO -= metaScore.penalty;
            if (metaScore.bonus > 0) _onPageGEO = Math.Min(100, _onPageGEO + metaScore.bonus);

            var headingScore = CalculateHeadingScore();
            _onPageGEO -= headingScore;

            var contentScore = CalculateContentScore();
            _onPageGEO -= contentScore;

            var imageScore = CalculateImageScore();
            _onPageGEO -= imageScore;

            var technicalScore = CalculateTechnicalSeoScore();
            _onPageGEO -= technicalScore;

            ApplyBonusPoints();
        }

        private (int penalty, int bonus) CalculateTitleScore()
        {
            int penalty = 0;
            int bonus = 0;

            if (string.IsNullOrWhiteSpace(metaTitle))
            {
                penalty += WEIGHT_HIGH;
            }
            else
            {
                int titleLength = metaTitle.Length;

                if (titleLength < 50)
                {
                    penalty += WEIGHT_MEDIUM;
                }
                else if (titleLength > 60)
                {
                    penalty += WEIGHT_LOW;
                }
                else
                {
                    bonus += WEIGHT_LOW; 
                }

                if (HasKeywordStuffing(metaTitle))
                {
                    penalty += WEIGHT_MEDIUM;
                }

                if (metaTitle.Length > 10)
                {
                    bonus += WEIGHT_MINOR;
                }
            }

            return (penalty, bonus);
        }

        private (int penalty, int bonus) CalculateMetaDescriptionScore()
        {
            int penalty = 0;
            int bonus = 0;

            if (string.IsNullOrWhiteSpace(metaDescription))
            {
                penalty += WEIGHT_HIGH;
            }
            else
            {
                int metaLength = metaDescription.Length;

                if (metaLength < 120)
                {
                    penalty += WEIGHT_MEDIUM;
                }
                else if (metaLength > 160)
                {
                    penalty += WEIGHT_LOW;
                }
                else
                {
                    bonus += WEIGHT_LOW;
                }

                if (HasKeywordStuffing(metaDescription))
                {
                    penalty += WEIGHT_MEDIUM;
                }
            }

            return (penalty, bonus);
        }

        private int CalculateHeadingScore()
        {
            int penalty = 0;

            if (h1Tags.Count == 0)
            {
                penalty += WEIGHT_HIGH;
            }
            else if (h1Tags.Count > 1)
            {
                penalty += WEIGHT_MEDIUM; 
            }

            if (otherHTags.Count == 0)
            {
                penalty += WEIGHT_MEDIUM;
            }
            else if (otherHTags.Count >= 3)
            {
                
            }

            return penalty;
        }

        private int CalculateContentScore()
        {
            int penalty = 0;
            wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount < 200)
            {
                penalty += WEIGHT_HIGH;
            }
            else if (wordCount < 400)
            {
                penalty += WEIGHT_MEDIUM;
            }
            else if (wordCount < 600)
            {
                penalty += WEIGHT_LOW; 
            }

            if (HasKeywordStuffing(content))
            {
                penalty += WEIGHT_MEDIUM;
            }

            return penalty;
        }

        private int CalculateImageScore()
        {
            int penalty = 0;
            int totalImages = imagesWithAltCount + imagesWithoutAltCount;

            if (totalImages == 0) return 0; 

            double altTextRatio = (double)imagesWithAltCount / totalImages;

            if (altTextRatio < 0.5) 
            {
                penalty += WEIGHT_MEDIUM;
            }
            else if (altTextRatio < 0.8)
            {
                penalty += WEIGHT_LOW;
            }

            return penalty;
        }

        private int CalculateTechnicalSeoScore()
        {
            int penalty = 0;

            if (!hasJsonLd)
            {
                penalty += WEIGHT_MEDIUM;
            }

            if (!hasViewportMeta)
            {
                penalty += WEIGHT_HIGH;
            }

            return penalty;
        }

        private void ApplyBonusPoints()
        {
            int bonus = 0;

            if (wordCount >= 800) bonus += WEIGHT_LOW;
            if (wordCount >= 1200) bonus += WEIGHT_MINOR;

            if (otherHTags.Count >= 3) bonus += WEIGHT_MINOR;

            if (internalLinksCount > 3 && externalLinksCount > 1) bonus += WEIGHT_LOW;

            if (loadTimeMs > 0 && loadTimeMs < 1000) bonus += WEIGHT_MEDIUM;

            if (imagesWithAltCount > 0 && imagesWithoutAltCount == 0) bonus += WEIGHT_LOW;

            _onPageGEO = Math.Min(100, _onPageGEO + bonus);
        }

        private bool HasKeywordStuffing(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 10) return false;

            var wordFrequency = words.GroupBy(w => w)
                                   .ToDictionary(g => g.Key, g => g.Count());

            double maxFrequency = wordFrequency.Values.Max();
            return (maxFrequency / words.Length) > 0.15;
        }

        private void Parse(HtmlDocument html)
        {
            var headNode = html.DocumentNode.SelectSingleNode("//head");
            containsLang = headNode?.GetAttributeValue("lang", "") != "";

            h1Tags = html.DocumentNode.SelectNodes("//h1")?.ToList() ?? [];

            otherHTags = html.DocumentNode.SelectNodes("//h2")?.ToList() ?? [];

            otherHTags.Concat(html.DocumentNode.SelectNodes("//h3")?.ToList() ?? []);

            otherHTags.Concat(html.DocumentNode.SelectNodes("//h4")?.ToList() ?? []);

            otherHTags.Concat(html.DocumentNode.SelectNodes("//h5")?.ToList() ?? []);

            otherHTags.Concat(html.DocumentNode.SelectNodes("//h6")?.ToList() ?? []);

            var scriptNodes = html.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            hasJsonLd = scriptNodes != null && scriptNodes.Count > 0;

            var canonicalLink = html.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
            noCanonicalTag = canonicalLink == null;

            var metaDescriptionNode = html.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (metaDescriptionNode != null)
            {
                metaDescription = metaDescriptionNode.GetAttributeValue("content", "");
            }

            var metaTitleNode = html.DocumentNode.SelectSingleNode("//meta[@name='title']");
            if (metaTitleNode != null)
            {
                metaTitle = metaTitleNode.GetAttributeValue("content", "");
            }
            else
            {
                var pageTitleNode = html.DocumentNode.SelectSingleNode("//title");
                if (pageTitleNode != null)
                {
                    metaTitle = pageTitleNode.InnerText.Trim();
                }
            }

            var metaKeywordsNode = html.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            if (metaKeywordsNode != null)
            {
                keywords = metaKeywordsNode.GetAttributeValue("content", "")
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            var metaRobotsNode = html.DocumentNode.SelectSingleNode("//meta[@name='robots']");
            if (metaRobotsNode != null)
            {
                var robotsContent = metaRobotsNode.GetAttributeValue("content", "").ToLower();
                noindexTag = robotsContent.Contains("noindex");
            }

            var images = html.DocumentNode.SelectNodes("//img");

            if (images != null)
            {
                foreach (var image in images)
                {
                    if (image.GetAttributeValue("alt", "") == "")
                    {
                        imageWithNoAlt = true;

                        break;
                    }
                }
            }

            content = ExtractContentText(html);
        }

        private string ExtractContentText(HtmlDocument html)
        {
            var textBuilder = new StringBuilder();

            var contentElements = html.DocumentNode.SelectNodes("//p|//div|//span|//h1|//h2|//h3|//h4|//h5|//h6|//li|//td|//a|//article|//section");

            if (contentElements != null)
            {
                foreach (var element in contentElements)
                {
                    if (IsInHeaderFooterOrNav(element))
                        continue;

                    var text = element.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textBuilder.Append(text + " ");
                    }
                }
            }

            return ContentRegex().Replace(textBuilder.ToString(), " ").Trim();
        }

        private bool IsInHeaderFooterOrNav(HtmlNode node)
        {
            var parent = node.ParentNode;
            while (parent != null)
            {
                if (parent.Name == "header" || parent.Name == "footer" || parent.Name == "nav")
                    return true;
                parent = parent.ParentNode;
            }
            return false;
        }

        private async Task ValidateRobotsAsync(string url)
        {
            try
            {
                var robotsTxtUrl = new Uri(new Uri(url), "/robots.txt").ToString();
                HttpClient _httpClient = new();
                string robotsContent = await _httpClient.GetStringAsync(robotsTxtUrl);
                hasRobots = !string.IsNullOrEmpty(robotsContent);
            }
            catch (HttpRequestException)
            {
                hasRobots = false;
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
        private async Task<HtmlDocument> FetchHtmlDocumentAsync(string url)
        {
            var cachedHtml = await LoadFromCacheAsync(url);
            if (cachedHtml != null)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(cachedHtml);
                return htmlDoc;
            }

            using (var httpClient = new HttpClient())
            {
                var htmlString = await httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlString);

                await SaveToCacheAsync(url, htmlDoc);
                return htmlDoc;
            }
        }
        private async Task<string?> LoadFromCacheAsync(string url)
        {
            try
            {
                var cachePath = GetCacheFilePath(url);
                if (File.Exists(cachePath))
                {
                    return await File.ReadAllTextAsync(cachePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cache read error: {ex.Message}");
            }
            return null;
        }

        private async Task SaveToCacheAsync(string url, HtmlDocument htmlDocument)
        {
            try
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    Directory.CreateDirectory(CacheDirectory);
                }

                var cachePath = GetCacheFilePath(url);
                await File.WriteAllTextAsync(cachePath, htmlDocument.DocumentNode.OuterHtml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cache write error: {ex.Message}");
            }
        }

        private string GetCacheFilePath(string url)
        {
            using var sha256 = SHA256.Create();
            var urlBytes = Encoding.UTF8.GetBytes(url);
            var hashBytes = sha256.ComputeHash(urlBytes);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return Path.Combine(CacheDirectory, $"{hashString}.html");
        }

        [GeneratedRegex(@"^(https?):\/\/(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$", RegexOptions.IgnoreCase, "ru-RU")]
        private static partial Regex UriRegex();
        [GeneratedRegex(@"(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(?::(?:0|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5]))?(?:\/(?:[-a-zA-Z0-9@%_\+.~#?&=]+\/?)*)?$", RegexOptions.IgnoreCase, "ru-RU")]
        private static partial Regex UrlRegex();
        [GeneratedRegex(@"\s+")]
        private static partial Regex ContentRegex();
    }
}
