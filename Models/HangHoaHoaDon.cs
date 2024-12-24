using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class HangHoaHoaDon
    {
        public Guid Id { get; set; }
        public string TenHangHoa { get; set; } = null!;
        public int DonViTinhId { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public decimal DonGiaSauThue { get; set; }
        public int Vat { get; set; }
        public decimal TongGiaTruocThue { get; set; }
        public decimal TongGiaSauThue { get; set; }
        public int SoLuong { get; set; }
        public Guid KhoHangId { get; set; }
        public int NhomHangId { get; set; }

        public byte[]? Image1 { get; set; }
        public byte[]? Image2 { get; set; }
        public byte[]? Image3 { get; set; }
        public virtual DonViTinh DonViTinh { get; set; } = null!;
        public virtual NhomHang NhomHang { get; set; } = null!;
        public virtual KhoHang KhoHang { get; set; } = null!;
    }
}
