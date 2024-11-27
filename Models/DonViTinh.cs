using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class DonViTinh
    {
        public DonViTinh()
        {
            HangHoaHoaDons = new HashSet<HangHoaHoaDon>();
            HangHoas = new HashSet<HangHoa>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; }
        public virtual ICollection<HangHoa> HangHoas { get; set; }
    }
}
