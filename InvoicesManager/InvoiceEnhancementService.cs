using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using ZXing;
using ZXing.Common;

namespace InvoicesManager
{
    internal static class InvoiceEnhancementService
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();
        private static readonly Regex DomainRegex = new Regex(@"^(?:https?://)?(?:www\.)?(?<domain>[a-z0-9.-]+\.[a-z]{2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ClearbitDomainRegex = new Regex("\"domain\"\\s*:\\s*\"(?<domain>[^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> KnownClientDomains = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "emag", "emag.ro" },
            { "e-mag", "emag.ro" },
            { "e mag", "emag.ro" }
        };

        internal static string GetAssetsDirectory()
        {
            string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string assetsDirectory = Path.Combine(baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory, "invoice-assets");
            Directory.CreateDirectory(assetsDirectory);
            return assetsDirectory;
        }

        internal static string GetClientLogoPath(string clientName, string assetsDirectory)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                return CreateFallbackClientBadge("Client", assetsDirectory);
            }

            foreach (string domain in ResolveCandidateDomains(clientName))
            {
                string safeDomain = SanitizePathSegment(domain);
                string logoPath = Path.Combine(assetsDirectory, $"logo_{safeDomain}.png");
                if (IsUsableImageFile(logoPath))
                {
                    return logoPath;
                }

                if (TryDownloadDomainLogoAsPng(domain, logoPath))
                {
                    return logoPath;
                }
            }

            return CreateFallbackClientBadge(clientName, assetsDirectory);
        }

        internal static string BuildBarcodeValue(string invoiceNumber, int invoiceId)
        {
            return $"INV-{invoiceNumber}-{invoiceId}";
        }

        internal static string GenerateRandomBarcodeValue()
        {
            string randomToken = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{randomToken}";
        }

        internal static string GenerateBarcodeImage(string barcodeValue, string assetsDirectory)
        {
            if (string.IsNullOrWhiteSpace(barcodeValue))
            {
                return null;
            }

            string barcodePath = Path.Combine(assetsDirectory, $"barcode_{SanitizePathSegment(barcodeValue)}.png");

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 120,
                    Width = 520,
                    Margin = 6,
                    PureBarcode = true
                }
            };

            using (Bitmap barcodeBitmap = writer.Write(barcodeValue))
            {
                barcodeBitmap.Save(barcodePath, ImageFormat.Png);
            }

            return barcodePath;
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("InvoicesManager/1.0");
            return client;
        }

        private static IEnumerable<string> ResolveCandidateDomains(string clientName)
        {
            var domains = new List<string>();
            string normalizedName = clientName.Trim().ToLowerInvariant();

            if (KnownClientDomains.TryGetValue(normalizedName, out string mappedDomain))
            {
                domains.Add(mappedDomain);
            }

            string extractedDomain = ExtractDomain(clientName);
            if (!string.IsNullOrWhiteSpace(extractedDomain))
            {
                domains.Add(extractedDomain);
            }

            string autoCompleteDomain = ResolveDomainWithAutocomplete(clientName);
            if (!string.IsNullOrWhiteSpace(autoCompleteDomain))
            {
                domains.Add(autoCompleteDomain);
            }

            string slug = Regex.Replace(normalizedName, @"[^a-z0-9]", string.Empty);
            if (!string.IsNullOrWhiteSpace(slug))
            {
                domains.Add($"{slug}.ro");
                domains.Add($"{slug}.com");
            }

            return domains
                .Where(domain => !string.IsNullOrWhiteSpace(domain))
                .Select(domain => domain.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string ResolveDomainWithAutocomplete(string clientName)
        {
            try
            {
                string endpoint = "https://autocomplete.clearbit.com/v1/companies/suggest?query=" + Uri.EscapeDataString(clientName);
                using (HttpResponseMessage response = HttpClient.GetAsync(endpoint).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Match match = ClearbitDomainRegex.Match(json);
                    if (match.Success)
                    {
                        return match.Groups["domain"].Value;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string ExtractDomain(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            Match match = DomainRegex.Match(value.Trim());
            return match.Success ? match.Groups["domain"].Value : null;
        }

        private static bool TryDownloadDomainLogoAsPng(string domain, string destinationPath)
        {
            string encodedDomain = Uri.EscapeDataString(domain);
            string[] imageUrls =
            {
                $"https://logos.hunter.io/{encodedDomain}",
                $"https://logos.hunter.io/{encodedDomain}?format=png&size=256",
                $"https://www.google.com/s2/favicons?domain={encodedDomain}&sz=256",
                $"https://icons.duckduckgo.com/ip3/{encodedDomain}.ico"
            };

            foreach (string imageUrl in imageUrls)
            {
                if (TryDownloadAndNormalizeImage(imageUrl, destinationPath))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryDownloadAndNormalizeImage(string imageUrl, string destinationPath)
        {
            try
            {
                using (HttpResponseMessage response = HttpClient.GetAsync(imageUrl).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    string mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                    if (!mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    byte[] bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    if (bytes.Length < 128)
                    {
                        return false;
                    }

                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                    using (Image sourceImage = Image.FromStream(memoryStream, true, true))
                    using (Bitmap normalizedBitmap = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb))
                    using (Graphics graphics = Graphics.FromImage(normalizedBitmap))
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.DrawImage(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height);
                        normalizedBitmap.Save(destinationPath, ImageFormat.Png);
                    }

                    return IsUsableImageFile(destinationPath);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsUsableImageFile(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return false;
            }

            if (new FileInfo(imagePath).Length < 128)
            {
                return false;
            }

            try
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    return image.Width >= 16 && image.Height >= 16;
                }
            }
            catch
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch
                {
                    // Ignore cleanup failures.
                }

                return false;
            }
        }

        private static string CreateFallbackClientBadge(string clientName, string assetsDirectory)
        {
            string safeClient = SanitizePathSegment(clientName.Trim().ToLowerInvariant());
            string fallbackPath = Path.Combine(assetsDirectory, $"logo_fallback_{safeClient}.png");
            if (IsUsableImageFile(fallbackPath))
            {
                return fallbackPath;
            }

            string text = string.IsNullOrWhiteSpace(clientName) ? "CLIENT" : clientName.Trim().ToUpperInvariant();
            if (text.Length > 24)
            {
                text = text.Substring(0, 24);
            }

            using (Bitmap bitmap = new Bitmap(640, 190, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(245, 248, 252)))
            using (Pen borderPen = new Pen(Color.FromArgb(30, 58, 95), 4))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(30, 58, 95)))
            using (Font titleFont = new Font("Segoe UI Semibold", 36, FontStyle.Bold))
            using (Font subtitleFont = new Font("Segoe UI", 14, FontStyle.Regular))
            using (SolidBrush subtitleBrush = new SolidBrush(Color.FromArgb(87, 106, 129)))
            {
                graphics.Clear(Color.White);
                graphics.FillRectangle(backgroundBrush, 4, 4, 632, 182);
                graphics.DrawRectangle(borderPen, 4, 4, 632, 182);

                using (StringFormat centerFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    RectangleF mainRect = new RectangleF(12, 18, 616, 104);
                    graphics.DrawString(text, titleFont, titleBrush, mainRect, centerFormat);
                }

                graphics.DrawString("Brand image unavailable - using fallback", subtitleFont, subtitleBrush, 24, 144);
                bitmap.Save(fallbackPath, ImageFormat.Png);
            }

            return fallbackPath;
        }

        private static string SanitizePathSegment(string input)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safe = new string(input.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
            return safe.Replace(" ", "_");
        }
    }
}
