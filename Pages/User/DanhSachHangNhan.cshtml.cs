using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }

            var user = await _dbContext.Accounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(a => a.Id == userId);
            if (user == null || user.RoleId != 3)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            var query = _dbContext.XuatKhos
                .Include(x => x.HoaDonXuat)
                .ThenInclude(h => h.NguoiNhan)
                .Include(x => x.HangHoaHoaDon)
                .ThenInclude(h => h.DonViTinh)
                .Where(x => x.HoaDonXuat.NguoiNhanId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(
                    x => EF.Functions.Like(x.HangHoaHoaDon.TenHangHoa, $"%{SearchTerm}%")
                );
            }

            if (StartDate.HasValue)
            {
                query = query.Where(x => x.HoaDonXuat.NgayNhan >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(x => x.HoaDonXuat.NgayNhan <= EndDate.Value);
            }

            query = SortOrder switch
            {
                "SoLuongAsc" => query.OrderBy(x => x.HangHoaHoaDon.SoLuong),
                "SoLuongDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.SoLuong),
                "DonGiaAsc" => query.OrderBy(x => x.HangHoaHoaDon.DonGiaTruocThue),
                "DonGiaDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.DonGiaTruocThue),
                "ThanhTienAsc" => query.OrderBy(x => x.HangHoaHoaDon.TongGiaTruocThue),
                "ThanhTienDesc" => query.OrderByDescending(x => x.HangHoaHoaDon.TongGiaTruocThue),
                _ => query.OrderByDescending(x => x.HoaDonXuat.NgayNhan)
            };

            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);
            XuatKhos = await query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToListAsync();

            return Page();
        }
    }
}
