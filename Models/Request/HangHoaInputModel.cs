﻿namespace SuppliesManagement.Models.Request
{
    public class HangHoaInputModel
    {
        public string TenHangHoa { get; set; }
        public int NhomHangID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public int DonViTinhID { get; set; }
        public int VAT { get; set; }
    }
}
