namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;

public class ZiPageSectionEditModel
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public string? Name { get; set; }
    public int DisplayOrder { get; set; }
}
