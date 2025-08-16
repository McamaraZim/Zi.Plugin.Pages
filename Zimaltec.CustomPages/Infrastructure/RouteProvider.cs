using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Services.Seo;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Zimaltec.CustomPages.Infrastructure;
/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var sp = endpointRouteBuilder.ServiceProvider;
        var urlRecordService = sp.GetRequiredService<IUrlRecordService>();

        var constraint = new ZiPageSlugConstraint(urlRecordService);

        endpointRouteBuilder.MapControllerRoute(
            name: "ZiPagePublicBySlug",
            pattern: "{SeName}",
            defaults: new { controller = "CustomPagesPublic", action = "Details", area = "" },
            constraints: new { SeName = constraint }
        );
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 10;
}