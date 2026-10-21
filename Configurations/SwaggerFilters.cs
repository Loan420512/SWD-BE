using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using StickerRobot.Utilities;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StickerRobot.Configurations;

/// <summary>
/// Thêm yêu cầu bảo mật JWT Bearer và các mã phản hồi 401/403 cho các endpoint có thuộc tính [Authorize].
/// Các endpoint công khai (không có [Authorize] hoặc có [AllowAnonymous]) sẽ không hiển thị biểu tượng ổ khóa.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var declaringType = context.MethodInfo.DeclaringType;
        var hasAuthorize = (declaringType != null && declaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any())
                           || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymous = (declaringType != null && declaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any())
                                || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [securityScheme] = Array.Empty<string>()
            });

            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Chưa xác thực hoặc JWT token không hợp lệ (Unauthorized)."
                });
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Không có quyền truy cập tài nguyên (Forbidden - yêu cầu quyền quản trị)."
                });
            }
        }
        else
        {
            // Đảm bảo endpoint ẩn danh không bị gán security
            operation.Security?.Clear();
        }
    }
}

/// <summary>
/// Sinh OperationId duy nhất, chuẩn hóa dạng camelCase theo mẫu Swagger Petstore cho từng endpoint.
/// </summary>
public class OperationIdNamingFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            // Nếu có gắn thuộc tính EndpointNameAttribute thì ưu tiên dùng
            var endpointNameAttr = descriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().FirstOrDefault();
            if (endpointNameAttr != null && !string.IsNullOrWhiteSpace(endpointNameAttr.EndpointName))
            {
                operation.OperationId = endpointNameAttr.EndpointName;
                return;
            }

            var controller = descriptor.ControllerName;
            var action = descriptor.ActionName;

            operation.OperationId = GenerateOperationId(controller, action, descriptor.AttributeRouteInfo?.Template);
        }
    }

    private static string GenerateOperationId(string controller, string action, string? route)
    {
        // Xử lý các action đặc biệt
        if (controller.Equals("Auth", StringComparison.OrdinalIgnoreCase) && action.Equals("Login", StringComparison.OrdinalIgnoreCase))
            return "login";

        if (controller.Equals("Catalog", StringComparison.OrdinalIgnoreCase) && action.Equals("Get", StringComparison.OrdinalIgnoreCase))
            return "getCatalog";

        if (controller.Equals("Uploads", StringComparison.OrdinalIgnoreCase) && action.Equals("Upload", StringComparison.OrdinalIgnoreCase))
            return "uploadFile";

        if (controller.Equals("Orders", StringComparison.OrdinalIgnoreCase) && action.Equals("GetByCode", StringComparison.OrdinalIgnoreCase))
            return "getOrderByCode";

        if (action.Equals("SetStatus", StringComparison.OrdinalIgnoreCase))
            return "setOrderStatus";

        if (action.Equals("SetFit", StringComparison.OrdinalIgnoreCase))
        {
            return controller.Equals("PhoneCases", StringComparison.OrdinalIgnoreCase)
                ? "setPhoneCaseFit"
                : "setPhoneModelFit";
        }

        // Đơn số hóa tên entity từ tên Controller số nhiều
        var entitySingular = ToSingular(controller);

        return action switch
        {
            "GetAll" => $"getAll{controller}",
            "GetById" => $"get{entitySingular}ById",
            "Create" => $"create{entitySingular}",
            "Update" => $"update{entitySingular}",
            "Delete" => $"delete{entitySingular}",
            _ => $"{char.ToLowerInvariant(action[0])}{action[1..]}{controller}"
        };
    }

    private static string ToSingular(string plural)
    {
        if (plural.EndsWith("cases", StringComparison.OrdinalIgnoreCase))
            return plural[..^1]; // PhoneCases -> PhoneCase
        if (plural.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
            return plural[..^3] + "y";
        if (plural.EndsWith("ses", StringComparison.OrdinalIgnoreCase))
            return plural[..^2];
        if (plural.EndsWith("s", StringComparison.OrdinalIgnoreCase) && !plural.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            return plural[..^1];
        return plural;
    }
}

/// <summary>
/// Định nghĩa danh sách các Tags theo tài nguyên kèm mô tả chi tiết, sắp xếp trực quan như Swagger Petstore.
/// </summary>
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Auth",
                Description = "Xác thực tài khoản quản trị viên và cấp phát JWT Bearer token."
            },
            new OpenApiTag
            {
                Name = "Catalog",
                Description = "Truy xuất danh mục tổng hợp dành cho Kiosk (mô hình điện thoại, ốp lưng, fit 3D và sticker)."
            },
            new OpenApiTag
            {
                Name = "PhoneModels",
                Description = "Quản lý các dòng máy điện thoại và thông số mô hình 3D (GLB)."
            },
            new OpenApiTag
            {
                Name = "PhoneCases",
                Description = "Quản lý danh mục ốp lưng, kích thước thực tế và ma trận vùng dán fit 3D."
            },
            new OpenApiTag
            {
                Name = "StickerProducts",
                Description = "Quản lý thư viện sticker thương mại in sẵn để bán tại kiosk."
            },
            new OpenApiTag
            {
                Name = "Orders",
                Description = "Tiếp nhận đơn hàng từ kiosk, tra cứu công khai và cập nhật tiến trình in dán của robot."
            },
            new OpenApiTag
            {
                Name = "Customers",
                Description = "Quản lý thông tin và lịch sử khách hàng tại kiosk."
            },
            new OpenApiTag
            {
                Name = "Payments",
                Description = "Quản lý và ghi nhận các giao dịch thanh toán đơn hàng (tiền mặt, VNPay, Momo, ZaloPay)."
            },
            new OpenApiTag
            {
                Name = "Uploads",
                Description = "Tải lên tệp mô hình 3D (GLB) và hình ảnh sticker (PNG, WebP)."
            },
            new OpenApiTag
            {
                Name = "DesignTemplates",
                Description = "Quản lý các mẫu thiết kế phối sẵn sticker và ốp lưng mẫu cho khách hàng lựa chọn."
            },
            new OpenApiTag
            {
                Name = "StickerItems",
                Description = "Quản lý chi tiết vị trí tọa độ, kích thước và góc xoay của sticker trên từng mẫu thiết kế."
            },
            new OpenApiTag
            {
                Name = "IncidentReports",
                Description = "Ghi nhận và theo dõi xử lý sự cố kỹ thuật của kiosk robot."
            },
            new OpenApiTag
            {
                Name = "CashFlowAudits",
                Description = "Đối soát và kiểm toán dòng tiền mặt tại kiosk so với hệ thống thanh toán."
            },
            new OpenApiTag
            {
                Name = "RefundTransactions",
                Description = "Quản lý giao dịch và lịch sử hoàn tiền cho khách hàng."
            },
            new OpenApiTag
            {
                Name = "Staffs",
                Description = "Quản lý danh sách nhân viên kỹ thuật vận hành, thiết kế và hỗ trợ."
            },
            new OpenApiTag
            {
                Name = "Admins",
                Description = "Quản lý tài khoản quản trị hệ thống cấp cao (Manager, SuperAdmin)."
            }
        };
    }
}

