using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPage;

public record ZiPageSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Zimaltec.CustomPages.Admin.Pages.Title")]
    public string? SearchTitle { get; set; }
}