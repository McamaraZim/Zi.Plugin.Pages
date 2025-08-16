using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Zimaltec.CustomPages.Domain;
using Nop.Plugin.Zimaltec.CustomPages.Services;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Controllers;

namespace Nop.Plugin.Zimaltec.CustomPages.Controllers;

public partial class CustomPagesPublicController : BasePublicController
{
    private readonly IUrlRecordService _urlRecordService;
    private readonly IZiPageService _pageService;
    private readonly IZiPageSectionService _pageSectionService;
    private readonly ITopicService _topicService;
    private readonly IPictureService _pictureService;

    public CustomPagesPublicController(
        IUrlRecordService urlRecordService,
        IZiPageService pageService,
        IZiPageSectionService pageSectionService,
        ITopicService topicService,
        IPictureService pictureService)
    {
        _urlRecordService = urlRecordService;
        _pageService = pageService;
        _pageSectionService = pageSectionService;
        _topicService = topicService;
        _pictureService = pictureService;
    }

    // /{SeName} con constraint (solo slugs de ZiPage llegan aquí)
    [HttpGet]
    public async Task<IActionResult> Details(string seName)
    {
        var record = await _urlRecordService.GetBySlugAsync(seName);
        if (record is not { IsActive: true } ||
            !record.EntityName.Equals("ZiPage", StringComparison.InvariantCultureIgnoreCase))
            return RedirectToRoute("Homepage");

        var page = await _pageService.GetByIdAsync(record.EntityId);
        if (page is not { Published: true })
            return RedirectToRoute("Homepage");

        // Render de secciones vs. plantilla Topic + valores
        var sections = await _pageSectionService.GetByPageIdAsync(page.Id);
        var rendered = new List<RenderedSectionVm>();

        foreach (var section in sections.OrderBy(s => s.DisplayOrder))
        {
            var topic = await _topicService.GetTopicByIdAsync(section.TopicId);
            if (topic is null) continue;

            var (_, pairs) = await _pageSectionService.GetSectionDetailsAsync(section.Id);

            var html = await RenderTopicWithValuesAsync(topic.Body ?? string.Empty, pairs);
            rendered.Add(new RenderedSectionVm
            {
                Name = string.IsNullOrWhiteSpace(section.Name) ? topic.SystemName : section.Name,
                Html = html
            });
        }

        var vm = new PublicZiPageVm
        {
            Title = page.Title,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            Sections = rendered
        };

        return View("~/Plugins/Zimaltec.CustomPages/Views/Public/Details.cshtml", vm);
    }

