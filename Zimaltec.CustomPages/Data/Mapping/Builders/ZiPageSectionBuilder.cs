using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Topics;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Data.Mapping.Builders;

public class ZiPageSectionBuilder : NopEntityBuilder<ZiPageSection>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ZiPageSection.PageId))
            .AsInt32().ForeignKey<ZiPage>(onDelete: Rule.Cascade).NotNullable()
            .WithColumn(nameof(ZiPageSection.TopicId))
            .AsInt32().ForeignKey<Topic>(onDelete: Rule.None).NotNullable()
            .WithColumn(nameof(ZiPageSection.Name)).AsString(400).Nullable()
            .WithColumn(nameof(ZiPageSection.DisplayOrder)).AsInt32().NotNullable()
            .WithColumn(nameof(ZiPageSection.TemplateSnapshotHash)).AsString(1000).Nullable()
            .WithColumn(nameof(ZiPageSection.CreatedOnUtc)).AsDateTime2().NotNullable()
            .WithColumn(nameof(ZiPageSection.UpdatedOnUtc)).AsDateTime2().NotNullable();
    }
}