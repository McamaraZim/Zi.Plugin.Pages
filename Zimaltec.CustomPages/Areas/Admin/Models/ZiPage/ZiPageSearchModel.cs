namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPage;

public class ZiPageSearchModel
{
    public string? Keywords { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}