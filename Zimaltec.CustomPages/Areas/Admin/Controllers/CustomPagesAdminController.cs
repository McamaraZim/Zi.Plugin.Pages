using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Zimaltec.CustomPages.Constants;
using Nop.Plugin.Zimaltec.CustomPages.Services;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPage;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
public class CustomPagesAdminController : BaseController
{
    private readonly IPermissionService _permissionService;
    private readonly IZiPageService _pageService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    public CustomPagesAdminController(
        IPermissionService permissionService,
        IZiPageService pageService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _pageService = pageService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    private const string VIEW_ROOT = "~/Plugins/Zimaltec.CustomPages/Areas/Admin/Views/CustomPagesAdmin/";


    public async Task<IActionResult> List(string? keywords = null, int page = 1, int pageSize = 15)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var search = new ZiPageSearchModel { Keywords = keywords, Page = page, PageSize = pageSize };
        var result = await _pageService.SearchAsync(search.Keywords, search.Page - 1, search.PageSize);

        ViewBag.Search = search;
        return View(VIEW_ROOT + "List.cshtml", result);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var model = new ZiPageModel();
        return View(VIEW_ROOT + "Create.cshtml", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ZiPageModel model)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return View(VIEW_ROOT + "Create.cshtml", model);

        var entity = new ZiPage
        {
            Title = model.Title,
            SystemName = model.SystemName,
            Published = model.Published,
            MetaTitle = model.MetaTitle,
            MetaDescription = model.MetaDescription,
            MetaKeywords = model.MetaKeywords
        };

        await _pageService.InsertAsync(entity, model.SeName);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.SAVED));

        return RedirectToAction(nameof(List));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var entity = await _pageService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction(nameof(List));

        var model = new ZiPageModel
        {
            Id = entity.Id,
            Title = entity.Title,
            SystemName = entity.SystemName,
            Published = entity.Published,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords
            // SeName lo cargaremos a demanda si quieres (UrlRecordService.GetSeName), por simplicidad lo dejamos vacío
        };

        return View(VIEW_ROOT + "Edit.cshtml", model); // 👈
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ZiPageModel model)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var entity = await _pageService.GetByIdAsync(model.Id);
        if (entity == null)
            return RedirectToAction(nameof(List));

        if (!ModelState.IsValid)
            return View(VIEW_ROOT + "Edit.cshtml", model);

        entity.Title = model.Title;
        entity.SystemName = model.SystemName;
        entity.Published = model.Published;
        entity.MetaTitle = model.MetaTitle;
        entity.MetaDescription = model.MetaDescription;
        entity.MetaKeywords = model.MetaKeywords;

        await _pageService.UpdateAsync(entity, model.SeName);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.SAVED));

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var entity = await _pageService.GetByIdAsync(id);
        if (entity != null)
            await _pageService.DeleteAsync(entity);

        return RedirectToAction(nameof(List));
    }
}