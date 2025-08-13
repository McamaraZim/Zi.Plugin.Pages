using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Migrations;

[NopMigration("2025/08/11 21:16:00:0000000", "Nop.Plugin.Zimaltec.CustomPages indices & constraints")]
public class ConstraintsMigration : Migration
{
    public override void Up()
    {
        // Unique (PageSectionId, Key) para evitar duplicados de placeholder en la misma sección
        Create.Index("IX_ZiSectionField_PageSection_Key")
            .OnTable(nameof(ZiSectionField))
            .OnColumn(nameof(ZiSectionField.PageSectionId)).Ascending()
            .OnColumn(nameof(ZiSectionField.Key)).Ascending()
            .WithOptions().Unique();

        // Ordenación y acceso rápido: secciones por página
        Create.Index("IX_ZiPageSection_Page_DisplayOrder")
            .OnTable(nameof(ZiPageSection))
            .OnColumn(nameof(ZiPageSection.PageId)).Ascending()
            .OnColumn(nameof(ZiPageSection.DisplayOrder)).Ascending();

        // Acceso rápido a valores por campo
        Create.Index("IX_ZiSectionFieldValue_Field")
            .OnTable(nameof(ZiSectionFieldValue))
            .OnColumn(nameof(ZiSectionFieldValue.SectionFieldId)).Ascending();
    }

    public override void Down()
    {
        Delete.Index("IX_ZiSectionField_PageSection_Key").OnTable(nameof(ZiSectionField));
        Delete.Index("IX_ZiPageSection_Page_DisplayOrder").OnTable(nameof(ZiPageSection));
        Delete.Index("IX_ZiSectionFieldValue_Field").OnTable(nameof(ZiSectionFieldValue));
    }
}