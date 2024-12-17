using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class HoaDonNhap
    {
        public Guid Id { get; set; }
        public string NhaCungCap { get; set; } = null!;
        public DateTime NgayNhap { get; set; }
        public string SoHoaDon { get; set; } = null!;
        public string Serial { get; set; } = null!;
        public decimal ThanhTien { get; set; }
        public Guid KhoHangId { get; set; }

        // public Guid? NguoiNhapId { get; set; }
        // public virtual Account NguoiNhap { get; set; } = null!;
        public virtual KhoHang KhoHang { get; set; } = null!;
    }
}
