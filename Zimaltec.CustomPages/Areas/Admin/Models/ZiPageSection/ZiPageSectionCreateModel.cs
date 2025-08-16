using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;

public class ZiPageSectionCreateModel
{
    [Required] public int PageId { get; set; }
    [Required] public int TopicId { get; set; }
    [StringLength(400)] public string? Name { get; set; }
    public int? DisplayOrder { get; set; }

    // Se rellena dinámicamente tras elegir Topic
    public List<SectionFieldPostModel> Fields { get; set; } = [];
}