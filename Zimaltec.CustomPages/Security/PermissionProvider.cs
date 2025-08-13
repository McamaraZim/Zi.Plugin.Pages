using Nop.Core.Domain.Security;
using Nop.Plugin.Zimaltec.CustomPages.Constants;

namespace Nop.Plugin.Zimaltec.CustomPages.Security;

public static class CustomPagesPermissions
{
    public static PermissionRecord Manage => new()
    {
        Name = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_NAME,
        SystemName = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_SYSTEM_NAME,
        Category = Defaults.Plugins.Zimaltec.CustomPages.Permissions.MANAGE_CATEGORY
    };
}