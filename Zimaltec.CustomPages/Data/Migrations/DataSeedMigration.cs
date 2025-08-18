using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Zimaltec.CustomPages.Constants;
using LAdmin = Nop.Plugin.Zimaltec.CustomPages.Constants.Defaults.Plugins.Zimaltec.CustomPages.Localization.Admin;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Migrations;

[NopSchemaMigration("2025/08/18 21:00:00:0000000", "Nop.Plugin.Zimaltec.CustomPages data seed",
    MigrationProcessType.NoMatter)]
public class DataSeedMigration(INopDataProvider dataProvider) : Migration
{
    public override void Up()
    {
        EnsureSpanishLanguagePublished();
        EnsurePermissionAndMapToAdministrators();
        AddOrUpdateAllTranslations();
    }

    public override void Down() { }

    // ------------------------------
    // Idioma: asegurar ES publicado
    // ------------------------------
    private void EnsureSpanishLanguagePublished()
    {
        var es = dataProvider.GetTable<Language>()
            .FirstOrDefault(l => l.LanguageCulture.ToLower() == "es-es");

        if (es is null)
        {
            Insert.IntoTable(nameof(Language)).Row(new
            {
                Name = "Spanish",
                LanguageCulture = "es-ES",
                UniqueSeoCode = "es",
                FlagImageFileName = "es.png",
                Rtl = false,
                Published = true,
                DisplayOrder = 2
            });
        }
        else if (!es.Published)
        {
            Execute.Sql($"UPDATE [{nameof(Language)}] SET [Published] = 1 WHERE [Id] = {es.Id}");
        }
    }

