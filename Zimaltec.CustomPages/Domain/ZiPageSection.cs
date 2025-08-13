using Nop.Core;

namespace Nop.Plugin.Zimaltec.CustomPages.Domain;

public class ZiPageSection : BaseEntity
{
    public int PageId { get; set; }
    public int TopicId { get; set; }

    public string? Name { get; set; }
    public int DisplayOrder { get; set; }

    // Hash del snapshot de placeholders del Topic
    public string? TemplateSnapshotHash { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}