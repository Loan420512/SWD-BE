using System.ComponentModel.DataAnnotations;
using StickerRobot.Utilities;

namespace StickerRobot.DTO.DTORequest;

/// <summary>
/// Yêu cầu đăng nhập quản trị hệ thống.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Tên tài khoản quản trị (ADMIN.username).
    /// </summary>
    /// <example>manager</example>
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên đăng nhập tối đa 100 ký tự.")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Mật khẩu tài khoản (được băm bằng BCrypt trong cơ sở dữ liệu).
    /// </summary>
    /// <example>Manager@123</example>
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// Cấu hình tọa độ và hướng trục cho mô hình 3D của dòng máy điện thoại.
/// </summary>
public class PhoneModelFitRequest
{
    /// <summary>
    /// Đường dẫn tệp mô hình 3D GLB của điện thoại (trên máy chủ).
    /// </summary>
    /// <example>/uploads/phone/iphone-15-pro.glb</example>
    [Required(ErrorMessage = "Đường dẫn mô hình không được để trống.")]
    [MaxLength(500)]
    public string ModelUrl { get; set; } = null!;

    /// <summary>
    /// Trục mặt lưng điện thoại (chuỗi hợp lệ: '+X', '-X', '+Y', '-Y', '+Z', '-Z').
    /// </summary>
    /// <example>+Z</example>
    [Required(ErrorMessage = "Trục mặt lưng không được để trống.")]
    [RegularExpression(BackSheetMath.AxisPattern, ErrorMessage = "Trục phải có định dạng [+|-][X|Y|Z].")]
    public string BackAxis { get; set; } = null!;

    /// <summary>
    /// Trục đỉnh trên điện thoại (chuỗi hợp lệ: '+X', '-X', '+Y', '-Y', '+Z', '-Z').
    /// </summary>
    /// <example>+Y</example>
    [Required(ErrorMessage = "Trục hướng đỉnh không được để trống.")]
    [RegularExpression(BackSheetMath.AxisPattern, ErrorMessage = "Trục phải có định dạng [+|-][X|Y|Z].")]
    public string UpAxis { get; set; } = null!;
}

/// <summary>
/// Cấu hình thông số kỹ thuật, hướng trục và ma trận mặt nạ vùng dán (occupancy mask) cho ốp lưng.
/// </summary>
public class PhoneCaseFitRequest
{
    /// <summary>
    /// Đường dẫn tệp mô hình 3D GLB của ốp lưng.
    /// </summary>
    /// <example>/uploads/case/iphone-15-clear-case.glb</example>
    [Required(ErrorMessage = "Đường dẫn mô hình không được để trống.")]
    [MaxLength(500)]
    public string ModelUrl { get; set; } = null!;

    /// <summary>
    /// Trục hướng mặt lưng ốp (chuỗi hợp lệ: '+X', '-X', '+Y', '-Y', '+Z', '-Z').
    /// </summary>
    /// <example>+Z</example>
    [Required(ErrorMessage = "Trục mặt lưng không được để trống.")]
    [RegularExpression(BackSheetMath.AxisPattern, ErrorMessage = "Trục phải có định dạng [+|-][X|Y|Z].")]
    public string BackAxis { get; set; } = null!;

    /// <summary>
    /// Trục hướng đỉnh ốp (chuỗi hợp lệ: '+X', '-X', '+Y', '-Y', '+Z', '-Z').
    /// </summary>
    /// <example>+Y</example>
    [Required(ErrorMessage = "Trục hướng đỉnh không được để trống.")]
    [RegularExpression(BackSheetMath.AxisPattern, ErrorMessage = "Trục phải có định dạng [+|-][X|Y|Z].")]
    public string UpAxis { get; set; } = null!;

    /// <summary>
    /// Chiều cao thực tế của ốp lưng (tính bằng milimet, từ 20 đến 400 mm).
    /// </summary>
    /// <example>146.6</example>
    [Range(20, 400, ErrorMessage = "Chiều cao ốp phải nằm trong khoảng 20 - 400 mm.")]
    public decimal HeightMm { get; set; }

