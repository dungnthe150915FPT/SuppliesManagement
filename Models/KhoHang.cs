using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class KhoHang
    {
        public KhoHang()
        {
            HangHoas = new HashSet<HangHoa>();
            HoaDonNhaps = new HashSet<HoaDonNhap>();
        }

        public Guid Id { get; set; }
        public string Ten { get; set; } = null!;
        public string DiaChi { get; set; } = null!;

        public virtual ICollection<HangHoa> HangHoas { get; set; }
        public virtual ICollection<HoaDonNhap> HoaDonNhaps { get; set; }
    }
}
