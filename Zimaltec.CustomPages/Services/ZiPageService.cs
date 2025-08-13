using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Services.Seo;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Services;

public class ZiPageService : IZiPageService
{
    private readonly IRepository<ZiPage> _pageRepo;
    private readonly IUrlRecordService _urlRecordService;
    private readonly SeoSettings _seoSettings;

    public ZiPageService(
        IRepository<ZiPage> pageRepo,
        IUrlRecordService urlRecordService,
        SeoSettings seoSettings)
    {
        _pageRepo = pageRepo;
        _urlRecordService = urlRecordService;
        _seoSettings = seoSettings;
    }

    public async Task<ZiPage?> GetByIdAsync(int id) => await _pageRepo.GetByIdAsync(id);

    public async Task<IPagedList<ZiPage>> SearchAsync(string? keywords, int pageIndex, int pageSize)
    {
        var query = _pageRepo.Table;

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(x => x.Title.Contains(keywords) || x.SystemName.Contains(keywords));

        query = query.OrderBy(x => x.Id);

        var total = await query.CountAsync();
        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<ZiPage>(items, pageIndex, pageSize, total);
    }

    public async Task InsertAsync(ZiPage page, string? seName)
    {
        await _pageRepo.InsertAsync(page);

        var validated = await ValidateSeNameAsync(page, seName);
        await _urlRecordService.SaveSlugAsync(page, validated, 0);
    }

    public async Task UpdateAsync(ZiPage page, string? seName)
    {
        await _pageRepo.UpdateAsync(page);

        var validated = await ValidateSeNameAsync(page, seName);
        await _urlRecordService.SaveSlugAsync(page, validated, 0);
    }

    public async Task DeleteAsync(ZiPage page)
    {
        await _pageRepo.DeleteAsync(page);
    }

    private async Task<string> ValidateSeNameAsync(ZiPage page, string? seName)
    {
        var baseText = string.IsNullOrWhiteSpace(seName) ? page.Title : seName;
        var validated = await _urlRecordService.ValidateSeNameAsync(page, baseText, baseText, true);
        return validated;
    }
}
