namespace StickerRobot.DTO.DTOResponse;

/// <summary>
/// Dữ liệu phản hồi khi đăng nhập thành công.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Chuỗi mã thông báo JWT Bearer token để đính kèm vào Authorization header.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Tên đăng nhập của tài khoản quản trị.
    /// </summary>
    /// <example>manager</example>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Vai trò hệ thống của tài khoản ('Manager' hoặc 'SuperAdmin').
    /// </summary>
    /// <example>Manager</example>
    public string Role { get; set; } = null!;

    /// <summary>
    /// Thời điểm hết hạn của token theo chuẩn UTC.
    /// </summary>
    /// <example>2026-10-21T12:00:00Z</example>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Dữ liệu phản hồi sau khi tải lên tệp tin thành công.
/// </summary>
public class UploadResponse
{
    /// <summary>
    /// Đường dẫn URL tĩnh của tệp đã lưu trữ (dưới tiền tố /uploads/{kind}/).
    /// </summary>
    /// <example>/uploads/sticker/c90e210cb5874288b8352b2fbb15f5bc.png</example>
    public string Url { get; set; } = null!;
}

/// <summary>
/// Thông tin chi tiết của một sản phẩm sticker trong thư viện bán tại kiosk.
/// </summary>
public class StickerProductResponse
{
    /// <summary>
    /// Mã định danh duy nhất của sản phẩm sticker.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Tên mẫu sticker.
    /// </summary>
    /// <example>Sticker Mèo Thần Tài May Mắn</example>
    public string StickerName { get; set; } = null!;

    /// <summary>
    /// Đường dẫn hình ảnh hiển thị của sticker.
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// Chiều rộng thực tế (mm).
    /// </summary>
    /// <example>45.0</example>
    public decimal WidthMm { get; set; }

    /// <summary>
    /// Chiều cao thực tế (mm).
    /// </summary>
    /// <example>45.0</example>
    public decimal HeightMm { get; set; }

    /// <summary>
    /// Đơn giá bán (VNĐ).
    /// </summary>
    /// <example>25000</example>
    public decimal Price { get; set; }

    /// <summary>
    /// Trạng thái đang hoạt động kinh doanh hay đã ngừng bán.
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Thời điểm tạo sản phẩm.
    /// </summary>
    /// <example>2026-10-20T10:00:00Z</example>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Thông tin vị trí, hướng trục và đường dẫn 3D của mô hình.
/// </summary>
public class PoseResponse
{
    /// <summary>
    /// Đường dẫn tệp GLB 3D.
    /// </summary>
    /// <example>/uploads/phone/iphone-15-pro.glb</example>
    public string ModelUrl { get; set; } = null!;

    /// <summary>
    /// Hướng trục mặt lưng (+X, -X, +Y, -Y, +Z, -Z).
    /// </summary>
    /// <example>+Z</example>
    public string BackAxis { get; set; } = null!;

    /// <summary>
    /// Hướng trục đỉnh trên (+X, -X, +Y, -Y, +Z, -Z).
    /// </summary>
    /// <example>+Y</example>
    public string UpAxis { get; set; } = null!;
}

/// <summary>
/// Thông số khớp 3D và mặt nạ vùng dán của ốp lưng điện thoại.
/// </summary>
public class CaseFitResponse : PoseResponse
{
    /// <summary>
    /// Chiều cao thực tế của ốp (mm).
    /// </summary>
    /// <example>146.6</example>
    public decimal HeightMm { get; set; }

    /// <summary>
    /// Chiều rộng vùng lưng ốp (mm).
    /// </summary>
    /// <example>70.6</example>
    public decimal BackWidthMm { get; set; }

    /// <summary>
    /// Số cột ma trận lưới (độ phân giải 1mm/điểm ảnh).
    /// </summary>
    /// <example>71</example>
    public int MaskCols { get; set; }

