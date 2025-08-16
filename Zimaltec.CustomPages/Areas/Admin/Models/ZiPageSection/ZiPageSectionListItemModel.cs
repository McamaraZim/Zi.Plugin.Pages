namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;

public class ZiPageSectionListItemModel
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = "";
    public string? Name { get; set; }
    public int DisplayOrder { get; set; }
}