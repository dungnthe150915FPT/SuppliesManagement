﻿namespace SuppliesManagement.Models.ViewModels
{
    public class HangHoaViewModel
    {
        public Guid Id { get; set; }
        public string TenHangHoa { get; set; }
        public int SoLuong { get; set; }
        public int DonViTinhId { get; set; }
        public int NhomHangId { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public decimal DonGiaSauThue { get; set; }
        public decimal TongGiaTruocThue { get; set; }
        public decimal TongGiaSauThue { get; set; }
        public string TenKhoHang { get; set; }
        public string NhomHangName { get; set; }
        public NhomHang NhomHang { get; set; }
        public KhoHang KhoHang { get; set; }
        public int VAT { get; set; }
        public byte[]? Image1 { get; set; }
        public byte[]? Image2 { get; set; }
        public byte[]? Image3 { get; set; }
    }
}
