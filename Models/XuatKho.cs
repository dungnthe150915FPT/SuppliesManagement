using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class XuatKho
    {
        public Guid XuatKhoId { get; set; }
        public Guid HoaDonXuatId { get; set; }
        public Guid HangHoaHoaDonId { get; set; }
        // public Guid HangHoaId { get; set; }

        public virtual HangHoaHoaDon HangHoaHoaDon { get; set; } = null!;
        public virtual HoaDonXuat HoaDonXuat { get; set; } = null!;
        // public virtual HangHoa HangHoa { get; set; } = null!;
    }
}
