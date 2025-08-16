using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Zimaltec.CustomPages.Constants;

namespace Nop.Plugin.Zimaltec.CustomPages.EventConsumers;

/// <summary>
/// Añade el menú "Custom Pages" en el panel de administración cuando se construye el menú.
/// </summary>
public class CustomPagesAdminMenuConsumer : IConsumer<AdminMenuCreatedEvent>
{
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    public CustomPagesAdminMenuConsumer(
        IPermissionService permissionService,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Mostrar el menú solo si el usuario tiene permiso
        if (!await _permissionService.AuthorizeAsync(
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME))
            return;

        // Títulos localizados (sin especificar languageId -> usa el idioma de trabajo del admin)
        var rootTitle  = await _localizationService.GetResourceAsync(
                             Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Menu.ROOT) 
                         ?? "Custom Pages";

        var pagesTitle = await _localizationService.GetResourceAsync(
                             Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Menu.PAGES) 
                         ?? "Pages";

        // Nodo raíz del plugin (nivel superior, si luego quieres moverlo, puedes usar InsertBefore/After)
        var rootNode = new AdminMenuItem
        {
            SystemName = "Zimaltec.CustomPages",
            Title = rootTitle,
            IconClass = "far fa-file",
            Visible = true,
            PermissionNames = new List<string>
            {
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME
            }
        };

        // Hijo: Pages → apunta a tu controller/acción
        // IMPORTANTE: Usa una URL con al menos 3 segmentos para que AdminMenuItem derive Controller/Action.
        // "Admin/CustomPagesAdmin/List" => ControllerName="CustomPagesAdmin", ActionName="List"
        rootNode.ChildNodes.Add(new AdminMenuItem
        {
            SystemName = "Zimaltec.CustomPages.Pages",
            Title = pagesTitle,
            Url = "~/Admin/CustomPagesAdmin/List",
            IconClass = "far fa-dot-circle",
            Visible = true,
            PermissionNames = new List<string>
            {
                Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME
            }
        });

        // Añadir al menú raíz
        eventMessage.RootMenuItem.ChildNodes.Add(rootNode);

        // Si prefieres insertarlo antes de un nodo existente (ej. "Help"):
        // eventMessage.RootMenuItem.InsertBefore("Help", rootNode);
    }
}