using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class SuppliesDashboardModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public SuppliesDashboardModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int TotalHangHoas { get; set; }
        public Dictionary<string, int> HangHoaCounts { get; set; } = new();
        public Dictionary<string, double> HangHoaDistribution { get; set; } = new();

        public int TotalHoaDon { get; set; }
        public Dictionary<string, int> HoaDonCounts { get; set; } = new();
        public Dictionary<string, double> HoaDonDistribution { get; set; } = new();
        public string SoLuongDaXuatPercentage { get; set; }
        public string SoLuongConLaiPercentage { get; set; }

        public int TotalSoLuongDaXuat { get; set; }
        public int TotalSoLuongConLai { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            // Tổng số hàng hóa và phân phối
            var hangHoas = _dbContext.HangHoas.ToList();
            TotalHangHoas = hangHoas.Count;
            HangHoaCounts["Công cụ dụng cụ"] = hangHoas.Count(h => h.NhomHangId == 1);
            HangHoaCounts["Vật tư"] = hangHoas.Count(h => h.NhomHangId == 2);
            HangHoaCounts["Phụ tùng thay thế"] = hangHoas.Count(h => h.NhomHangId == 3);
            HangHoaCounts["Tài sản cố định"] = hangHoas.Count(h => h.NhomHangId == 4);

            HangHoaDistribution["Công cụ dụng cụ"] =
                HangHoaCounts["Công cụ dụng cụ"] * 100.0 / TotalHangHoas;
            HangHoaDistribution["Vật tư"] = HangHoaCounts["Vật tư"] * 100.0 / TotalHangHoas;
            HangHoaDistribution["Phụ tùng thay thế"] =
                HangHoaCounts["Phụ tùng thay thế"] * 100.0 / TotalHangHoas;
            HangHoaDistribution["Tài sản cố định"] =
                HangHoaCounts["Tài sản cố định"] * 100.0 / TotalHangHoas;

            // Tổng số hóa đơn nhập và xuất
            var hoaDonNhaps = _dbContext.HoaDonNhaps.ToList();
            var HoaDonXuats = _dbContext.HoaDonXuats.ToList();
            TotalHoaDon = hoaDonNhaps.Count + HoaDonXuats.Count;
            HoaDonCounts["Hóa đơn nhập"] = hoaDonNhaps.Count();
            HoaDonCounts["Hóa đơn xuất"] = HoaDonXuats.Count();
            HoaDonDistribution["Hóa đơn nhập"] = HoaDonCounts["Hóa đơn nhập"] * 100.0 / TotalHoaDon;
            HoaDonDistribution["Hóa đơn xuất"] = HoaDonCounts["Hóa đơn xuất"] * 100.0 / TotalHoaDon;

            // Tổng số lượng hàng hóa đã xuất và còn lại
            TotalSoLuongDaXuat = hangHoas.Sum(h => h.SoLuongDaXuat);
            TotalSoLuongConLai = hangHoas.Sum(h => h.SoLuongConLai);
            var totalSoLuong = TotalSoLuongDaXuat + TotalSoLuongConLai;
            SoLuongDaXuatPercentage = (TotalSoLuongDaXuat * 100.0 / totalSoLuong).ToString("F2");
            SoLuongConLaiPercentage = (TotalSoLuongConLai * 100.0 / totalSoLuong).ToString("F2");
            return Page();
        }
    }
}