    /// <summary>
    /// Số hàng ma trận lưới (độ phân giải 1mm/điểm ảnh).
    /// </summary>
    /// <example>147</example>
    public int MaskRows { get; set; }

    /// <summary>
    /// Chuỗi Base64 hình ảnh mặt nạ nhị phân vùng được phép dán sticker.
    /// </summary>
    /// <example>iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=</example>
    public string MaskBase64 { get; set; } = null!;

    /// <summary>
    /// Độ sâu thụt vào (mm) của điện thoại so với mặt ngoài ốp.
    /// </summary>
    /// <example>1.5</example>
    public decimal PhoneInsetMm { get; set; }

    /// <summary>
    /// Tỷ lệ co dãn 3D áp dụng cho điện thoại.
    /// </summary>
    /// <example>0.97</example>
    public decimal PhoneScale { get; set; }
}

/// <summary>
/// Thông tin ốp lưng kèm thông số fit 3D trong danh mục kiosk.
/// </summary>
public class CatalogCaseResponse
{
    /// <summary>
    /// Mã định danh duy nhất của ốp lưng.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Mã SKU sản phẩm của ốp lưng.
    /// </summary>
    /// <example>CASE-IP15P-CLR</example>
    public string? Sku { get; set; }

    /// <summary>
    /// Chất liệu của ốp lưng (ví dụ: Silicone dẻo, Nhựa cứng PC, Da nhân tạo).
    /// </summary>
    /// <example>Silicone trong suốt chống ố</example>
    public string? Material { get; set; }

    /// <summary>
    /// Màu sắc cơ bản của ốp lưng.
    /// </summary>
    /// <example>Trong suốt</example>
    public string? Color { get; set; }

    /// <summary>
    /// Đơn giá của ốp lưng (VNĐ).
    /// </summary>
    /// <example>80000</example>
    public decimal? Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho hiện có tại khay kiosk.
    /// </summary>
    /// <example>50</example>
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Thông số khớp 3D và mặt nạ vùng dán phục vụ hiển thị trên kiosk.
    /// </summary>
    public CaseFitResponse? Fit { get; set; }
}

/// <summary>
/// Dòng máy điện thoại kèm danh sách các ốp lưng tương thích trong danh mục kiosk.
/// </summary>
public class CatalogModelResponse
{
    /// <summary>
    /// Mã định danh duy nhất của dòng điện thoại.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Tên thương hiệu sản xuất (Apple, Samsung, Xiaomi,...).
    /// </summary>
    /// <example>Apple</example>
    public string? Brand { get; set; }

    /// <summary>
    /// Tên thương mại của dòng máy.
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    public string? ModelName { get; set; }

    /// <summary>
    /// Kích thước vật lý cơ bản (Dài x Rộng x Dày).
    /// </summary>
    /// <example>146.6 x 70.6 x 8.25 mm</example>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Thông số mô hình 3D của thân máy.
    /// </summary>
    public PoseResponse? Phone { get; set; }

    /// <summary>
    /// Danh sách các loại ốp lưng hỗ trợ cho dòng máy này.
    /// </summary>
    public List<CatalogCaseResponse> Cases { get; set; } = new();
}

/// <summary>
/// Toàn bộ dữ liệu danh mục đồng bộ cho ứng dụng kiosk khách hàng.
/// </summary>
public class CatalogResponse
{
    /// <summary>
    /// Danh sách các dòng máy và ốp lưng tương thích.
    /// </summary>
    public List<CatalogModelResponse> Models { get; set; } = new();

    /// <summary>
    /// Danh sách các mẫu sticker đang mở bán.
    /// </summary>
    public List<StickerProductResponse> Stickers { get; set; } = new();
}

/// <summary>
/// Tọa độ và thông số đặt của từng sticker trên ốp trong đơn hàng.
/// </summary>
public class OrderPlacementResponse
{
    /// <summary>
    /// Tên sticker.
    /// </summary>
    /// <example>Sticker Mèo May Mắn</example>
    public string? Name { get; set; }

