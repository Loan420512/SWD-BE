namespace StickerRobot.Constants;

public static class AuthRoles
{
    /// <summary>ADMIN.role values allowed to manage catalog, assets, and orders.</summary>
    public const string CatalogAdmins = UserRoles.Manager + "," + UserRoles.SuperAdmin;
}
