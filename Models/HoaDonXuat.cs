using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class HoaDonXuat
    {
        public Guid Id { get; set; }
        public DateTime NgayNhan { get; set; }
        public string LyDoNhan { get; set; } = null!;
        public decimal ThanhTien { get; set; }
        public Guid KhoHangId { get; set; }
        public Guid? NguoiNhanId { get; set; }
        public Account? NguoiNhan { get; set; }
        public KhoHang KhoHang { get; set; } = null!;
    }
}
