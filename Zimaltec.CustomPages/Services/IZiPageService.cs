using Nop.Core;
using Nop.Plugin.Zimaltec.CustomPages.Domain;

namespace Nop.Plugin.Zimaltec.CustomPages.Services;

public interface IZiPageService
{
    Task<ZiPage?> GetByIdAsync(int id);
    Task<IPagedList<ZiPage>> SearchAsync(string? keywords, int pageIndex, int pageSize);
    Task InsertAsync(ZiPage page, string? seName);
    Task UpdateAsync(ZiPage page, string? seName);
    Task DeleteAsync(ZiPage page);
}