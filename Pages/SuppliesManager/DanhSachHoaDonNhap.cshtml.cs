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

    var query = _dbContext.NhapKhos
        .Include(n => n.HoaDonNhap)
            .ThenInclude(n => n.KhoHang)
        .Include(n => n.HangHoaHoaDon)
        .AsQueryable();

    if (startDate.HasValue)
    {
        query = query.Where(h => h.HoaDonNhap.NgayNhap >= startDate.Value);
    }
    if (endDate.HasValue)
    {
        query = query.Where(h => h.HoaDonNhap.NgayNhap <= endDate.Value);
    }
    if (!string.IsNullOrEmpty(hoadon))
    {
        query = query.Where(h => h.HoaDonNhap.NhaCungCap.Contains(hoadon) || h.HoaDonNhap.SoHoaDon.Contains(hoadon));
    }

    // Tính tổng số hóa đơn và tổng số trang
    int totalItems = query
        .Select(n => n.HoaDonNhap.Id)
        .Distinct()
        .Count();
    TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
    CurrentPage = pageNumber;

    // Lấy danh sách hóa đơn cho trang hiện tại
    HoaDonNhaps = query
        .GroupBy(n => n.HoaDonNhap.Id)
        .Select(group => new HoaDonNhapViewModel
        {
            ID = group.Key,
            NhaCungCap = group.First().HoaDonNhap.NhaCungCap,
            NgayNhap = group.First().HoaDonNhap.NgayNhap,
            SoHoaDon = group.First().HoaDonNhap.SoHoaDon,
            ThanhTien = group.First().HoaDonNhap.ThanhTien,
            Serial = group.First().HoaDonNhap.Serial,
            KhoNhap = group.First().HoaDonNhap.KhoHang.Ten,
            SoLuongMatHang = group
            .Select(n => n.HangHoaHoaDon.Id) // Lấy các Id mặt hàng
            .Distinct() // Loại bỏ trùng lặp
            .Count()  // Tính tổng số lượng hàng hóa
        })
        .OrderByDescending(h => h.NgayNhap)
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .ToList();

    return Page();
}

    }
}
