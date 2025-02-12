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

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<HoaDonNhapViewModel> HoaDonNhaps { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        public IActionResult OnGet(int pageNumber = 1)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            // Đảm bảo SortOrder luôn có giá trị mặc định
            SortOrder = string.IsNullOrEmpty(SortOrder) ? "NgayNhapDesc" : SortOrder;

            var query = _dbContext.NhapKhos
                .Include(n => n.HoaDonNhap)
                .ThenInclude(n => n.KhoHang)
                .Include(n => n.HangHoaHoaDon)
                .AsQueryable();

            // Lọc theo ngày nhập
            if (StartDate.HasValue)
                query = query.Where(h => h.HoaDonNhap.NgayNhap >= StartDate.Value);
            if (EndDate.HasValue)
                query = query.Where(h => h.HoaDonNhap.NgayNhap <= EndDate.Value);

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(
                    h =>
                        h.HoaDonNhap.NhaCungCap.Contains(SearchTerm)
                        || h.HoaDonNhap.SoHoaDon.Contains(SearchTerm)
                        || h.HoaDonNhap.Serial.Contains(SearchTerm)
                        || h.HoaDonNhap.ThanhTien.ToString().Contains(SearchTerm)
                        || h.HoaDonNhap.KhoHang.Ten.Contains(SearchTerm)
                        || h.HoaDonNhap.Id.ToString().Contains(SearchTerm)
                );
            }

            // Đếm tổng số hóa đơn
            int totalItems = query.Select(n => n.HoaDonNhap.Id).Distinct().Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            // Lấy danh sách hóa đơn
            var hoaDonList = query
                .GroupBy(n => n.HoaDonNhap.Id)
                .Select(
                    group =>
                        new HoaDonNhapViewModel
                        {
                            ID = group.Key,
                            NhaCungCap = group.First().HoaDonNhap.NhaCungCap,
                            NgayNhap = group.First().HoaDonNhap.NgayNhap,
                            SoHoaDon = group.First().HoaDonNhap.SoHoaDon,
                            ThanhTien = group.First().HoaDonNhap.ThanhTien,
                            Serial = group.First().HoaDonNhap.Serial,
                            KhoNhap = group.First().HoaDonNhap.KhoHang.Ten,
                            SoLuongMatHang = group
                                .Select(n => n.HangHoaHoaDon.Id)
                                .Distinct()
                                .Count()
                        }
                )
                .ToList(); // Chuyển thành List trước khi sắp xếp

            // Sắp xếp danh sách sau khi lấy dữ liệu
            hoaDonList = SortHoaDonList(hoaDonList, SortOrder);

            // Phân trang sau khi sắp xếp
            HoaDonNhaps = hoaDonList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }

        // Hàm sắp xếp danh sách hóa đơn theo SortOrder
        private List<HoaDonNhapViewModel> SortHoaDonList(
            List<HoaDonNhapViewModel> hoaDonList,
            string sortOrder
        )
        {
            return sortOrder switch
            {
                "NgayNhapAsc" => hoaDonList.OrderBy(h => h.NgayNhap).ToList(),
                "NgayNhapDesc" => hoaDonList.OrderByDescending(h => h.NgayNhap).ToList(),
                "TongTienAsc" => hoaDonList.OrderBy(h => h.ThanhTien).ToList(),
                "TongTienDesc" => hoaDonList.OrderByDescending(h => h.ThanhTien).ToList(),
                "SoLuongMatHangAsc" => hoaDonList.OrderBy(h => h.SoLuongMatHang).ToList(),
                "SoLuongMatHangDesc"
                    => hoaDonList.OrderByDescending(h => h.SoLuongMatHang).ToList(),
                _ => hoaDonList.OrderByDescending(h => h.NgayNhap).ToList(), // Mặc định sắp xếp theo NgayNhap giảm dần
            };
        }
    }
}