    /// <summary>
    /// Chiều rộng vùng lưng ốp (tính bằng milimet, từ 10 đến 300 mm).
    /// </summary>
    /// <example>70.6</example>
    [Range(10, 300, ErrorMessage = "Chiều rộng lưng ốp phải nằm trong khoảng 10 - 300 mm.")]
    public decimal BackWidthMm { get; set; }

    /// <summary>
    /// Số cột ma trận lưới vùng dán (tương ứng độ phân giải 1mm/điểm ảnh).
    /// </summary>
    /// <example>71</example>
    [Range(1, 400, ErrorMessage = "Số cột mask từ 1 đến 400.")]
    public int MaskCols { get; set; }

    /// <summary>
    /// Số hàng ma trận lưới vùng dán (tương ứng độ phân giải 1mm/điểm ảnh).
    /// </summary>
    /// <example>147</example>
    [Range(1, 400, ErrorMessage = "Số hàng mask từ 1 đến 400.")]
    public int MaskRows { get; set; }

    /// <summary>
    /// Chuỗi Base64 của ảnh bitmap mặt nạ nhị phân thể hiện vùng cho phép dán sticker (tránh camera, phím bấm, cảm biến).
    /// </summary>
    /// <example>iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=</example>
    [Required(ErrorMessage = "Mặt nạ vùng dán không được để trống.")]
    public string MaskBase64 { get; set; } = null!;

    /// <summary>
    /// Độ sâu thụt vào (mm) của điện thoại so với mặt ngoài ốp (mặc định 1.5mm).
    /// </summary>
    /// <example>1.5</example>
    public decimal PhoneInsetMm { get; set; } = 1.5m;

    /// <summary>
    /// Tỷ lệ co dãn 3D áp dụng cho điện thoại sau khi khớp chiều cao (mặc định 0.97).
    /// </summary>
    /// <example>0.97</example>
    public decimal PhoneScale { get; set; } = 0.97m;
}

/// <summary>
/// Yêu cầu tạo mới hoặc cập nhật sản phẩm sticker bán lẻ tại kiosk.
/// </summary>
public class StickerProductRequest
{
    /// <summary>
    /// Tên mẫu sticker hiển thị cho khách hàng chọn.
    /// </summary>
    /// <example>Sticker Mèo Thần Tài May Mắn</example>
    [Required(ErrorMessage = "Tên sticker không được để trống.")]
    [MaxLength(150, ErrorMessage = "Tên sticker tối đa 150 ký tự.")]
    public string StickerName { get; set; } = null!;

    /// <summary>
    /// Đường dẫn tĩnh của ảnh sticker trong thư mục /uploads/sticker/ (.png hoặc .webp trong suốt).
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    [Required(ErrorMessage = "Đường dẫn ảnh sticker không được để trống.")]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Chiều rộng in thực tế của sticker (milimet, từ 5 đến 200 mm).
    /// </summary>
    /// <example>45.0</example>
    [Range(5, 200, ErrorMessage = "Chiều rộng sticker phải từ 5 đến 200 mm.")]
    public decimal WidthMm { get; set; }

    /// <summary>
    /// Chiều cao in thực tế của sticker (milimet, từ 5 đến 200 mm).
    /// </summary>
    /// <example>45.0</example>
    [Range(5, 200, ErrorMessage = "Chiều cao sticker phải từ 5 đến 200 mm.")]
    public decimal HeightMm { get; set; }

    /// <summary>
    /// Đơn giá in/bán sticker (VNĐ).
    /// </summary>
    /// <example>25000</example>
    [Range(0, 99999999, ErrorMessage = "Giá bán không hợp lệ.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Trạng thái đang kinh doanh (true) hoặc đã ngừng bán (false).
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Yêu cầu cập nhật trạng thái đơn hàng.
/// </summary>
public class OrderStatusRequest
{
    /// <summary>
    /// Trạng thái đơn hàng mới. Giá trị hợp lệ: 'Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled'.
    /// </summary>
    /// <example>Processing</example>
    [Required(ErrorMessage = "Trạng thái không được để trống.")]
    [MaxLength(30)]
    public string Status { get; set; } = null!;
}
