using Nop.Core;
using Nop.Core.Domain.Seo;

namespace Nop.Plugin.Zimaltec.CustomPages.Domain;

public class ZiPage : BaseEntity, ISlugSupported
{
    public required string SystemName { get; set; }
    public required string Title { get; set; }
    public bool Published { get; set; }

    // SEO básicos (localizables vía LocalizedProperty)
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}