using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages
{
    public class VatTuModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        public VatTuModel(SuppliesManagementProjectContext dBContext)
        {
            this.dBContext = dBContext;
        }
        public List<HangHoa> HangHoas { get; set; }
        public List<NhomHang> NhomHangs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;
        public IActionResult OnGet(
    string hanghoa,
    string sortOrder,
    int pageNumber = 1,
    int? year = null)
        {
            NhomHangs = dBContext.NhomHangs.ToList();

            IQueryable<HangHoa> query = dBContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh).Where(h => h.NhomHangId == 2);

            // Lọc theo năm nhập
            if (year.HasValue)
            {
                query = query.Where(h => h.NgayNhap.Year == year.Value);
            }

            // Tìm kiếm theo tên hàng hóa
            if (!string.IsNullOrEmpty(hanghoa))
            {
                query = query.Where(t => t.TenHangHoa.Contains(hanghoa) ||
                                         t.NhomHang.Name.Contains(hanghoa) ||
                                         t.DonViTinh.Name.Contains(hanghoa));
            }

            // Sắp xếp theo tiêu chí
            switch (sortOrder)
            {
                case "SoLuongAsc":
                    query = query.OrderBy(h => h.SoLuong);
                    break;
                case "SoLuongDesc":
                    query = query.OrderByDescending(h => h.SoLuong);
                    break;
                case "DonGiaAsc":
                    query = query.OrderBy(h => h.DonGiaTruocThue);
                    break;
                case "DonGiaDesc":
                    query = query.OrderByDescending(h => h.DonGiaTruocThue);
                    break;
                case "ThanhTienAsc":
                    query = query.OrderBy(h => h.TongGiaTruocThue);
                    break;
                case "ThanhTienDesc":
                    query = query.OrderByDescending(h => h.TongGiaTruocThue);
                    break;
                default:
                    query = query.OrderByDescending(h => h.SoLuong);
                    break;
            }

            // Phân trang
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            HangHoas = query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            return Page();
        }
    }
}
