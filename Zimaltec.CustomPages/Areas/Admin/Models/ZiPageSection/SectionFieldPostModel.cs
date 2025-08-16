using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;

public class SectionFieldPostModel
{
    [Required] public string Key { get; set; } = "";
    [Required] public FieldType Type { get; set; } = FieldType.Text;

    // Valores posibles (uno u otro según Type)
    public string? TextValue { get; set; }
    public string? RichTextValue { get; set; }
    public int? PictureId { get; set; }     // Image
    public int? DownloadId { get; set; }    // File
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public bool? BoolValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public string? LinkText { get; set; }   // Link
    public string? LinkUrl { get; set; }
    public string? LinkTarget { get; set; }
    public string? EntityName { get; set; } // Entity
    public int? EntityId { get; set; }
    public string? JsonValue { get; set; }  // Json
    public List<string>? ListItems { get; set; } // List
}