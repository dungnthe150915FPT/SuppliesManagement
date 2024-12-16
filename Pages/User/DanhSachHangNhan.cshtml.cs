using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.User
{
    public class DanhSachHangNhanModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public DanhSachHangNhanModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<XuatKho> XuatKhos { get; set; }
        public List<NhomHang> NhomHangs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        public IActionResult OnGet(
            string hanghoa,
            string sortOrder,
            int pageNumber = 1,
            int? year = null
        )
        {
            // Kiểm tra session để đảm bảo user đã đăng nhập
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }

            // Kiểm tra user có phải RoleId = 3 (Người nhận hàng)
            var user = _dbContext.Accounts.Include(u => u.Role).FirstOrDefault(a => a.Id == userId);
            if (user == null || user.RoleId != 3)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            // Lấy danh sách xuất kho liên quan đến người nhận
            var query = _dbContext.XuatKhos
                .Include(x => x.HoaDonXuat)
                .ThenInclude(h => h.NguoiNhan)
                .Include(x => x.HangHoaHoaDon)
                .ThenInclude(h => h.DonViTinh)
                .Where(x => x.HoaDonXuat.NguoiNhanId == userId)
                .AsQueryable();

            // Lọc theo tên hàng hóa
            if (!string.IsNullOrEmpty(hanghoa))
            {
                query = query.Where(
                    x => EF.Functions.Like(x.HangHoaHoaDon.TenHangHoa, $"%{hanghoa}%")
                );
            }

            // Lọc theo năm
            if (year.HasValue)
            {
                query = query.Where(x => x.HoaDonXuat.NgayNhan.Year == year.Value);
            }

            // Sắp xếp
            query = sortOrder switch
            {
                "SoLuongAsc" => query.OrderBy(x => x.HangHoaHoaDon.SoLuong),
                "SoLuongDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.SoLuong),
                "DonGiaAsc" => query.OrderBy(x => x.HangHoaHoaDon.DonGiaTruocThue),
                "DonGiaDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.DonGiaTruocThue),
                "ThanhTienAsc" => query.OrderBy(x => x.HangHoaHoaDon.TongGiaTruocThue),
                "ThanhTienDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.TongGiaTruocThue),
                _ => query.OrderBy(x => x.HoaDonXuat.NgayNhan) // Mặc định sắp xếp theo ngày nhận
            };

            // Phân trang
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(query.Count() / (double)PageSize);
            XuatKhos = query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }
    }
}
