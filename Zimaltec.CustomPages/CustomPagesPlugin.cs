using Nop.Core.Domain.Cms;
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

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.TopicDetailsBlock });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(TopicTemplateDetailsComponent);
    }

    public bool HideInWidgetList => false;
}