    // -----------------------------------------------------
    // Permiso: crear si no existe + mapear a Administrators
    // -----------------------------------------------------
    private void EnsurePermissionAndMapToAdministrators()
    {
        const string sysName = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME;

        var perm = dataProvider.GetTable<PermissionRecord>()
            .FirstOrDefault(p => p.SystemName == sysName);

        if (perm is null)
        {
            Insert.IntoTable(nameof(PermissionRecord)).Row(new
            {
                Name = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_NAME,
                SystemName = sysName,
                Category = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_CATEGORY
            });

            // Reconsulta para obtener Id
            perm = dataProvider.GetTable<PermissionRecord>()
                .FirstOrDefault(p => p.SystemName == sysName);
        }

        if (perm is null)
            return;

        var adminRole = dataProvider.GetTable<CustomerRole>()
            .FirstOrDefault(r => r.SystemName == NopCustomerDefaults.AdministratorsRoleName);

        if (adminRole is null)
            return;

        // Solo existe PermissionRecord_Role_Mapping en nop (y Customer_CustomerRole_Mapping, que no aplica aquí)
        Execute.Sql($@"
                    IF NOT EXISTS (
                        SELECT 1 FROM [PermissionRecord_Role_Mapping]
                        WHERE [CustomerRole_Id] = {adminRole.Id} AND [PermissionRecord_Id] = {perm.Id}
                    )
                    BEGIN
                        INSERT INTO [PermissionRecord_Role_Mapping] ([CustomerRole_Id], [PermissionRecord_Id])
                        VALUES ({adminRole.Id}, {perm.Id});
                    END
                    ");
    }

    // ----------------------------------------
    // Traducciones: EN y ES (todas las claves)
    // ----------------------------------------
    private void AddOrUpdateAllTranslations()
    {
        var languages = dataProvider.GetTable<Language>().ToList();

        var englishIds = languages
            .Where(l => l.LanguageCulture.StartsWith("en", StringComparison.InvariantCultureIgnoreCase))
            .Select(l => l.Id).ToList();

        var spanishIds = languages
            .Where(l => l.LanguageCulture.StartsWith("es", StringComparison.InvariantCultureIgnoreCase))
            .Select(l => l.Id).ToList();

        var en = BuildEnglishTranslations();
        var es = BuildSpanishTranslations();

        foreach (var langId in englishIds)
            AddOrUpdateLocaleResources(dataProvider, en, langId);

        foreach (var langId in spanishIds)
            AddOrUpdateLocaleResources(dataProvider, es, langId);

        if (englishIds.Count == 0 && spanishIds.Count == 0 && languages.Count > 0)
            AddOrUpdateLocaleResources(dataProvider, en, languages[0].Id);
    }

    private static Dictionary<string, string> BuildEnglishTranslations()
    {
        return new Dictionary<string, string>
        {
            // Menu
            [LAdmin.Menu.ROOT] = "Custom Pages",
            [LAdmin.Menu.PAGES] = "Pages",
            [LAdmin.Menu.SECTIONS] = "Sections",

            // Pages
            [LAdmin.Pages.LIST] = "Custom pages",
            [LAdmin.Pages.ADD_NEW] = "Add new page",
            [LAdmin.Pages.EDIT] = "Edit page",
            [LAdmin.Pages.TITLE] = "Title",
            [LAdmin.Pages.SYSTEM_NAME] = "System name",
            [LAdmin.Pages.PUBLISHED] = "Published",
            [LAdmin.Pages.META_TITLE] = "Meta title",
            [LAdmin.Pages.META_DESCRIPTION] = "Meta description",
            [LAdmin.Pages.META_KEYWORDS] = "Meta keywords",
            [LAdmin.Pages.BACK_TO_LIST] = "Back to list",
            [LAdmin.Pages.TAB_SETTINGS] = "Settings",
            [LAdmin.Pages.SEO_SECTION] = "SEO",
            [LAdmin.Pages.SEO_SENAME_PLACEHOLDER] = "Leave empty to auto-generate from Title",
            [LAdmin.Pages.SEO_SENAME_HINT] =
                "If a slug already exists, leaving it empty keeps it; otherwise, it will be generated from the title.",

            // Sections
            [LAdmin.Sections.LIST] = "Sections",
            [LAdmin.Sections.ADD_NEW] = "Add new section",
            [LAdmin.Sections.TOPIC] = "Template topic",
            [LAdmin.Sections.NAME] = "Name",
            [LAdmin.Sections.DISPLAY_ORDER] = "Display order",
            [LAdmin.Sections.SYNC_BANNER] = "The template has changed since last sync.",
            [LAdmin.Sections.SYNC_NOW] = "Sync placeholders",
            [LAdmin.Sections.TAB_TITLE] = "Sections",
            [LAdmin.Sections.LIST_TITLE] = "Page sections",
            [LAdmin.Sections.LOADING] = "Loading sections",
            [LAdmin.Sections.BACK_TO_PAGE] = "Back to page",

            // Fields
            [LAdmin.Fields.TITLE] = "Fields",
            [LAdmin.Fields.OBSOLETE] = "Obsolete fields",
            [LAdmin.Fields.TYPE] = "Type",
            [LAdmin.Fields.SETTINGS] = "Settings",
            [LAdmin.Fields.VALUE] = "Value",
            [LAdmin.Fields.DELETE_OBSOLETE] = "Delete obsolete",
            [LAdmin.Fields.DELETE_OBSOLETE_CONFIRM] =
                "Are you sure? This will remove obsolete fields and their values.",
            [LAdmin.Fields.NO_OBSOLETE] = "No obsolete placeholders.",

            // Common
            [LAdmin.Common.SAVED] = "Saved successfully.",
            [LAdmin.Common.ERROR] = "An error occurred."
        };
    }

    private static Dictionary<string, string> BuildSpanishTranslations()
    {
        return new Dictionary<string, string>
        {
            // Menú
            [LAdmin.Menu.ROOT] = "Páginas personalizadas",
            [LAdmin.Menu.PAGES] = "Páginas",
            [LAdmin.Menu.SECTIONS] = "Secciones",

            // Páginas
            [LAdmin.Pages.LIST] = "Páginas personalizadas",
            [LAdmin.Pages.ADD_NEW] = "Añadir página",
            [LAdmin.Pages.EDIT] = "Editar página",
            [LAdmin.Pages.TITLE] = "Título",
            [LAdmin.Pages.SYSTEM_NAME] = "Nombre del sistema",
            [LAdmin.Pages.PUBLISHED] = "Publicado",
            [LAdmin.Pages.META_TITLE] = "Meta título",
            [LAdmin.Pages.META_DESCRIPTION] = "Meta descripción",
            [LAdmin.Pages.META_KEYWORDS] = "Meta palabras clave",
            [LAdmin.Pages.BACK_TO_LIST] = "Volver al listado",
            [LAdmin.Pages.TAB_SETTINGS] = "Ajustes",
            [LAdmin.Pages.SEO_SECTION] = "SEO",
            [LAdmin.Pages.SEO_SENAME_PLACEHOLDER] = "Dejar vacío para generar automáticamente desde el título",
            [LAdmin.Pages.SEO_SENAME_HINT] =
                "Si ya existe un slug, al dejarlo vacío se mantendrá; si no, se generará a partir del título.",

            // Secciones
            [LAdmin.Sections.LIST] = "Secciones",
            [LAdmin.Sections.ADD_NEW] = "Añadir sección",
            [LAdmin.Sections.TOPIC] = "Topic plantilla",
            [LAdmin.Sections.NAME] = "Nombre",
            [LAdmin.Sections.DISPLAY_ORDER] = "Orden",
            [LAdmin.Sections.SYNC_BANNER] = "La plantilla ha cambiado desde la última sincronización.",
            [LAdmin.Sections.SYNC_NOW] = "Sincronizar placeholders",
            [LAdmin.Sections.TAB_TITLE] = "Secciones",
            [LAdmin.Sections.LIST_TITLE] = "Secciones de la página",
            [LAdmin.Sections.LOADING] = "Cargando secciones",
            [LAdmin.Sections.BACK_TO_PAGE] = "Volver a la página",

            // Campos
            [LAdmin.Fields.TITLE] = "Campos",
            [LAdmin.Fields.OBSOLETE] = "Campos obsoletos",
            [LAdmin.Fields.TYPE] = "Tipo",
            [LAdmin.Fields.SETTINGS] = "Ajustes",
            [LAdmin.Fields.VALUE] = "Valor",
            [LAdmin.Fields.DELETE_OBSOLETE] = "Borrar obsoletos",
            [LAdmin.Fields.DELETE_OBSOLETE_CONFIRM] =
                "¿Estás seguro? Esto eliminará los campos obsoletos y sus valores?",
            [LAdmin.Fields.NO_OBSOLETE] = "No hay placeholders obsoletos.",

            // Común
            [LAdmin.Common.SAVED] = "Guardado correctamente.",
            [LAdmin.Common.ERROR] = "Se ha producido un error."
        };
    }

    private static void AddOrUpdateLocaleResources(
        INopDataProvider dataProvider,
        IDictionary<string, string> resources,
        int languageId,
        bool overwriteExisting = true)
    {
        if (resources.Count <= 0)
            return;

        var existing = dataProvider.GetTable<LocaleStringResource>()
            .Where(r => r.LanguageId == languageId)
            .ToList();

        foreach (var (name, value) in resources)
        {
            var found = existing.FirstOrDefault(r =>
                (r.ResourceName ?? string.Empty).ToLowerInvariant() == name.ToLowerInvariant());

            if (found == null)
                dataProvider.InsertEntity(new LocaleStringResource
                {
                    LanguageId = languageId, ResourceName = name, ResourceValue = value
                });
            else if (overwriteExisting &&
                     (found.ResourceValue ?? string.Empty).ToLowerInvariant()
                     != value.ToLowerInvariant())
            {
                found.ResourceValue = value;
                dataProvider.UpdateEntity(found);
            }
        }
    }
}