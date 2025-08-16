using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Seo;

namespace Nop.Plugin.Zimaltec.CustomPages.Infrastructure;

// Restringe la ruta {SeName} únicamente a slugs cuya UrlRecord sea de ZiPage
public sealed class ZiPageSlugConstraint : IRouteConstraint
{
    private readonly IUrlRecordService _urlRecordService;

    public ZiPageSlugConstraint(IUrlRecordService urlRecordService)
    {
        _urlRecordService = urlRecordService;
    }

    public bool Match(HttpContext httpContext, IRouter route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        // Solo para matching de peticiones (no para generación de URL)
        if (routeDirection == RouteDirection.UrlGeneration)
            return false;

        if (!values.TryGetValue("SeName", out var v) || v is null)
            return false;

        var slug = v.ToString();
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        // Síncrono por firma del constraint (ok aquí)
        var record = _urlRecordService.GetBySlugAsync(slug).GetAwaiter().GetResult();
        return record is { IsActive: true } && record.EntityName.Equals("ZiPage", StringComparison.InvariantCultureIgnoreCase);
    }
}