/// <summary>
/// Chuẩn hóa schema ApiResponse: Phân biệt rõ schema phản hồi lỗi (ApiResponse of Object) với success = false
/// và schema phản hồi thành công (ApiResponse of Entity) với success = true.
/// </summary>
public class ApiResponseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            var dataType = context.Type.GetGenericArguments()[0];
            if (dataType == typeof(object))
            {
                // ApiResponse<object> dùng cho phản hồi lỗi
                if (schema.Properties.TryGetValue("success", out var successProp))
                {
                    successProp.Example = new OpenApiBoolean(false);
                }
                if (schema.Properties.TryGetValue("message", out var messageProp))
                {
                    messageProp.Example = new OpenApiString("Mô tả lỗi xử lý yêu cầu.");
                }
                if (schema.Properties.TryGetValue("data", out var dataProp))
                {
                    dataProp.Example = new OpenApiNull();
                }
            }
            else
            {
                // ApiResponse<T> dùng cho phản hồi thành công
                if (schema.Properties.TryGetValue("success", out var successProp))
                {
                    successProp.Example = new OpenApiBoolean(true);
                }
                if (schema.Properties.TryGetValue("message", out var messageProp))
                {
                    messageProp.Example = new OpenApiString("Thao tác thành công.");
                }
            }
        }
    }
}

