namespace SuppliesManagement.Models
{
    public class XuatKho
    {
        public Guid ID { get; set; }
        public Guid HoaDonXuatID { get; set; }
        public Guid HangHoaHoaDonID { get; set; }
/*        public ICollection<HangHoa> HangHoas { get; set; }
        public HoaDonXuat HoaDonXuat { get; set; }*/
/*        public ICollection<HangHoaHoaDon> HangHoaHoaDons { get; set; }*/
        public HoaDonXuat HoaDonXuat { get; set; }
        public HangHoaHoaDon HangHoaHoaDon { get; set; }
        public HangHoa HangHoa { get; set; }
    }
}