    // ---------- Helpers de render ----------

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToLowerInvariant().Replace(' ', '_');
    }

    private async Task<string> RenderTopicWithValuesAsync(
        string topicBody,
        IList<(ZiSectionField field, ZiSectionFieldValue? value)> pairs)
    {
        // key normalizada -> html
        var map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var (field, val) in pairs)
        {
            var key = NormalizeKey(field.Key);
            var html = await RenderValueJsonToHtmlAsync((FieldType)field.Type, val?.ValueJson);
            map[key] = html ?? string.Empty;
        }

        var rx = PlaceholderRegex();
        var result = rx.Replace(topicBody, m =>
        {
            var rawKey = m.Groups[1].Value;
            var k = NormalizeKey(rawKey);
            return map.TryGetValue(k, out var replacement) ? replacement : string.Empty;
        });

        return result;
    }

    private async Task<string?> RenderValueJsonToHtmlAsync(FieldType type, string? valueJson)
    {
        if (string.IsNullOrWhiteSpace(valueJson))
            return type switch { _ => string.Empty };

        try
        {
            using var doc = JsonDocument.Parse(valueJson);
            var root = doc.RootElement;

            switch (type)
            {
                case FieldType.Text:
                    return HtmlEncode(root);

                case FieldType.RichText:
                    // HTML tal cual
                    return root.ValueKind == JsonValueKind.String ? root.GetString() : root.ToString();

                case FieldType.Image:
                    if (root.ValueKind != JsonValueKind.Object ||
                        !root.TryGetProperty("pictureId", out var pid) ||
                        !pid.TryGetInt32(out var picId) ||
                        picId <= 0) return string.Empty;
                    var pictureUrl = await _pictureService.GetPictureUrlAsync(picId);
                    return $"<img src=\"{WebUtility.HtmlEncode(pictureUrl)}\" alt=\"\" />";

                case FieldType.File:
                    if (root.ValueKind != JsonValueKind.Object ||
                        !root.TryGetProperty("downloadId", out var did) ||
                        !did.TryGetInt32(out var dId) ||
                        dId <= 0) return string.Empty;
                    var href = $"/download/getfile/{dId}";
                    return $"<a href=\"{WebUtility.HtmlEncode(href)}\">Descargar archivo</a>";

                case FieldType.Number:
                    return root.ValueKind == JsonValueKind.Number && root.TryGetInt32(out var n)
                        ? n.ToString()
                        : HtmlEncode(root);

                case FieldType.Decimal:
                    return root.ValueKind == JsonValueKind.Number && root.TryGetDecimal(out var de)
                        ? de.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        : HtmlEncode(root);

                case FieldType.Bool:
                    if (root.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        return root.GetBoolean() ? "true" : "false";
                    return string.Empty;

                case FieldType.DateTime:
                    if (root.ValueKind == JsonValueKind.String &&
                        DateTime.TryParse(root.GetString(), out var dt))
                        return dt.ToString("yyyy-MM-dd HH:mm");
                    return string.Empty;

                case FieldType.Link:
                    if (root.ValueKind != JsonValueKind.Object) return string.Empty;
                    var text = root.TryGetProperty("text", out var lt) ? lt.GetString() : null;
                    var url = root.TryGetProperty("url", out var lu) ? lu.GetString() : null;
                    var target = root.TryGetProperty("target", out var lta) ? lta.GetString() : null;
                    if (string.IsNullOrWhiteSpace(url)) return string.Empty;

                    var safeUrl = WebUtility.HtmlEncode(url);
                    var safeText = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(text) ? url : text);
                    var tgt = string.IsNullOrWhiteSpace(target) ? "" : $" target=\"{WebUtility.HtmlEncode(target)}\"";
                    return $"<a href=\"{safeUrl}\"{tgt}>{safeText}</a>";

                case FieldType.Entity:
                    if (root.ValueKind != JsonValueKind.Object) return string.Empty;
                    var name = root.TryGetProperty("entityName", out var en) ? en.GetString() : null;
                    var id = (root.TryGetProperty("id", out var eid) && eid.TryGetInt32(out var ei)) ? ei : (int?)null;
                    var safeName = WebUtility.HtmlEncode(name ?? "Entity");
                    return id.HasValue ? $"{safeName} #{id}" : safeName;

                case FieldType.Json:
                    return $"<pre>{WebUtility.HtmlEncode(valueJson)}</pre>";

                case FieldType.List:
                    if (root.ValueKind != JsonValueKind.Object ||
                        !root.TryGetProperty("items", out var items) ||
                        items.ValueKind != JsonValueKind.Array) return string.Empty;
                    var sb = new StringBuilder().Append("<ul>");
                    foreach (var e in items.EnumerateArray())
                        sb.Append("<li>")
                            .Append(WebUtility.HtmlEncode(e.ValueKind == JsonValueKind.String ? e.GetString() : e.ToString()))
                            .Append("</li>");
                    sb.Append("</ul>");
                    return sb.ToString();

                default:
                    return HtmlEncode(root);
            }
        }
        catch
        {
            // Si no parsea JSON: para RichText lo devolvemos crudo, para el resto lo escapamos
            return type == FieldType.RichText ? valueJson : WebUtility.HtmlEncode(valueJson);
        }
    }

    private static string? HtmlEncode(JsonElement el)
    {
        return WebUtility.HtmlEncode(el.ValueKind == JsonValueKind.String ? el.GetString() : el.ToString());
    }

    [GeneratedRegex(@"{{\s*([^:{}|]+?)\s*}}", RegexOptions.IgnoreCase, "es-ES")]
    private static partial Regex PlaceholderRegex();
}

// ===== ViewModel público =====
public class PublicZiPageVm
{
    public string? Title { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<RenderedSectionVm> Sections { get; set; } = [];
}

public class RenderedSectionVm
{
    public string? Name { get; set; }
    public string Html { get; set; } = "";
}
