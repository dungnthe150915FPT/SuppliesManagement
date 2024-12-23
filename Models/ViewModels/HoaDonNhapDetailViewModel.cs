﻿namespace SuppliesManagement.Models.ViewModels
{
    public class HoaDonNhapDetailViewModel
    {
        public Guid ID { get; set; }
        public string NhaCungCap { get; set; }
        public string KhoHang { get; set; }
        public DateTime NgayNhap { get; set; }
        public string SoHoaDon { get; set; }
        public decimal ThanhTien { get; set; }
        public string Serial { get; set; }
        public List<HangHoaViewModel> HangHoas { get; set; }
    }
}
