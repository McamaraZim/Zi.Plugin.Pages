using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Plugin.Zimaltec.CustomPages.Components;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Plugin.Zimaltec.CustomPages.Constants;
using Nop.Web.Framework.Infrastructure;
using Nop.Services.Cms;


namespace Nop.Plugin.Zimaltec.CustomPages;

public class CustomPagesPlugin : BasePlugin, IWidgetPlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly IPermissionService _permissionService;
    private readonly ICustomerService _customerService;
    private readonly ISettingService _settingService;

    public CustomPagesPlugin(
        ILocalizationService localizationService,
        ILanguageService languageService,
        IPermissionService permissionService,
        ICustomerService customerService,
        ISettingService settingService)
    {
        _localizationService = localizationService;
        _languageService = languageService;
        _permissionService = permissionService;
        _customerService = customerService;
        _settingService = settingService;
    }

    public override async Task InstallAsync()
    {
        // 1) Asegurar idioma ES publicado (por culture, sin fijar Id)
        var es = (await _languageService.GetAllLanguagesAsync(showHidden: true))
            .FirstOrDefault(l => l.LanguageCulture.Equals("es-ES", StringComparison.InvariantCultureIgnoreCase));

        if (es == null)
        {
            es = new Language
            {
                Name = "Spanish",
                LanguageCulture = "es-ES",
                UniqueSeoCode = "es",
                FlagImageFileName = "es.png",
                Rtl = false,
                Published = true,
                DisplayOrder = 2
            };
            await _languageService.InsertLanguageAsync(es);
        }
        else if (!es.Published)
        {
            es.Published = true;
            await _languageService.UpdateLanguageAsync(es);
        }

        // 2) Crear permiso si no existe
        var existing = (await _permissionService.GetAllPermissionRecordsAsync())
            .FirstOrDefault(p => p.SystemName == Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME);

        if (existing == null)
        {
            var perm = new PermissionRecord
            {
                Name = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_NAME,
                SystemName = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME,
                Category = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_CATEGORY
            };
            await _permissionService.InsertPermissionRecordAsync(perm);

            // Asignar a Administrators
            var adminRole =
                await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
            if (adminRole != null)
                await _permissionService.InsertPermissionMappingAsync(adminRole.Id, perm.SystemName);
        }

        // 3) Recursos EN (default) y ES (por id real)
        await AddOrUpdateResourcesAsync(null);
        await AddOrUpdateResourcesAsync(es.Id);
        
        // Activar este widget por defecto
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (!widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(widgetSettings);
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _permissionService.DeletePermissionAsync(Defaults.Plugins.Zimaltec.CustomPages.Permissions
            .MANAGE_SYSTEM_NAME);
        await _localizationService.DeleteLocaleResourcesAsync(Defaults.Plugins.Zimaltec.CustomPages.Localization.KEY);
        
        // Quitar este widget de los activos
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName))
            await _settingService.SaveSettingAsync(widgetSettings);
        
        await base.UninstallAsync();
    }

    private async Task AddOrUpdateResourcesAsync(int? languageId)
    {
        var isSpanish = languageId is > 0;

        var r = new Dictionary<string, string>
        {
            // Menu
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Menu.ROOT] =
                isSpanish ? "Páginas personalizadas" : "Custom Pages",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Menu.PAGES] = isSpanish ? "Páginas" : "Pages",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Menu.SECTIONS] =
                isSpanish ? "Secciones" : "Sections",

            // Pages
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.LIST] =
                isSpanish ? "Páginas personalizadas" : "Custom pages",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.ADD_NEW] =
                isSpanish ? "Añadir página" : "Add new page",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.EDIT] =
                isSpanish ? "Editar página" : "Edit page",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.TITLE] = isSpanish ? "Título" : "Title",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.SYSTEM_NAME] =
                isSpanish ? "Nombre del sistema" : "System name",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.PUBLISHED] =
                isSpanish ? "Publicado" : "Published",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.META_TITLE] =
                isSpanish ? "Meta título" : "Meta title",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.META_DESCRIPTION] =
                isSpanish ? "Meta descripción" : "Meta description",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Pages.META_KEYWORDS] =
                isSpanish ? "Meta palabras clave" : "Meta keywords",

            // Sections
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.LIST] =
                isSpanish ? "Secciones" : "Sections",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.ADD_NEW] =
                isSpanish ? "Añadir sección" : "Add new section",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.TOPIC] =
                isSpanish ? "Topic plantilla" : "Template topic",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.NAME] = isSpanish ? "Nombre" : "Name",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.DISPLAY_ORDER] =
                isSpanish ? "Orden" : "Display order",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.SYNC_BANNER] =
                isSpanish
                    ? "La plantilla ha cambiado desde la última sincronización."
                    : "The template has changed since last sync.",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Sections.SYNC_NOW] =
                isSpanish ? "Sincronizar placeholders" : "Sync placeholders",

            // Fields
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.TITLE] = isSpanish ? "Campos" : "Fields",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.OBSOLETE] =
                isSpanish ? "Campos obsoletos" : "Obsolete fields",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.TYPE] = isSpanish ? "Tipo" : "Type",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.SETTINGS] =
                isSpanish ? "Ajustes" : "Settings",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.VALUE] = isSpanish ? "Valor" : "Value",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.DELETE_OBSOLETE] =
                isSpanish ? "Borrar obsoletos" : "Delete obsolete",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.DELETE_OBSOLETE_CONFIRM] =
                isSpanish
                    ? "¿Estás seguro? Esto eliminará los campos obsoletos y sus valores."
                    : "Are you sure? This will remove obsolete fields and their values.",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Fields.NO_OBSOLETE] =
                isSpanish ? "No hay placeholders obsoletos." : "No obsolete placeholders.",

            // Common
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.SAVED] =
                isSpanish ? "Guardado correctamente." : "Saved successfully.",
            [Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin.Common.ERROR] =
                isSpanish ? "Se ha producido un error." : "An error occurred."
        };

        if (languageId is > 0)
            await _localizationService.AddOrUpdateLocaleResourceAsync(r, languageId.Value);
        else
            await _localizationService.AddOrUpdateLocaleResourceAsync(r);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            AdminWidgetZones.TopicDetailsBlock
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(TopicTemplateDetailsComponent);
    }

    public bool HideInWidgetList => false;
}