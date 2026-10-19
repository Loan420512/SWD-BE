using Microsoft.EntityFrameworkCore;
using StickerRobot.Models;

namespace StickerRobot.Data;

public partial class StickerRobotDbContext
{
    public virtual DbSet<StickerProduct> StickerProducts { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhoneModel>(entity =>
        {
            entity.Property(e => e.ModelUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("model_url");
            entity.Property(e => e.BackAxis).HasMaxLength(2).IsUnicode(false).IsFixedLength().HasColumnName("back_axis");
            entity.Property(e => e.UpAxis).HasMaxLength(2).IsUnicode(false).IsFixedLength().HasColumnName("up_axis");
        });

        modelBuilder.Entity<PhoneCase>(entity =>
        {
            entity.Property(e => e.ModelUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("model_url");
            entity.Property(e => e.BackAxis).HasMaxLength(2).IsUnicode(false).IsFixedLength().HasColumnName("back_axis");
            entity.Property(e => e.UpAxis).HasMaxLength(2).IsUnicode(false).IsFixedLength().HasColumnName("up_axis");
            entity.Property(e => e.HeightMm).HasColumnType("decimal(7, 2)").HasColumnName("height_mm");
            entity.Property(e => e.BackWidthMm).HasColumnType("decimal(7, 2)").HasColumnName("back_width_mm");
            entity.Property(e => e.MaskCols).HasColumnName("mask_cols");
            entity.Property(e => e.MaskRows).HasColumnName("mask_rows");
            entity.Property(e => e.BackMask).HasColumnName("back_mask");
            entity.Property(e => e.PhoneInsetMm).HasColumnType("decimal(5, 2)").HasColumnName("phone_inset_mm");
            entity.Property(e => e.PhoneScale).HasColumnType("decimal(5, 3)").HasColumnName("phone_scale");
        });

        modelBuilder.Entity<StickerProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("STICKER_PRODUCT");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StickerName).HasMaxLength(150).HasColumnName("sticker_name");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsUnicode(false).HasColumnName("image_url");
            entity.Property(e => e.WidthMm).HasColumnType("decimal(7, 2)").HasColumnName("width_mm");
            entity.Property(e => e.HeightMm).HasColumnType("decimal(7, 2)").HasColumnName("height_mm");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)").HasColumnName("price");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("created_at");
        });

        modelBuilder.Entity<StickerItem>(entity =>
        {
            entity.Property(e => e.StickerProductId).HasColumnName("sticker_product_id");
            entity.Property(e => e.RotationDeg).HasColumnName("rotation_deg");
            entity.HasOne(d => d.StickerProduct).WithMany(p => p.StickerItems)
                .HasForeignKey(d => d.StickerProductId)
                .HasConstraintName("FK_STICKER_PRODUCT");
        });
    }
}
