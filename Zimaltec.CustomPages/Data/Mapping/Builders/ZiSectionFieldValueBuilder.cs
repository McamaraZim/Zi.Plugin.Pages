using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Mapping.Builders;

public class ZiSectionFieldValueBuilder : NopEntityBuilder<ZiSectionFieldValue>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ZiSectionFieldValue.SectionFieldId))
            .AsInt32().ForeignKey<ZiSectionField>(onDelete: Rule.Cascade).NotNullable()
            .WithColumn(nameof(ZiSectionFieldValue.ValueJson)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ZiSectionFieldValue.UpdatedOnUtc)).AsDateTime2().NotNullable();
    }
}