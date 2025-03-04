using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class NhapKho
    {
        public Guid NhapKhoId { get; set; }
        public Guid HoaDonNhapId { get; set; }
        public Guid HangHoaHoaDonId { get; set; }
        public Guid HangHoaId { get; set; }
        public virtual HangHoaHoaDon HangHoaHoaDon { get; set; } = null!;
        public virtual HoaDonNhap HoaDonNhap { get; set; } = null!;
        public virtual HangHoa HangHoa { get; set; } = null!;
    }
}
