using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StickerRobot.Configurations;
using StickerRobot.Data;
using StickerRobot.Services;
using StickerRobot.Services.Interfaces;
using StickerRobot.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<StickerRobotDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Configuration Options (IOptions<T>)
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<OrderSettings>(builder.Configuration.GetSection(OrderSettings.SectionName));

// CORS Configuration
var corsConfig = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (corsConfig?.AllowedOrigins != null && corsConfig.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsConfig.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Register Application Services (Dependency Injection)
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICashFlowAuditService, CashFlowAuditService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDesignTemplateService, DesignTemplateService>();
builder.Services.AddScoped<IIncidentReportService, IncidentReportService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPhoneCaseService, PhoneCaseService>();
builder.Services.AddScoped<IPhoneModelService, PhoneModelService>();
builder.Services.AddScoped<IRefundTransactionService, RefundTransactionService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IStickerItemService, StickerItemService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? builder.Configuration["Jwt:SecretKey"]
    ?? "StickerRobotSuperSecretKeyForJwtAuthentication2026!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "StickerRobot",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "StickerRobotUsers",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI & Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StickerRobot API",
        Version = "v1"
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập mã JWT token được cấp từ POST /api/Auth/login (hệ thống tự động thêm tiền tố 'Bearer ')."
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
    options.OperationFilter<OperationIdNamingFilter>();
    options.OperationFilter<ResponseExamplesOperationFilter>();
    options.SchemaFilter<ApiResponseSchemaFilter>();
    options.DocumentFilter<TagDescriptionsDocumentFilter>();
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StickerRobot API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(1);
});

app.UseHttpsRedirection();

var contentTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypes.Mappings[".glb"] = "model/gltf-binary";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });

app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
