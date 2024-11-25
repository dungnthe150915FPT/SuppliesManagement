namespace SuppliesManagement.Models
{
    public class HoaDonNhap
    {
        public Guid ID { get; set; }
        public string NhaCungCap { get; set; }
        public DateTime NgayNhap { get; set; }
        public string? SoHoaDon { get; set; }
        public decimal ThanhTien { get; set; }
        public string? Serial { get; set; }
        public Guid KhoHangID { get; set; }
        public ICollection<HangHoa> HangHoas { get; set; } = new List<HangHoa>();
        public ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; } = new List<HangHoaHoaDon>();
        public KhoHang KhoHang { get; set; }
    }
}
