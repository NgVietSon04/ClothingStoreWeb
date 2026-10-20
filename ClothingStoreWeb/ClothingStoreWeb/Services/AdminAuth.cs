namespace ClothingStoreWeb.Services;

public static class AdminAuth
{
    /// <summary>Tên scheme cookie riêng cho khu vực Admin (không đụng tới đăng nhập của khách).</summary>
    public const string Scheme = "AdminCookie";

    public const string Role = "Admin";
    public const string LoginPath = "/Admin/Auth/Login";
    public const string DeniedPath = "/Admin/Auth/Denied";
}
