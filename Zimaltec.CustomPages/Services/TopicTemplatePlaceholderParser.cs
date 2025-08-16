using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Zimaltec.CustomPages.Services;

public static partial class TopicTemplatePlaceholderParser
{
    // Coincide {{clave}} sin tipo
    private static readonly Regex _rx = MyRegex();

    public static IReadOnlyList<string> Parse(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return [];
        // Normaliza claves: trim, lower, espacios -> _
        return _rx.Matches(html)
            .Select(m => NormalizeKey(m.Groups[1].Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string ComputeSnapshotHash(IEnumerable<string> keys)
    {
        var canon = string.Join("|", keys.Select(NormalizeKey).OrderBy(k => k));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canon)));
    }

    public static string NormalizeKey(string raw)
    {
        return string.Join("_", raw.Trim().ToLowerInvariant()
            .Split([' '], StringSplitOptions.RemoveEmptyEntries));
    }

    [GeneratedRegex(@"{{\s*([^:{}|]+?)\s*}}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "es-ES")]
    private static partial Regex MyRegex();
}