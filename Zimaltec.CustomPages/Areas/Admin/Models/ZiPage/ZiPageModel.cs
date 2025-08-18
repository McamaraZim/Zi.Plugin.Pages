using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPage;

public partial record ZiPageModel : BaseNopEntityModel
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string SystemName { get; set; } = string.Empty;

    public bool Published { get; set; } = true;
    
    public string? MetaTitle { get; set; }
    
    public string? MetaDescription { get; set; }
    
    public string? MetaKeywords { get; set; }
    
    [Display(Name = "SeName (Slug)")]
    public string? SeName { get; set; }
}