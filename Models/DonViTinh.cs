﻿namespace SuppliesManagement.Models
{
    public class DonViTinh
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<HangHoa> HangHoas { get; set; }
        public ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; }
    }
}
