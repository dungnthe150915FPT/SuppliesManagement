using SuppliesManagement.Models.Request;

namespace SuppliesManagement.Entities.ViewModels
{
    public class NhapMuaHangHoaModel
    {
        public string NhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public List<HangHoaInputModel> HangHoaModels { get; set; } = new List<HangHoaInputModel>();

    }
}
