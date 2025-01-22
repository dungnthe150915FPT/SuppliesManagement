using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuppliesManagement.Models
{
    public partial class HangHoa
    {
        public Guid Id { get; set; }
        public string TenHangHoa { get; set; } = null!;
        public int DonViTinhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal DonGiaTruocThue { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal DonGiaSauThue { get; set; }
        public int Vat { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal TongGiaTruocThue { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal TongGiaSauThue { get; set; }
        public int SoLuongDaXuat { get; set; }
        public int SoLuongConLai { get; set; }
        public int SoLuong { get; set; }
        public Guid KhoHangId { get; set; }
        public int NhomHangId { get; set; }
        public DateTime NgayNhap { get; set; }
        public byte[]? Image1 { get; set; }
        public byte[]? Image2 { get; set; }
        public byte[]? Image3 { get; set; }
        public virtual DonViTinh DonViTinh { get; set; } = null!;
        public virtual KhoHang KhoHang { get; set; } = null!;
        public virtual NhomHang NhomHang { get; set; } = null!;
    }
}
