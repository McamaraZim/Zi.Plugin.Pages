using Nop.Core;

namespace Nop.Plugin.Zimaltec.CustomPages.Domain;

public class ZiSectionField : BaseEntity
{
    public int PageSectionId { get; set; }

    // clave normalizada del placeholder (única por sección)

    public required string Key { get; set; }

    // tipo elegido en la sección
    public int Type { get; set; } // usar enum FieldType

    // reglas/ajustes de validación y UI por campo en esta sección
    public string? SettingsJson { get; set; }

    // marcado si el placeholder desapareció del Topic tras sincronizar
    public bool IsObsolete { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}