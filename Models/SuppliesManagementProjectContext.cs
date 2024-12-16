using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SuppliesManagement.Models
{
    public partial class SuppliesManagementProjectContext : DbContext
    {
        public SuppliesManagementProjectContext() { }

        public SuppliesManagementProjectContext(
            DbContextOptions<SuppliesManagementProjectContext> options
        )
            : base(options) { }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<DonViTinh> DonViTinhs { get; set; } = null!;
        public virtual DbSet<HangHoa> HangHoas { get; set; } = null!;
        public virtual DbSet<HangHoaHoaDon> HangHoaHoaDons { get; set; } = null!;
        public virtual DbSet<HoaDonNhap> HoaDonNhaps { get; set; } = null!;
        public virtual DbSet<HoaDonXuat> HoaDonXuats { get; set; } = null!;
        public virtual DbSet<KhoHang> KhoHangs { get; set; } = null!;
        public virtual DbSet<NhapKho> NhapKhos { get; set; } = null!;
        public virtual DbSet<NhomHang> NhomHangs { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<XuatKho> XuatKhos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("SuppliesManagement");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");

                entity.Property(e => e.Password).IsUnicode(false);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.Username).IsUnicode(false);

                entity
                    .HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Accounts_Roles");
            });

            modelBuilder.Entity<DonViTinh>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");
            });

            modelBuilder.Entity<HangHoa>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DonGiaSauThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DonGiaTruocThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DonViTinhId).HasColumnName("DonViTinhID");

                entity.Property(e => e.KhoHangId).HasColumnName("KhoHangID");

                entity.Property(e => e.NhomHangId).HasColumnName("NhomHangID");

                entity.Property(e => e.TongGiaSauThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TongGiaTruocThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity
                    .HasOne(d => d.DonViTinh)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.DonViTinhId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HangHoas_DonViTinhs");

                entity
                    .HasOne(d => d.KhoHang)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.KhoHangId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HangHoas_KhoHangs");

                entity
                    .HasOne(d => d.NhomHang)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.NhomHangId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HangHoas_NhomHangs");
            });

            modelBuilder.Entity<HangHoaHoaDon>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DonGiaSauThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DonGiaTruocThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DonViTinhId).HasColumnName("DonViTinhID");

                entity.Property(e => e.KhoHangId).HasColumnName("KhoHangID");

                entity.Property(e => e.NhomHangId).HasColumnName("NhomHangID");

                entity.Property(e => e.TongGiaSauThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TongGiaTruocThue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Vat).HasColumnName("VAT");

                entity
                    .HasOne(d => d.DonViTinh)
                    .WithMany(p => p.HangHoaHoaDons)
                    .HasForeignKey(d => d.DonViTinhId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HangHoaHoaDons_DonViTinhs");

                entity
                    .HasOne(d => d.NhomHang)
                    .WithMany(p => p.HangHoaHoaDons)
                    .HasForeignKey(d => d.NhomHangId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HangHoaHoaDons_NhomHangs");
            });

            modelBuilder.Entity<HoaDonNhap>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");

                entity.Property(e => e.KhoHangId).HasColumnName("KhoHangID");

                entity.Property(e => e.NgayNhap).HasColumnType("datetime");

                entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 0)");

                entity
                    .HasOne(d => d.KhoHang)
                    .WithMany(p => p.HoaDonNhaps)
                    .HasForeignKey(d => d.KhoHangId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HoaDonNhaps_KhoHangs");
            });

            modelBuilder.Entity<HoaDonXuat>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");

                entity.Property(e => e.KhoHangId).HasColumnName("KhoHangID");

                entity.Property(e => e.NgayNhan).HasColumnType("datetime");

                entity.Property(e => e.NguoiNhanId).HasColumnName("NguoiNhanID");

                entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 0)");

                entity
                    .HasOne(d => d.NguoiNhan)
                    .WithMany(p => p.HoaDonXuats)
                    .HasForeignKey(d => d.NguoiNhanId)
                    .HasConstraintName("FK_HoaDonXuats_Accounts");
            });

            modelBuilder.Entity<KhoHang>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");
            });

            modelBuilder.Entity<NhapKho>(entity =>
            {
                /*entity.Property(e => e.Id).ValueGeneratedNever();*/
                entity.Property(e => e.NhapKhoId).ValueGeneratedNever().HasColumnName("NhapKhoID");

                entity.Property(e => e.HangHoaHoaDonId).HasColumnName("HangHoaHoaDonID");

                entity.Property(e => e.HoaDonNhapId).HasColumnName("HoaDonNhapID");

                entity
                    .HasOne(d => d.HangHoaHoaDon)
                    .WithMany()
                    .HasForeignKey(d => d.HangHoaHoaDonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NhapKhos_HangHoaHoaDons");

                entity
                    .HasOne(d => d.HoaDonNhap)
                    .WithMany()
                    .HasForeignKey(d => d.HoaDonNhapId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NhapKhos_HoaDonNhaps");
            });

            modelBuilder.Entity<NhomHang>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ID");
            });

            modelBuilder.Entity<XuatKho>(entity =>
            {
                /*entity.HasNoKey();*/
                entity.Property(e => e.XuatKhoId).ValueGeneratedNever().HasColumnName("XuatKhoID");

                entity.Property(e => e.HangHoaHoaDonId).HasColumnName("HangHoaHoaDonID");

                entity.Property(e => e.HoaDonXuatId).HasColumnName("HoaDonXuatID");

                entity
                    .HasOne(d => d.HangHoaHoaDon)
                    .WithMany()
                    .HasForeignKey(d => d.HangHoaHoaDonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_XuatKhos_HangHoaHoaDons");

                entity
                    .HasOne(d => d.HoaDonXuat)
                    .WithMany()
                    .HasForeignKey(d => d.HoaDonXuatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_XuatKhos_HoaDonXuats");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
