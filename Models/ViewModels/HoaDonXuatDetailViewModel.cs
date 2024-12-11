namespace SuppliesManagement.Models.ViewModels
{
    public class HoaDonXuatDetailViewModel
    {
        public Guid ID { get; set; }
        public string LyDoNhan { get; set; }
        public DateTime NgayNhan { get; set; }
        public string NguoiNhan { get; set; }
        public decimal ThanhTien { get; set; }
        public string KhoHang { get; set; }
        public List<HangHoaViewModel> HangHoas { get; set; }
    }
}
