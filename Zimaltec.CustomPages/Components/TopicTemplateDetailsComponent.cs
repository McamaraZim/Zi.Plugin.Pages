using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Services.Common;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Models.Topics;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Zimaltec.CustomPages.Components;

[ViewComponent(Name = "TopicTemplateDetails")]
public class TopicTemplateDetailsComponent : NopViewComponent
{
    private readonly IPermissionService _permissionService;
    private readonly IGenericAttributeService _ga;
    private readonly ITopicService _topicService;

    private const string TOPIC_IS_TEMPLATE = "CustomPages.IsTemplate";

    public TopicTemplateDetailsComponent(IPermissionService permissionService,
        IGenericAttributeService ga, ITopicService topicService)
    {
        _permissionService = permissionService;
        _ga = ga;
        _topicService = topicService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Permisos típicos de Topics
        if (!await _permissionService.AuthorizeAsync(StandardPermission.ContentManagement.TOPICS_CREATE_EDIT_DELETE))
            return Content(string.Empty);

        if (additionalData is not TopicModel topicModel)
            return Content(string.Empty);

        var topic = topicModel.Id > 0 ? await _topicService.GetTopicByIdAsync(topicModel.Id) : null;
        var isTemplate = topic != null && await _ga.GetAttributeAsync<bool>(topic, TOPIC_IS_TEMPLATE);

        var vm = new TopicTemplateFlagModel
        {
            TopicId = topicModel.Id, IsTemplate = isTemplate, Body = topicModel.Body // para previsualizar placeholders
        };
        
        return View("~/Plugins/Zimaltec.CustomPages/Areas/Admin/Views/Topics/_TemplateFlag.cshtml", vm);
    }
}

public class TopicTemplateFlagModel
{
    public int TopicId { get; set; }
    public bool IsTemplate { get; set; }
    public string? Body { get; set; }
}