using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Mapping.Builders;

public class ZiSectionFieldBuilder : NopEntityBuilder<ZiSectionField>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ZiSectionField.PageSectionId))
            .AsInt32().ForeignKey<ZiPageSection>(onDelete: Rule.Cascade).NotNullable()
            .WithColumn(nameof(ZiSectionField.Key)).AsString(400).NotNullable()
            .WithColumn(nameof(ZiSectionField.Type)).AsInt32().NotNullable()
            .WithColumn(nameof(ZiSectionField.SettingsJson)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ZiSectionField.IsObsolete)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(ZiSectionField.DisplayOrder)).AsInt32().NotNullable()
            .WithColumn(nameof(ZiSectionField.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(ZiSectionField.UpdatedOnUtc)).AsDateTime2().NotNullable();
    }
}