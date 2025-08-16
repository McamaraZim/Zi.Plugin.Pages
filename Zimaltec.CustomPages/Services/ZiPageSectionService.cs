using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Services.Topics;
using Nop.Services.Common;
using System.Text.Json;
using Nop.Core;
using Nop.Plugin.Zimaltec.CustomPages.Domain;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiPageSection;
using Nop.Plugin.Zimaltec.CustomPages.Areas.Admin.Models.ZiSectionField;
using static Nop.Plugin.Zimaltec.CustomPages.Services.TopicTemplatePlaceholderParser;

namespace Nop.Plugin.Zimaltec.CustomPages.Services;

public class ZiPageSectionService : IZiPageSectionService
{
    private readonly IRepository<ZiPageSection> _sectionRepo;
    private readonly IRepository<ZiSectionField> _fieldRepo;
    private readonly IRepository<ZiSectionFieldValue> _valRepo;
    private readonly ITopicService _topicService;
    private readonly IStoreContext _storeContext;
    private readonly IGenericAttributeService _ga;

    private const string TOPIC_IS_TEMPLATE = "CustomPages.IsTemplate";

    public ZiPageSectionService(
        IRepository<ZiPageSection> sectionRepo,
        IRepository<ZiSectionField> fieldRepo,
        IRepository<ZiSectionFieldValue> valRepo,
        ITopicService topicService,
        IStoreContext storeContext,
        IGenericAttributeService ga)
    {
        _sectionRepo = sectionRepo;
        _fieldRepo = fieldRepo;
        _valRepo = valRepo;
        _topicService = topicService;
        _storeContext = storeContext;
        _ga = ga;
    }

