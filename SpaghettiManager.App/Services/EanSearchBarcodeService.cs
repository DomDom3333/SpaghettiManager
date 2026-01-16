using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Linq;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Services;

public sealed record BarcodeLookupResult(
    string Barcode,
    Enums.BarcodeType BarcodeType,
    string? ProductName,
    string? Brand,
    string? Category,
    Material? Material,
    bool AddedMapping,
    string? ErrorMessage);

public sealed class EanSearchBarcodeService
{
    private static readonly Regex TableRowRegex = new("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex CellRegex = new("<t[dh][^>]*>(.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex TagRegex = new("<[^>]+>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private readonly HttpClient httpClient;
    private readonly SpaghettiDatabase database;

    public EanSearchBarcodeService(HttpClient httpClient, SpaghettiDatabase database)
    {
        this.httpClient = httpClient;
        this.database = database;
    }

    public async Task<BarcodeLookupResult> LookupAndMapAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var digits = NormalizeBarcode(barcode);
        if (string.IsNullOrWhiteSpace(digits))
        {
            return new BarcodeLookupResult(barcode, Enums.BarcodeType.Unknown, null, null, null, null, false, "Barcode is empty or invalid.");
        }

        var barcodeType = DetermineBarcodeType(digits);
        if (!long.TryParse(digits, out var barcodeValue))
        {
            return new BarcodeLookupResult(digits, barcodeType, null, null, null, null, false, "Barcode is not numeric.");
        }

        var existing = await FindExistingMappingAsync(barcodeValue, cancellationToken);
        if (existing is not null)
        {
            return new BarcodeLookupResult(digits, existing.BarcodeType, existing.Material?.Name, existing.Manufacturer, null, existing.Material, false, null);
        }

        string html;
        try
        {
            html = await FetchHtmlAsync(digits, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return new BarcodeLookupResult(digits, barcodeType, null, null, null, null, false, $"Failed to fetch barcode data: {ex.Message}");
        }

        var info = ParseHtml(html, digits, barcodeType);
        if (info is null)
        {
            return new BarcodeLookupResult(digits, barcodeType, null, null, null, null, false, "No barcode information found in HTML response.");
        }

        var material = await FindMaterialMatchAsync(info, cancellationToken);
        if (material is null)
        {
            return new BarcodeLookupResult(digits, barcodeType, info.ProductName, info.Brand, info.Category, null, false, "No catalog match found.");
        }

        var spool = new Spool
        {
            Barcode = barcodeValue,
            BarcodeType = barcodeType,
            Material = material,
            MaterialId = material.Id,
            Manufacturer = info.Brand ?? material.Manufacturer ?? material.Name
        };

        await database.SaveSpoolAsync(spool);

        return new BarcodeLookupResult(digits, barcodeType, info.ProductName, info.Brand, info.Category, material, true, null);
    }

    private async Task<string> FetchHtmlAsync(string barcode, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.ean-search.org/?q={Uri.EscapeDataString(barcode)}");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("SpaghettiManager", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<Spool?> FindExistingMappingAsync(long barcode, CancellationToken cancellationToken)
    {
        await foreach (var spool in database.StreamSpoolsAsync(barcode: barcode, pageSize: 1, cancellationToken: cancellationToken))
        {
            return spool;
        }

        return null;
    }

    private static EanSearchLookupInfo? ParseHtml(string html, string barcode, Enums.BarcodeType barcodeType)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var entries = ExtractKeyValues(html);
        var productName = FindValue(entries, "Product name", "Product", "Name", "Description", "Title");
        var brand = FindValue(entries, "Brand", "Manufacturer", "Company", "Issuer", "Producent", "Maker");
        var category = FindValue(entries, "Category", "Group", "Type");

        productName ??= ExtractHeadline(html);
        productName ??= ExtractProductLineFromLink(html, barcode);
        productName ??= ExtractProductLineFromText(html, barcode);
        if (string.IsNullOrWhiteSpace(brand) && !string.IsNullOrWhiteSpace(productName))
        {
            brand = DeriveBrandFromProductName(productName);
        }

        return new EanSearchLookupInfo(barcode, barcodeType, productName, brand, category, entries);
    }

    private static string? ExtractHeadline(string html)
    {
        var headline = ExtractFirstMatch(html, "<h1[^>]*>(.*?)</h1>")
            ?? ExtractFirstMatch(html, "<h2[^>]*class=\"product-name\"[^>]*>(.*?)</h2>")
            ?? ExtractFirstMatch(html, "<h2[^>]*>(.*?)</h2>");

        return headline;
    }

    private static string? ExtractProductLineFromText(string html, string barcode)
    {
        var text = StripHtml(html);
        var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length >= 8 && line.Length <= 140)
            .Where(line => !line.Contains("Access denied", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (lines.Count == 0)
        {
            return null;
        }

        var candidates = lines
            .Select(line => new { Line = line, Score = ScoreCandidateLine(line, barcode) })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ToList();

        return candidates.Count > 0 ? candidates[0].Line : null;
    }

    private static string? ExtractProductLineFromLink(string html, string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        var pattern = $"<a[^>]*href=\\\"/ean/{Regex.Escape(barcode)}\\\"[^>]*>(.*?)</a>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!match.Success)
        {
            return null;
        }

        return CleanHtml(match.Groups[1].Value);
    }

    private static int ScoreCandidateLine(string line, string barcode)
    {
        var score = 0;
        if (line.Contains("mm", StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }
        if (line.Contains("kg", StringComparison.OrdinalIgnoreCase) || line.Contains(" g", StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }
        if (line.Contains("PLA", StringComparison.OrdinalIgnoreCase)
            || line.Contains("PETG", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ABS", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Filament", StringComparison.OrdinalIgnoreCase))
        {
            score += 3;
        }
        if (!string.IsNullOrWhiteSpace(barcode) && line.Contains(barcode, StringComparison.OrdinalIgnoreCase))
        {
            score -= 2;
        }
        if (line.Any(char.IsLetter) && line.Any(char.IsDigit))
        {
            score += 1;
        }

        return score;
    }

    private static string? DeriveBrandFromProductName(string productName)
    {
        var firstToken = productName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstToken) ? null : firstToken.Trim();
    }

    private static string? ExtractFirstMatch(string html, string pattern)
    {
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!match.Success)
        {
            return null;
        }

        return CleanHtml(match.Groups[1].Value);
    }

    private static Dictionary<string, string> ExtractKeyValues(string html)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match row in TableRowRegex.Matches(html))
        {
            var cells = CellRegex.Matches(row.Groups[1].Value);
            if (cells.Count < 2)
            {
                continue;
            }

            var key = CleanHtml(cells[0].Groups[1].Value);
            var value = CleanHtml(cells[1].Groups[1].Value);
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!result.ContainsKey(key))
            {
                result[key] = value;
            }
        }

        return result;
    }

    private static string? FindValue(Dictionary<string, string> entries, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (entries.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static string NormalizeBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return string.Empty;
        }

        var digits = barcode.Where(char.IsDigit).ToArray();
        return digits.Length == 0 ? string.Empty : new string(digits);
    }

    private static Enums.BarcodeType DetermineBarcodeType(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode) || !barcode.All(char.IsDigit))
        {
            return Enums.BarcodeType.Other;
        }

