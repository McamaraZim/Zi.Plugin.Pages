using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPage;

public class ZiPageModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string SystemName { get; set; } = string.Empty;

    public bool Published { get; set; } = true;

    // SEO
    [StringLength(400)]
    public string? MetaTitle { get; set; }

    [StringLength(1000)]
    public string? MetaDescription { get; set; }

    [StringLength(400)]
    public string? MetaKeywords { get; set; }

    // Slug (UrlRecord)
    [Display(Name = "SeName (Slug)")]
    [StringLength(400)]
    public string? SeName { get; set; }
}