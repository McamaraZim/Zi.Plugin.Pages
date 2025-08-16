using Nop.Core.Domain.Topics;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiSectionField;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Services;

public interface IZiPageSectionService
{
    Task<IList<Topic>> GetTemplateTopicsAsync();
    Task<IReadOnlyList<string>> GetPlaceholdersAsync(int topicId);
    Task<int> CreateSectionWithFieldsAsync(ZiPageSectionCreateModel model);

    Task<(int created, int markedObsolete, string snapshotHash)>
        SyncFromTemplateAsync(int sectionId, bool createNewAsText = true);

    Task<IList<ZiPageSection>> GetByPageIdAsync(int pageId);
    Task<ZiPageSection?> GetByIdAsync(int id);
    Task UpdateAsync(ZiPageSection section);

    Task<(ZiPageSection section, List<(ZiSectionField field, ZiSectionFieldValue? val)>)>
        GetSectionDetailsAsync(int sectionId);

    Task SaveSectionEditAsync(int sectionId, int newTopicId, string? name,
        IEnumerable<ZiSectionFieldEditItemModel> fields);

    Task DeleteAsync(ZiPageSection entity);
}