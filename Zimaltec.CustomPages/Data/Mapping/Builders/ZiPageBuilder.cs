using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Mapping.Builders;

public class ZiPageBuilder : NopEntityBuilder<ZiPage>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ZiPage.SystemName)).AsString(400).NotNullable()
            .WithColumn(nameof(ZiPage.Title)).AsString(400).NotNullable()
            .WithColumn(nameof(ZiPage.Published)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(ZiPage.MetaTitle)).AsString(400).Nullable()
            .WithColumn(nameof(ZiPage.MetaDescription)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ZiPage.MetaKeywords)).AsString(400).Nullable()
            .WithColumn(nameof(ZiPage.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(ZiPage.UpdatedOnUtc)).AsDateTime2().NotNullable();
    }
}