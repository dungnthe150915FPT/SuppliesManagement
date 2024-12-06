using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;
namespace SuppliesManagement.Pages
{
    public class DanhSachHoaDonNhapModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public DanhSachHoaDonNhapModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<HoaDonNhapViewModel> HoaDonNhaps { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        public IActionResult OnGet(
            DateTime? startDate,
            string hoadon,
            DateTime? endDate,
            int pageNumber = 1
        )
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            var query = _dbContext.HoaDonNhaps.Include(n => n.KhoHang).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(hoadon))
            {
                query = query.Where(h => h.NhaCungCap.Contains(hoadon) || h.SoHoaDon.Contains(hoadon));
            }
            // Tính tổng số hóa đơn và tổng số trang
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;
            // Lấy hóa đơn cho trang hiện tại
            HoaDonNhaps = query
                .OrderByDescending(h => h.NgayNhap)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(
                    h =>
                        new HoaDonNhapViewModel
                        {
                            ID = h.Id,
                            NhaCungCap = h.NhaCungCap,
                            NgayNhap = h.NgayNhap,
                            SoHoaDon = h.SoHoaDon,
                            ThanhTien = h.ThanhTien,
                            Serial = h.Serial,
                            KhoNhap = h.KhoHang.Ten,
                        }
                )
                .ToList();
            return Page();
        }
    }
}
