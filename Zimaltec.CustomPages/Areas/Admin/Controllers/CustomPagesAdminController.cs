using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
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
using Nop.Services.Topics;
using Nop.Services.Common;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiSectionField;
using Nop.Services.Seo;

namespace Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[Route("Admin/CustomPagesAdmin")]
public class CustomPagesAdminController : BaseController
{
    private readonly IPermissionService _permissionService;
    private readonly IZiPageService _pageService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IZiPageSectionService _pageSectionService;
    private readonly ITopicService _topicService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _ga;

    public CustomPagesAdminController(
        IPermissionService permissionService,
        IZiPageService pageService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IZiPageSectionService pageSectionService,
        ITopicService topicService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        IGenericAttributeService ga)
    {
        _permissionService = permissionService;
        _pageService = pageService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _pageSectionService = pageSectionService;
        _topicService = topicService;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
        _ga = ga;
    }

    private const string VIEW_ROOT = "~/Plugins/Zimaltec.CustomPages/Areas/Admin/Views/CustomPagesAdmin/";

    // GET: pantalla "Añadir sección" (con select de Topics plantilla)
    [HttpGet("AddSection")]
    public async Task<IActionResult> AddSection(int pageId)
    {
        if (!await _permissionService.AuthorizeAsync(Defaults.Plugins.Zimaltec.CustomPages.Permissions
                .MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var model = new ZiPageSectionCreateModel { PageId = pageId };

        var items = (await _pageSectionService.GetTemplateTopicsAsync())
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(t.Title)
                    ? t.SystemName
                    : t.Title
            })
            .OrderBy(i => i.Text)
            .ToList();

        ViewBag.TemplateTopicItems = items;

        ViewBag.TemplateTopics = await _pageSectionService.GetTemplateTopicsAsync();
        return View(VIEW_ROOT + "AddSection.cshtml", model);
    }

    // Endpoint para cargar placeholders de un Topic (AJAX -> partial con los campos)
    [HttpGet("GetTemplateFields")]
    public async Task<IActionResult> GetTemplateFields(int topicId)
    {
        if (!await _permissionService.AuthorizeAsync(Defaults.Plugins.Zimaltec.CustomPages.Permissions
                .MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var keys = await _pageSectionService.GetPlaceholdersAsync(topicId);
        var fields = keys.Select(k => new SectionFieldPostModel { Key = k, Type = FieldType.Text }).ToList();
        return PartialView(VIEW_ROOT + "_SectionFieldsCreate.cshtml", fields);
    }

    [HttpGet("List")]
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

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var model = new ZiPageModel();
        return View(VIEW_ROOT + "Create.cshtml", model);
    }

// GET: Editar sección
    [HttpGet("EditSection/{id:int}")]
    public async Task<IActionResult> EditSection(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var details = await _pageSectionService.GetSectionDetailsAsync(id);
        var (section, pairs) = details;

        var topics = (await _pageSectionService.GetTemplateTopicsAsync())
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(t.Title) ? t.SystemName : t.Title,
                Selected = t.Id == section.TopicId
            })
            .OrderBy(i => i.Text)
            .ToList();

        var vm = new ZiPageSectionEditFullModel
        {
            Id = section.Id,
            PageId = section.PageId,
            Name = section.Name,
            TopicId = section.TopicId,
            Topics = topics
        };

        // Construir items y rellenar desde ValueJson
        foreach (var (field, val) in pairs.OrderBy(p => p.field.DisplayOrder))
        {
            var item = new ZiSectionFieldEditItemModel
            {
                FieldId = field.Id,
                Key = field.Key,
                Type = (FieldType)field.Type,
                SettingsJson = field.SettingsJson,
                IsObsolete = field.IsObsolete,
                DisplayOrder = field.DisplayOrder
            };

            FillItemFromValueJson(item, val?.ValueJson, item.Type);
            vm.Fields.Add(item);
        }