        return barcode.Length switch
        {
            12 => Enums.BarcodeType.Upc,
            13 => Enums.BarcodeType.Ean,
            8 => Enums.BarcodeType.Ean,
            _ => Enums.BarcodeType.Other
        };
    }

    private async Task<Material?> FindMaterialMatchAsync(EanSearchLookupInfo info, CancellationToken cancellationToken)
    {
        var query = info.Brand ?? info.ProductName;
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        Material? bestMatch = null;
        var bestScore = 0;

        await foreach (var material in database.StreamMaterialsAsync(query, pageSize: 200, cancellationToken: cancellationToken))
        {
            var score = ScoreMatch(material, info);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = material;
            }

            if (bestScore >= 4)
            {
                break;
            }
        }

        return bestScore > 0 ? bestMatch : null;
    }

    private static int ScoreMatch(Material material, EanSearchLookupInfo info)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(info.Brand) && !string.IsNullOrWhiteSpace(material.Manufacturer))
        {
            if (ContainsIgnoreCase(material.Manufacturer, info.Brand) || ContainsIgnoreCase(info.Brand, material.Manufacturer))
            {
                score += 3;
            }
        }

        if (!string.IsNullOrWhiteSpace(info.ProductName) && !string.IsNullOrWhiteSpace(material.Name))
        {
            if (ContainsIgnoreCase(material.Name, info.ProductName) || ContainsIgnoreCase(info.ProductName, material.Name))
            {
                score += 2;
            }
        }

        if (!string.IsNullOrWhiteSpace(info.ProductName) && !string.IsNullOrWhiteSpace(material.Notes))
        {
            if (ContainsIgnoreCase(material.Notes, info.ProductName))
            {
                score += 1;
            }
        }

        return score;
    }

    private static bool ContainsIgnoreCase(string source, string value)
    {
        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static string CleanHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var noTags = TagRegex.Replace(value, string.Empty);
        return WebUtility.HtmlDecode(noTags).Trim();
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withoutScripts = Regex.Replace(html, "<script[^>]*>.*?</script>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var withoutStyles = Regex.Replace(withoutScripts, "<style[^>]*>.*?</style>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var noTags = TagRegex.Replace(withoutStyles, "\n");
        return WebUtility.HtmlDecode(noTags);
    }

    private sealed record EanSearchLookupInfo(
        string Barcode,
        Enums.BarcodeType BarcodeType,
        string? ProductName,
        string? Brand,
        string? Category,
        Dictionary<string, string> Details);
}
