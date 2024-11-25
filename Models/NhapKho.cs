namespace SuppliesManagement.Models
{
    public class NhapKho
    {
        public Guid ID { get; set; }
        public Guid HoaDonNhapID { get; set; }
        public Guid HangHoaID { get; set; }
        /*public ICollection<HangHoa> HangHoas { get; set; }*/
        public HoaDonNhap HoaDonNhap { get; set; }
/*        public HangHoaHoaDon HangHoaHoaDon { get; set; }*/
        public HangHoa HangHoa { get; set; }
    }
}