        return View(VIEW_ROOT + "EditSection.cshtml", vm);
    }

    [HttpGet("Sections")]
    public async Task<IActionResult> Sections(int pageId)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var sections = await _pageSectionService.GetByPageIdAsync(pageId);

        // Diccionario topicId -> título (una sola consulta por topicId)
        var topicIds = sections.Select(s => s.TopicId).Distinct().ToList();
        var topicTitleById = new Dictionary<int, string>();
        foreach (var topicId in topicIds)
        {
            var t = await _topicService.GetTopicByIdAsync(topicId);
            topicTitleById[topicId] = (t == null ? "(topic no encontrado)" :
                string.IsNullOrWhiteSpace(t.Title) ? t.SystemName : t.Title);
        }

        var vm = sections
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ZiPageSectionListItemModel
            {
                Id = s.Id,
                PageId = s.PageId,
                TopicId = s.TopicId,
                TopicTitle = topicTitleById.TryGetValue(s.TopicId, out var tt) ? tt : "(topic no encontrado)",
                Name = s.Name,
                DisplayOrder = s.DisplayOrder
            })
            .ToList();

        return PartialView(VIEW_ROOT + "_SectionsList.cshtml", vm);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var entity = await _pageService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction(nameof(List));

        var currentLanguage = await _workContext.GetWorkingLanguageAsync();

        var model = new ZiPageModel
        {
            Id = entity.Id,
            Title = entity.Title,
            SystemName = entity.SystemName,
            Published = entity.Published,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            SeName = await _urlRecordService.GetSeNameAsync(entity, currentLanguage.Id)
        };

        return View(VIEW_ROOT + "Edit.cshtml", model); // 👈
    }


    //<------------------------------------------------->


    [HttpPost("Create"), ValidateAntiForgeryToken]
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

    [HttpPost("Edit/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ZiPageModel model, int id)
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

    [HttpPost("Delete")]
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

    // POST: guardar sección + campos + valores
    [HttpPost("AddSection"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSection(ZiPageSectionCreateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(Defaults.Plugins.Zimaltec.CustomPages.Permissions
                .MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        if (!ModelState.IsValid)
        {
            ViewBag.TemplateTopics = await _pageSectionService.GetTemplateTopicsAsync();
            return View(VIEW_ROOT + "AddSection.cshtml", model);
        }

        await _pageSectionService.CreateSectionWithFieldsAsync(model);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.SAVED));

        return RedirectToAction(nameof(Edit), new { id = model.PageId });
    }

    // POST: sincronizar placeholders desde la plantilla (nuevos=Text, obsoletos=marcados)
    [HttpPost("SyncSectionPlaceholders"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncSectionPlaceholders(int id, int pageId)
    {
        if (!await _permissionService.AuthorizeAsync(Defaults.Plugins.Zimaltec.CustomPages.Permissions
                .MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        await _pageSectionService.SyncFromTemplateAsync(id, createNewAsText: true);
        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    [HttpPost("DeleteSection"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSection(int id, int pageId)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var s = await _pageSectionService.GetByIdAsync(id);
        if (s != null) await _pageSectionService.DeleteAsync(s);

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    [HttpPost("MoveSection"), ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveSection(int id, int pageId, int direction)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        var list = (await _pageSectionService.GetByPageIdAsync(pageId)).ToList();
        var idx = list.FindIndex(x => x.Id == id);
        if (idx < 0) return RedirectToAction(nameof(Edit), new { id = pageId });

        var swapIdx = direction < 0 ? idx - 1 : idx + 1;
        if (swapIdx < 0 || swapIdx >= list.Count)
            return RedirectToAction(nameof(Edit), new { id = pageId });

        (list[idx].DisplayOrder, list[swapIdx].DisplayOrder) =
            (list[swapIdx].DisplayOrder, list[idx].DisplayOrder);

        await _pageSectionService.UpdateAsync(list[idx]);
        await _pageSectionService.UpdateAsync(list[swapIdx]);

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    [HttpPost("SaveTopicTemplateFlag")]
    public async Task<IActionResult> SaveTopicTemplateFlag(int id, bool isTemplate)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE))
            return Json(new { success = false });

        var topic = await _topicService.GetTopicByIdAsync(id);
        if (topic is null) return Json(new { success = false });

        await _ga.SaveAttributeAsync(topic, "CustomPages.IsTemplate", isTemplate);
        return Json(new { success = true });
    }

    // POST: Editar sección (guardar)
    [HttpPost("EditSection/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSection(int id, ZiPageSectionEditFullModel model)
    {
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return AccessDeniedView();

        await _pageSectionService.SaveSectionEditAsync(
            model.Id,
            model.TopicId,
            model.Name,
            model.Fields
        );

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.SAVED));

        return RedirectToAction(nameof(Edit), new { id = model.PageId });
    }

    // === Helper: parsea el ValueJson al modelo de edición ===
    private static void FillItemFromValueJson(
        ZiSectionFieldEditItemModel item,
        string? valueJson,
        FieldType type)
    {
        if (string.IsNullOrWhiteSpace(valueJson))
            return;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(valueJson);
            var root = doc.RootElement;

            switch (type)
            {
                case FieldType.Text:
                    item.TextValue = root.ValueKind == System.Text.Json.JsonValueKind.String
                        ? root.GetString()
                        : root.ToString();
                    break;

                case FieldType.RichText:
                    item.RichTextValue = root.ValueKind == System.Text.Json.JsonValueKind.String
                        ? root.GetString()
                        : root.ToString();
                    break;

                case FieldType.Image:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        root.TryGetProperty("pictureId", out var pid) && pid.TryGetInt32(out var p))
                        item.PictureId = p;
                    break;

                case FieldType.File:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        root.TryGetProperty("downloadId", out var did) && did.TryGetInt32(out var d))
                        item.DownloadId = d;
                    break;

                case FieldType.Number:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Number && root.TryGetInt32(out var n))
                        item.IntValue = n;
                    break;

                case FieldType.Decimal:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Number && root.TryGetDecimal(out var de))
                        item.DecimalValue = de;
                    break;

                case FieldType.Bool:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.True ||
                        root.ValueKind == System.Text.Json.JsonValueKind.False)
                        item.BoolValue = root.GetBoolean();
                    break;

                case FieldType.DateTime:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.String &&
                        DateTime.TryParse(root.GetString(), out var dt))
                        item.DateTimeValue = dt;
                    break;

                case FieldType.Link:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("text", out var lt)) item.LinkText = lt.GetString();
                        if (root.TryGetProperty("url", out var lu)) item.LinkUrl = lu.GetString();
                        if (root.TryGetProperty("target", out var lta)) item.LinkTarget = lta.GetString();
                    }

                    break;

                case FieldType.Entity:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("entityName", out var en)) item.EntityName = en.GetString();
                        if (root.TryGetProperty("id", out var eid) && eid.TryGetInt32(out var ei)) item.EntityId = ei;
                    }

                    break;

                case FieldType.Json:
                    item.JsonValue = valueJson;
                    break;

                case FieldType.List:
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        root.TryGetProperty("items", out var it) &&
                        it.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        item.ListItems = new List<string>();
                        foreach (var e in it.EnumerateArray())
                            item.ListItems.Add(e.ValueKind == System.Text.Json.JsonValueKind.String
                                ? (e.GetString() ?? "")
                                : e.ToString());
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        catch
        {
            // Si no parsea, lo dejamos en crudo (útil para Json)
            item.JsonValue ??= valueJson;
        }
    }
}