namespace SuppliesManagement.Models
{
    public class NhomHang
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<HangHoa> HangHoas { get; set; }
        public ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; }
    }
}
