namespace SuppliesManagement.Models
{
    public class HangHoa
    {
        public Guid Id { get; set; }
        public string TenHangHoa { get; set; }
        public int DonViTinhID { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public decimal DonGiaSauThue { get; set; }
        public decimal TongGiaTruocThue { get; set; }
        public int VAT { get; set; }
        public decimal TongGiaSauThue { get; set; }
        public int? SoLuongDaXuat { get; set; }
        public int? SoLuongConLai { get; set; }
        public int NhomHangID { get; set; }
        public int SoLuong { get; set; }
        public Guid KhoHangID { get; set; }
        public NhomHang NhomHang { get; set; }
        public KhoHang KhoHang { get; set; }
        public DonViTinh DonViTinh { get; set; }
    }
}
