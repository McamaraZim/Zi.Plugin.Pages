using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Zimaltec.CustomPages.Domains;

namespace Nop.Plugin.Zimaltec.CustomPages.Migrations;
[NopMigration("2025-08-09 21:34:19", "Nop.Plugin.Zimaltec.CustomPages schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<CustomTable>();
    }
}