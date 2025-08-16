using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiSectionField;

public class ZiSectionFieldEditItemModel
{
    public int FieldId { get; set; }
    public string Key { get; set; } = "";
    public FieldType Type { get; set; }

    // Valores editables (mapea con tus SectionFieldPostModel)
    public string? TextValue { get; set; }
    public string? RichTextValue { get; set; }
    public int? PictureId { get; set; }
    public int? DownloadId { get; set; }
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public bool? BoolValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public string? LinkText { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTarget { get; set; }
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public string? JsonValue { get; set; }
    public List<string>? ListItems { get; set; }
    public string? SettingsJson { get; set; }

    public bool IsObsolete { get; set; }
    public int DisplayOrder { get; set; }
}