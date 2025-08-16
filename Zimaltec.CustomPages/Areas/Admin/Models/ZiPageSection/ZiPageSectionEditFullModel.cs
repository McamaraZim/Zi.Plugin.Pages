using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiSectionField;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;

public class ZiPageSectionEditFullModel
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public string? Name { get; set; }
    public int TopicId { get; set; } // plantilla actual
    public IEnumerable<SelectListItem> Topics { get; set; } = [];
    public List<ZiSectionFieldEditItemModel> Fields { get; set; } = [];
}