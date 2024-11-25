namespace SuppliesManagement.Models
{
    public class KhoHang
    {
        public Guid ID { get; set; }
        public string TenKho { get; set; }
        public string DiaChi { get; set; }
        public ICollection<HangHoa> HangHoas { get; set; }
        public ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; }
    }
}
