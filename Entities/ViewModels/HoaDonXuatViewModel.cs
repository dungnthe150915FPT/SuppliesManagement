using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;

namespace SuppliesManagement.Entities.ViewModels
{
    public class HoaDonXuatViewModel
    {
        public Guid Id { get; set; }
        public DateTime NgayNhan { get; set; }
        public string LyDoNhan { get; set; } = null!;
        public decimal ThanhTien { get; set; }
        public string KhoHang { get; set; }
        public string NguoiNhanUsername { get; set; }
        public virtual Account? NguoiNhan { get; set; }
        public List<HangHoaViewModel> HangHoas { get; set; }
    }
}
