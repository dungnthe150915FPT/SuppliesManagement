﻿namespace SuppliesManagement.Models
{
    public class HoaDonXuat
    {
        public Guid ID { get; set; }
        public string NguoiNhan { get; set; }
        public DateTime NgayNhan { get; set; }
        public string LyDoXuat { get; set; }
        public decimal ThanhTien { get; set; }
        public Guid KhoHangID { get; set; }
        public KhoHang KhoHang { get; set; }
    }
}
