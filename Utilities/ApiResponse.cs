namespace StickerRobot.Utilities;

/// <summary>
/// Định dạng phản hồi chuẩn cho toàn bộ API của hệ thống StickerRobot.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần dữ liệu kết quả.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Cho biết yêu cầu đã được xử lý thành công (true) hay gặp lỗi (false).
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Thông điệp phản hồi từ máy chủ mô tả kết quả xử lý.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu kết quả nghiệp vụ (chỉ có khi Success = true, khi lỗi sẽ là null).
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Chi tiết lỗi phát sinh hoặc danh sách lỗi validation (khi Success = false).
    /// </summary>
    public object? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
