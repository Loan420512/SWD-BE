using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StickerRobot.Models;

namespace StickerRobot.Data;

public partial class StickerRobotDbContext : DbContext
{
    public StickerRobotDbContext(DbContextOptions<StickerRobotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<CashFlowAudit> CashFlowAudits { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DesignTemplate> DesignTemplates { get; set; }

    public virtual DbSet<IncidentReport> IncidentReports { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PhoneCase> PhoneCases { get; set; }

    public virtual DbSet<PhoneModel> PhoneModels { get; set; }

    public virtual DbSet<RefundTransaction> RefundTransactions { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StickerItem> StickerItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ADMIN__3213E83F80399D7D");

            entity.ToTable("ADMIN");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<CashFlowAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CASH_FLO__3213E83F7A8CBB26");

            entity.ToTable("CASH_FLOW_AUDIT");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.AuditDate).HasColumnName("audit_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalRefund)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_refund");
            entity.Property(e => e.TotalRevenue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_revenue");

            entity.HasOne(d => d.Admin).WithMany(p => p.CashFlowAudits)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_AUDIT_ADMIN");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CUSTOMER__3213E83FDD62113A");

            entity.ToTable("CUSTOMER");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<DesignTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DESIGN_T__3213E83FF21D24B0");

            entity.ToTable("DESIGN_TEMPLATE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.TemplateName)
                .HasMaxLength(150)
                .HasColumnName("template_name");

            entity.HasOne(d => d.Case).WithMany(p => p.DesignTemplates)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_TEMPLATE_CASE");

            entity.HasOne(d => d.Staff).WithMany(p => p.DesignTemplates)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_TEMPLATE_STAFF");
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__INCIDENT__3213E83F82E28D15");

            entity.ToTable("INCIDENT_REPORT");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IncidentType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("incident_type");
            entity.Property(e => e.ReportedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("reported_at");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Open")
                .HasColumnName("status");

            entity.HasOne(d => d.Staff).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_INCIDENT_STAFF");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ORDER__3213E83F999D3C97");

            entity.ToTable("ORDER");

            entity.HasIndex(e => e.OrderCode, "UQ__ORDER__99D12D3FB8C06EC9").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DesignTemplateId).HasColumnName("design_template_id");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("order_code");
            entity.Property(e => e.PhoneCaseId).HasColumnName("phone_case_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_ORDER_CUSTOMER");

            entity.HasOne(d => d.DesignTemplate).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DesignTemplateId)
                .HasConstraintName("FK_ORDER_TEMPLATE");

            entity.HasOne(d => d.PhoneCase).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PhoneCaseId)
                .HasConstraintName("FK_ORDER_CASE");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PAYMENT__3213E83FAB24541F");

            entity.ToTable("PAYMENT");

            entity.HasIndex(e => e.OrderId, "UQ__PAYMENT__46596228A0B5CE72").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("payment_status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Audit).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AuditId)
                .HasConstraintName("FK_PAYMENT_AUDIT");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PAYMENT_ORDER");
        });

        modelBuilder.Entity<PhoneCase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PHONE_CA__3213E83FD36EF80F");

            entity.ToTable("PHONE_CASE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("material");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("sku");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");

            entity.HasOne(d => d.Model).WithMany(p => p.PhoneCases)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK_CASE_MODEL");
        });

        modelBuilder.Entity<PhoneModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PHONE_MO__3213E83F0B668C42");

            entity.ToTable("PHONE_MODEL");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dimensions");
            entity.Property(e => e.ModelName)
                .HasMaxLength(100)
                .HasColumnName("model_name");
        });

        modelBuilder.Entity<RefundTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REFUND_T__3213E83FB36CA5E4");

            entity.ToTable("REFUND_TRANSACTION");

            entity.HasIndex(e => e.PaymentId, "UQ__REFUND_T__ED1FC9EB5C27B1E4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("refund_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Admin).WithMany(p => p.RefundTransactions)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_REFUND_ADMIN");

            entity.HasOne(d => d.Audit).WithMany(p => p.RefundTransactions)
                .HasForeignKey(d => d.AuditId)
                .HasConstraintName("FK_REFUND_AUDIT");

            entity.HasOne(d => d.Payment).WithOne(p => p.RefundTransaction)
                .HasForeignKey<RefundTransaction>(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_REFUND_PAYMENT");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__STAFF__3213E83FAAB1D57E");

            entity.ToTable("STAFF");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("employee_code");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Position)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("position");

            entity.HasOne(d => d.Admin).WithMany(p => p.Staff)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_STAFF_ADMIN");
        });

        modelBuilder.Entity<StickerItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__STICKER___3213E83FF9BDAF41");

            entity.ToTable("STICKER_ITEM");

            entity.HasIndex(e => e.TemplateId, "IX_STICKER_ITEM_TEMPLATE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PositionX).HasColumnName("position_x");
            entity.Property(e => e.PositionY).HasColumnName("position_y");
            entity.Property(e => e.Scale).HasColumnName("scale");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.StickerName)
                .HasMaxLength(150)
                .HasColumnName("sticker_name");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.StickerItems)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_STICKER_STAFF");

            entity.HasOne(d => d.Template).WithMany(p => p.StickerItems)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STICKER_TEMPLATE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
