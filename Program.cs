using Microsoft.EntityFrameworkCore;
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
        Title = "StickerRobot Web API",
        Version = "v1",
        Description = "ASP.NET Core 8.0 Web API for StickerRobot System management (Customers, Orders, Cases, Templates, Audits, Staffs, Admins)."
    });
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StickerRobot Web API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("DefaultCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
