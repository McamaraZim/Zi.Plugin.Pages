using Nop.Core;

namespace Nop.Plugin.Zimaltec.CustomPages.Domain;

public class ZiSectionFieldValue : BaseEntity
{
    public int SectionFieldId { get; set; }

    // valor base (overrides por idioma/tienda vía LocalizedProperty/StoreMapping)
    public string? ValueJson { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}