/// <summary>
/// Gán ví dụ phản hồi (Example Value) thực tế cho từng mã HTTP response:
/// - 404: success = false, message mô tả không tìm thấy thực thể cụ thể, data = null.
/// - 400: success = false, message lỗi nghiệp vụ/validation, data = null, errors chi tiết.
/// - 401: success = false, message chưa xác thực, data = null.
/// - 403: success = false, message không có quyền truy cập, data = null.
/// - 409: success = false, message xung đột ràng buộc dữ liệu, data = null.
/// - 200 (Delete / Set): success = true, message hoàn tất, data = null.
/// </summary>
public class ResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return;

        var controller = descriptor.ControllerName;
        var action = descriptor.ActionName;
        var entityName = GetEntityDisplayName(controller);
        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ApiResponse<object>), context.SchemaRepository);

        foreach (var (statusCode, response) in operation.Responses)
        {
            if (!response.Content.ContainsKey("application/json"))
            {
                if (statusCode is "200" or "400" or "401" or "403" or "404" or "409" or "500")
                {
                    response.Content["application/json"] = new OpenApiMediaType
                    {
                        Schema = errorSchema
                    };
                }
                else
                {
                    continue;
                }
            }

            var mediaType = response.Content["application/json"];

            switch (statusCode)
            {
                case "200":
                    if (action.StartsWith("Delete", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaType.Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(true),
                            ["message"] = new OpenApiString($"Xóa {entityName} thành công."),
                            ["data"] = new OpenApiNull(),
                            ["errors"] = new OpenApiNull()
                        };
                    }
                    else if (action.Equals("SetStatus", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaType.Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(true),
                            ["message"] = new OpenApiString("Cập nhật trạng thái đơn hàng thành công."),
                            ["data"] = new OpenApiNull(),
                            ["errors"] = new OpenApiNull()
                        };
                    }
                    else if (action.Equals("SetFit", StringComparison.OrdinalIgnoreCase))
                    {
                        var target = controller.Equals("PhoneCases", StringComparison.OrdinalIgnoreCase)
                            ? "3D và vùng dán ốp lưng"
                            : "3D dòng điện thoại";
                        mediaType.Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(true),
                            ["message"] = new OpenApiString($"Lưu cấu hình {target} thành công."),
                            ["data"] = new OpenApiNull(),
                            ["errors"] = new OpenApiNull()
                        };
                    }
                    break;

                case "400":
                    if (controller.Equals("Uploads", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaType.Example = CreateErrorResponseExample("Tham số kind bắt buộc phải là 'phone', 'case', hoặc 'sticker'.");
                    }
                    else if (controller.Equals("Orders", StringComparison.OrdinalIgnoreCase) && action.Equals("SetStatus", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaType.Example = CreateErrorResponseExample("Trạng thái mới không hợp lệ hoặc bước chuyển vi phạm quy trình nghiệp vụ.");
                    }
                    else
                    {
                        var validationErrors = new OpenApiObject
                        {
                            ["fieldName"] = new OpenApiArray
                            {
                                new OpenApiString("Dữ liệu đầu vào không hợp lệ hoặc thiếu trường bắt buộc.")
                            }
                        };
                        mediaType.Example = CreateErrorResponseExample("Dữ liệu đầu vào không hợp lệ.", validationErrors);
                    }
                    break;

                case "401":
                    if (controller.Equals("Auth", StringComparison.OrdinalIgnoreCase) && action.Equals("Login", StringComparison.OrdinalIgnoreCase))
                    {
                        mediaType.Example = CreateErrorResponseExample("Sai tên đăng nhập hoặc mật khẩu.");
                    }
                    else
                    {
                        mediaType.Example = CreateErrorResponseExample("Chưa xác thực hoặc JWT token không hợp lệ (Unauthorized).");
                    }
                    break;

                case "403":
                    mediaType.Example = CreateErrorResponseExample("Không có quyền truy cập tài nguyên (yêu cầu quyền Manager hoặc SuperAdmin).");
                    break;

                case "404":
                    var notFoundMsg = (controller, action) switch
                    {
                        ("Orders", "GetByCode") => "Không tìm thấy đơn hàng với mã ORD-20261020-0001.",
                        ("Orders", "SetStatus") => "Không tìm thấy đơn hàng 1.",
                        _ => $"Không tìm thấy {entityName} với ID 1."
                    };
                    mediaType.Example = CreateErrorResponseExample(notFoundMsg);
                    break;

                case "409":
                    var conflictMsg = controller switch
                    {
                        "PhoneModels" => "Không thể xóa dòng điện thoại này vì đang có ốp lưng liên kết.",
                        "PhoneCases" => "Không thể xóa ốp lưng này vì đang có đơn hàng hoặc mẫu thiết kế liên kết.",
                        _ => "Không thể xóa bản ghi do đang có dữ liệu liên kết ràng buộc."
                    };
                    mediaType.Example = CreateErrorResponseExample(conflictMsg);
                    break;

                case "500":
                    var serverErrorDetail = new OpenApiObject
                    {
                        ["detail"] = new OpenApiString("An unhandled exception occurred while processing request.")
                    };
                    mediaType.Example = CreateErrorResponseExample("An error occurred while processing your request.", serverErrorDetail);
                    break;
            }
        }
    }

    private static OpenApiObject CreateErrorResponseExample(string message, IOpenApiAny? errors = null)
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString(message),
            ["data"] = new OpenApiNull(),
            ["errors"] = errors ?? new OpenApiNull()
        };
    }

    private static string GetEntityDisplayName(string controller) => controller switch
    {
        "PhoneModels" => "dòng điện thoại",
        "PhoneCases" => "ốp lưng",
        "Orders" => "đơn hàng",
        "Customers" => "khách hàng",
        "Payments" => "giao dịch thanh toán",
        "StickerProducts" => "sản phẩm sticker",
        "DesignTemplates" => "mẫu thiết kế",
        "StickerItems" => "sticker item",
        "IncidentReports" => "báo cáo sự cố",
        "CashFlowAudits" => "bản ghi kiểm toán",
        "RefundTransactions" => "giao dịch hoàn tiền",
        "Staffs" => "nhân viên",
        "Admins" => "tài khoản quản trị",
        _ => "tài nguyên"
    };
}
