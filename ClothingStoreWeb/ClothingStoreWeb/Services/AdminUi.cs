using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ClothingStoreWeb.Services;

/// <summary>Các hàm tiện ích dùng trong view của Admin.</summary>
public static class AdminUi
{
    private static readonly CultureInfo Vi = new("vi-VN");

    /// <summary>Thư mục (URL) chứa ảnh. Ảnh upload được lưu vào wwwroot/uploads.</summary>
    public static string ImageBase { get; set; } = "/uploads/";

    public static readonly (string Text, string Value)[] PaymentMethods =
    {
        ("Tiền mặt", "0"),
        ("Chuyển khoản", "1"),
        ("Thẻ ngân hàng", "2"),
        ("Ví điện tử", "3"),
    };

    public static string Img(string? fileName)
        => string.IsNullOrWhiteSpace(fileName) ? "" : ImageBase + fileName;

    public static string Money(decimal? v)
        => v.HasValue ? v.Value.ToString("#,##0", Vi) + " ₫" : "—";

    public static string MoneyD(double? v)
        => v.HasValue ? ((decimal)v.Value).ToString("#,##0", Vi) + " ₫" : "—";

    public static string Trunc(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max) + "…";
    }

    public static string Payment(byte? v)
        => PaymentMethods.FirstOrDefault(p => p.Value == v?.ToString()).Text ?? "—";

    /// <summary>Tra tên hiển thị từ danh sách option trong ViewData (vd: BrandCountryIdOptions).</summary>
    public static string Lk(ViewDataDictionary vd, string key, object? value)
    {
        var s = value?.ToString() ?? "";
        if (s.Length == 0) return "—";
        var list = vd[key] as IEnumerable<SelectListItem>;
        return list?.FirstOrDefault(o => o.Value == s)?.Text ?? s;
    }

    public static string Num(decimal v) => v.ToString(CultureInfo.InvariantCulture);
    public static string Num(double v) => v.ToString(CultureInfo.InvariantCulture);
}
