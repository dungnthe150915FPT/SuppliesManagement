namespace SuppliesManagement.Models.ViewModels
{
    public class HangHoaViewModel
    {
        public string TenHangHoa { get; set; }
        public int SoLuong { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public decimal DonGiaSauThue { get; set; }
        public decimal TongGiaTruocThue { get; set; }
        public decimal TongGiaSauThue { get; set; }
        public string TenKhoHang { get; set; }
        public KhoHang KhoHang { get; set; }
    }
}