    /// <summary>
    /// Đường dẫn ảnh sticker.
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Tọa độ X trên lưng ốp (pixel/mm).
    /// </summary>
    /// <example>35</example>
    public int? PositionX { get; set; }

    /// <summary>
    /// Tọa độ Y trên lưng ốp (pixel/mm).
    /// </summary>
    /// <example>70</example>
    public int? PositionY { get; set; }

    /// <summary>
    /// Góc xoay sticker tính theo độ (0 - 360).
    /// </summary>
    /// <example>45</example>
    public int RotationDeg { get; set; }
}

/// <summary>
/// Thông tin tra cứu đơn hàng công khai dành cho khách hàng kiểm tra tiến trình dán.
/// </summary>
public class PublicOrderResponse
{
    /// <summary>
    /// ID đơn hàng trong cơ sở dữ liệu.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Mã tra cứu đơn hàng công khai.
    /// </summary>
    /// <example>ORD-20261020-0001</example>
    public string OrderCode { get; set; } = null!;

    /// <summary>
    /// Trạng thái đơn hàng ('Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled').
    /// </summary>
    /// <example>Processing</example>
    public string? Status { get; set; }

    /// <summary>
    /// Tổng tiền đơn hàng (VNĐ).
    /// </summary>
    /// <example>105000</example>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Nhãn hiển thị dòng máy đã chọn.
    /// </summary>
    /// <example>Apple iPhone 15 Pro</example>
    public string? ModelLabel { get; set; }

    /// <summary>
    /// Nhãn hiển thị ốp lưng đã chọn.
    /// </summary>
    /// <example>Silicone trong suốt chống ố</example>
    public string? CaseLabel { get; set; }

    /// <summary>
    /// Mã SKU của ốp lưng.
    /// </summary>
    /// <example>CASE-IP15P-CLR</example>
    public string? CaseSku { get; set; }

    /// <summary>
    /// Tên sticker chính (nếu có).
    /// </summary>
    /// <example>Sticker Mèo May Mắn</example>
    public string? StickerName { get; set; }

    /// <summary>
    /// URL ảnh sticker chính (nếu có).
    /// </summary>
    /// <example>/uploads/sticker/seed-sun.png</example>
    public string? StickerImageUrl { get; set; }

    /// <summary>
    /// Tọa độ X của sticker chính.
    /// </summary>
    /// <example>35</example>
    public int? PositionX { get; set; }

    /// <summary>
    /// Tọa độ Y của sticker chính.
    /// </summary>
    /// <example>70</example>
    public int? PositionY { get; set; }

    /// <summary>
    /// Góc xoay của sticker chính (độ).
    /// </summary>
    /// <example>0</example>
    public int RotationDeg { get; set; }

    /// <summary>
    /// Danh sách chi tiết tất cả sticker và vị trí dán của robot.
    /// </summary>
    public List<OrderPlacementResponse> Placements { get; set; } = new();

    /// <summary>
    /// Phương thức thanh toán ('Cash', 'VNPay', 'Momo', 'ZaloPay').
    /// </summary>
    /// <example>VNPay</example>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo đơn hàng.
    /// </summary>
    /// <example>2026-10-20T14:30:00Z</example>
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Dữ liệu quản trị chi tiết cho từng dòng đơn hàng trong danh sách của Admin.
/// </summary>
public class AdminOrderResponse : PublicOrderResponse
{
    /// <summary>
    /// Họ tên khách hàng đặt đơn.
    /// </summary>
    /// <example>Nguyễn Văn A</example>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Số điện thoại khách hàng.
    /// </summary>
    /// <example>0912345678</example>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Trạng thái thanh toán ('Success', 'Pending', 'Paid', 'Failed', 'Refunded').
    /// </summary>
    /// <example>Success</example>
    public string? PaymentStatus { get; set; }
}