    public async Task<IList<Topic>> GetTemplateTopicsAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var all = await _topicService.GetAllTopicsAsync(store.Id, showHidden: true);
        var list = new List<Topic>();
        foreach (var t in all)
            if (await _ga.GetAttributeAsync<bool>(t, TOPIC_IS_TEMPLATE))
                list.Add(t);
        return list.OrderBy(t => t.SystemName).ToList();
    }

    public async Task<IReadOnlyList<string>> GetPlaceholdersAsync(int topicId)
    {
        var topic = await _topicService.GetTopicByIdAsync(topicId) ?? throw new ArgumentException("Topic not found");
        return Parse(topic.Body);
    }

    public async Task<int> CreateSectionWithFieldsAsync(ZiPageSectionCreateModel model)
    {
        var topic = await _topicService.GetTopicByIdAsync(model.TopicId)
                    ?? throw new ArgumentException("Topic not found");
        if (!await _ga.GetAttributeAsync<bool>(topic, TOPIC_IS_TEMPLATE))
            throw new InvalidOperationException("Topic is not marked as template.");

        var keys = Parse(topic.Body);
        var now = DateTime.UtcNow;

        var section = new ZiPageSection
        {
            PageId = model.PageId,
            TopicId = model.TopicId,
            Name = string.IsNullOrWhiteSpace(model.Name) ? topic.SystemName : model.Name,
            DisplayOrder = model.DisplayOrder ?? int.MaxValue,
            TemplateSnapshotHash = ComputeSnapshotHash(keys),
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        };
        await _sectionRepo.InsertAsync(section);

        // Asegurar Fields de la petición coinciden con keys del Topic
        var map = model.Fields.ToDictionary(f => NormalizeKey(f.Key));
        var display = 0;
        foreach (var k in keys)
        {
            if (!map.TryGetValue(k, out var posted))
                posted = new SectionFieldPostModel { Key = k, Type = FieldType.Text };

            var field = new ZiSectionField
            {
                PageSectionId = section.Id,
                Key = k,
                Type = (int)posted.Type,
                SettingsJson = null,
                IsObsolete = false,
                DisplayOrder = display++,
                CreatedOnUtc = now,
                UpdatedOnUtc = now
            };
            await _fieldRepo.InsertAsync(field);

            var json = BuildValueJson(posted);
            await _valRepo.InsertAsync(new ZiSectionFieldValue
            {
                SectionFieldId = field.Id, ValueJson = json, UpdatedOnUtc = now
            });
        }

        return section.Id;
    }

    public async Task<(int created, int markedObsolete, string snapshotHash)>
        SyncFromTemplateAsync(int sectionId, bool createNewAsText = true)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId) ?? throw new ArgumentException("Section not found");
        var topic = await _topicService.GetTopicByIdAsync(section.TopicId) ??
                    throw new ArgumentException("Topic not found");
        if (!await _ga.GetAttributeAsync<bool>(topic, TOPIC_IS_TEMPLATE))
            throw new InvalidOperationException("Topic not marked as template.");

        var keys = Parse(topic.Body); // p.ej. ["cuerpo","nombre"]
        var hash = ComputeSnapshotHash(keys);

        var fields = await _fieldRepo.Table.Where(f => f.PageSectionId == sectionId).ToListAsync();
        var fieldsByKey = fields.ToDictionary(f => f.Key, StringComparer.InvariantCultureIgnoreCase);

        var created = 0;

        // 1) Asegurar existencia y reactivar si estaba obsoleto; actualizar DisplayOrder según orden de plantilla
        for (var i = 0; i < keys.Count; i++)
        {
            var k = keys[i];
            if (fieldsByKey.TryGetValue(k, out var f))
            {
                if (f.IsObsolete) f.IsObsolete = false; // 🔁 reactivar
                if (f.DisplayOrder != i) f.DisplayOrder = i; // opcional: respetar orden de la plantilla
                f.UpdatedOnUtc = DateTime.UtcNow;
                await _fieldRepo.UpdateAsync(f);
            }
            else
            {
                var nf = new ZiSectionField
                {
                    PageSectionId = sectionId,
                    Key = k,
                    Type = createNewAsText ? (int)FieldType.Text : (int)FieldType.Json,
                    DisplayOrder = i,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    IsObsolete = false
                };
                await _fieldRepo.InsertAsync(nf);
                await _valRepo.InsertAsync(new ZiSectionFieldValue
                {
                    SectionFieldId = nf.Id, ValueJson = null, UpdatedOnUtc = DateTime.UtcNow
                });
                created++;
            }
        }

        // 2) Marcar obsoletos los que ya no están
        var keySet = keys.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var obsolete = fields.Where(f => !keySet.Contains(f.Key)).ToList();
        foreach (var f in obsolete.Where(f => !f.IsObsolete))
        {
            f.IsObsolete = true;
            f.UpdatedOnUtc = DateTime.UtcNow;
            await _fieldRepo.UpdateAsync(f);
        }

        // 3) Actualizar snapshot
        section.TemplateSnapshotHash = hash;
        section.UpdatedOnUtc = DateTime.UtcNow;
        await _sectionRepo.UpdateAsync(section);

        return (created, obsolete.Count, hash);
    }

    public async Task<IList<ZiPageSection>> GetByPageIdAsync(int pageId)
    {
        return await _sectionRepo.Table.Where(s => s.PageId == pageId)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ZiPageSection?> GetByIdAsync(int id)
    {
        return await _sectionRepo.GetByIdAsync(id);
    }

    public async Task UpdateAsync(ZiPageSection entity)
    {
        await _sectionRepo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(ZiPageSection entity)
    {
        await _sectionRepo.DeleteAsync(entity);
    }

    public async Task<(ZiPageSection section, List<(ZiSectionField field, ZiSectionFieldValue? val)>)>
        GetSectionDetailsAsync(int sectionId)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
                      ?? throw new ArgumentException("Section not found");

        // Fields (ordenados) + values (left join)
        var fields = await _fieldRepo.Table
            .Where(f => f.PageSectionId == sectionId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        var fieldIds = fields.Select(f => f.Id).ToList();

        var values = await _valRepo.Table
            .Where(v => fieldIds.Contains(v.SectionFieldId))
            .ToListAsync();

        var byId = values.ToDictionary(v => v.SectionFieldId);

        var list = new List<(ZiSectionField, ZiSectionFieldValue?)>(fields.Count);
        list.AddRange(fields.Select(f => (f, byId.TryGetValue(f.Id, out var v) ? v : null)));

        return (section, list);
    }

    public async Task SaveSectionEditAsync(
        int sectionId,
        int newTopicId,
        string? name,
        IEnumerable<ZiSectionFieldEditItemModel> postedFields)
    {
        var section = await _sectionRepo.GetByIdAsync(sectionId)
                      ?? throw new ArgumentException("Section not found");

        // 1) Si cambia la plantilla, sincroniza keys (crear/activar/hacer obsoleto)
        if (newTopicId != section.TopicId)
        {
            var topic = await _topicService.GetTopicByIdAsync(newTopicId)
                        ?? throw new ArgumentException("Template topic not found");
            if (!await _ga.GetAttributeAsync<bool>(topic, TOPIC_IS_TEMPLATE))
                throw new InvalidOperationException("Topic not marked as template.");

            var newKeys = Parse(topic.Body);
            var newHash = ComputeSnapshotHash(newKeys);

            // Cargar fields actuales
            var fields = await _fieldRepo.Table
                .Where(f => f.PageSectionId == sectionId)
                .ToListAsync();

            var fieldsByKey = fields.ToDictionary(f => f.Key, StringComparer.InvariantCultureIgnoreCase);

            // Reactivar/crear y ordenar según la plantilla nueva
            for (var i = 0; i < newKeys.Count; i++)
            {
                var k = newKeys[i];
                if (fieldsByKey.TryGetValue(k, out var f))
                {
                    if (f.IsObsolete) f.IsObsolete = false;
                    f.DisplayOrder = i;
                    f.UpdatedOnUtc = DateTime.UtcNow;
                    await _fieldRepo.UpdateAsync(f);
                }
                else
                {
                    var nf = new ZiSectionField
                    {
                        PageSectionId = sectionId,
                        Key = k,
                        Type = (int)FieldType.Text, // por defecto
                        SettingsJson = null,
                        IsObsolete = false,
                        DisplayOrder = i,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    await _fieldRepo.InsertAsync(nf);

                    await _valRepo.InsertAsync(new ZiSectionFieldValue
                    {
                        SectionFieldId = nf.Id, ValueJson = null, UpdatedOnUtc = DateTime.UtcNow
                    });

                    fieldsByKey[k] = nf;
                }
            }

            // Marcar obsoletos los que ya no estén
            var newSet = new HashSet<string>(newKeys, StringComparer.InvariantCultureIgnoreCase);
            foreach (var f in fields.Where(f => !newSet.Contains(f.Key) && !f.IsObsolete))
            {
                f.IsObsolete = true;
                f.UpdatedOnUtc = DateTime.UtcNow;
                await _fieldRepo.UpdateAsync(f);
            }

            // Actualizar la sección
            section.TopicId = newTopicId;
            section.TemplateSnapshotHash = newHash;
            section.UpdatedOnUtc = DateTime.UtcNow;
            await _sectionRepo.UpdateAsync(section);
        }

        // 2) Guardar cambios de nombre
        if (name != section.Name)
        {
            section.Name = name?.Trim();
            section.UpdatedOnUtc = DateTime.UtcNow;
            await _sectionRepo.UpdateAsync(section);
        }

        // 3) Guardar tipos + valores de cada field posteado
        //    (los que no vengan posteados no se tocan)
        var existingFields = await _fieldRepo.Table
            .Where(f => f.PageSectionId == sectionId)
            .ToListAsync();
        var fieldById = existingFields.ToDictionary(f => f.Id);

        // cache de values por field
        var existingValues = await _valRepo.Table
            .Where(v => v.SectionFieldId > 0 && fieldById.Keys.Contains(v.SectionFieldId))
            .ToListAsync();
        var valueByFieldId = existingValues.ToDictionary(v => v.SectionFieldId);

        foreach (var item in postedFields)
        {
            if (!fieldById.TryGetValue(item.FieldId, out var f))
                continue; // puede venir de una plantilla previa; ignoramos silencioso

            // Tipo + settings
            var newType = (int)item.Type;
            if (f.Type != newType || f.SettingsJson != item.SettingsJson)
            {
                f.Type = newType;
                f.SettingsJson = string.IsNullOrWhiteSpace(item.SettingsJson) ? null : item.SettingsJson;
                f.UpdatedOnUtc = DateTime.UtcNow;
                await _fieldRepo.UpdateAsync(f);
            }

            // Valor
            var json = BuildValueJson(item); // puede ser null
            if (valueByFieldId.TryGetValue(f.Id, out var val))
            {
                // Solo tocar si cambia
                if (json == val.ValueJson) continue;
                val.ValueJson = json;
                val.UpdatedOnUtc = DateTime.UtcNow;
                await _valRepo.UpdateAsync(val);
            }
            else
                await _valRepo.InsertAsync(new ZiSectionFieldValue
                {
                    SectionFieldId = f.Id, ValueJson = json, UpdatedOnUtc = DateTime.UtcNow
                });
        }
    }

    private static string? BuildValueJson(SectionFieldPostModel m)
    {
        var val = m.Type switch
        {
            FieldType.Text => m.TextValue,
            FieldType.RichText => m.RichTextValue,
            FieldType.Image => m.PictureId is > 0 ? new { pictureId = m.PictureId } : null,
            FieldType.File => m.DownloadId is > 0 ? new { downloadId = m.DownloadId } : null,
            FieldType.Number => m.IntValue,
            FieldType.Decimal => m.DecimalValue,
            FieldType.Bool => m.BoolValue,
            FieldType.DateTime => m.DateTimeValue,
            FieldType.Link => (!string.IsNullOrWhiteSpace(m.LinkUrl)
                ? new { text = m.LinkText, url = m.LinkUrl, target = m.LinkTarget }
                : null),
            FieldType.Entity => (!string.IsNullOrWhiteSpace(m.EntityName) && m.EntityId is > 0
                ? new { entityName = m.EntityName, id = m.EntityId }
                : null),
            FieldType.Json => string.IsNullOrWhiteSpace(m.JsonValue)
                ? null
                : JsonSerializer.Deserialize<object>(m.JsonValue!),
            FieldType.List => m.ListItems is { Count: > 0 } ? new { items = m.ListItems } : null,
            _ => m.TextValue
        };

        return val == null ? null : JsonSerializer.Serialize(val);
    }

    private static string? BuildValueJson(ZiSectionFieldEditItemModel m)
    {
        var val = m.Type switch
        {
            FieldType.Text => m.TextValue,
            FieldType.RichText => m.RichTextValue,
            FieldType.Image => m.PictureId is > 0 ? new { pictureId = m.PictureId } : null,
            FieldType.File => m.DownloadId is > 0 ? new { downloadId = m.DownloadId } : null,
            FieldType.Number => m.IntValue,
            FieldType.Decimal => m.DecimalValue,
            FieldType.Bool => m.BoolValue,
            FieldType.DateTime => m.DateTimeValue,
            FieldType.Link => (!string.IsNullOrWhiteSpace(m.LinkUrl)
                ? new { text = m.LinkText, url = m.LinkUrl, target = m.LinkTarget }
                : null),
            FieldType.Entity => (!string.IsNullOrWhiteSpace(m.EntityName) && m.EntityId is > 0
                ? new { entityName = m.EntityName, id = m.EntityId }
                : null),
            FieldType.Json => string.IsNullOrWhiteSpace(m.JsonValue)
                ? null
                : JsonSerializer.Deserialize<object>(m.JsonValue!),
            FieldType.List => (m.ListItems is { Count: > 0 })
                ? new { items = m.ListItems }
                : null,
            _ => m.TextValue
        };

        return val == null ? null : JsonSerializer.Serialize(val);
    }
}