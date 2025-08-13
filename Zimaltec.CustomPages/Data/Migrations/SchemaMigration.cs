using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Migrations;

// Fecha formateada como en tus otras migraciones
[NopSchemaMigration("2025/08/11 21:15:00:0000000", "Nop.Plugin.Zimaltec.CustomPages schema", MigrationProcessType.NoMatter)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        // Crea las 4 tablas basadas en los Builders
        Create.TableFor<ZiPage>();
        Create.TableFor<ZiPageSection>();
        Create.TableFor<ZiSectionField>();
        Create.TableFor<ZiSectionFieldValue>();
    }
}