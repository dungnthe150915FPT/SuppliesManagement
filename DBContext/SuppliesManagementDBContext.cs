using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.DBContext
{
    public class SuppliesManagementDBContext : DbContext
    {
        public SuppliesManagementDBContext(DbContextOptions<SuppliesManagementDBContext> options) : base(options)
        { }
        public DbSet<HangHoa> HangHoas { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<HoaDonNhap> HoaDonNhaps { get; set; }
        public DbSet<HoaDonXuat> HoaDonXuats { get; set; }
        public DbSet<KhoHang> KhoHangs { get; set; }
        public DbSet<NhomHang> NhomHangs { get; set; }
        public DbSet<XuatKho> XuatKhos { get; set; }
        public DbSet<NhapKho> NhapKhos { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<DonViTinh> DonViTinhs { get; set; }
        public DbSet<HangHoaHoaDon> HangHoaHoaDons { get; set; }
    }
}
