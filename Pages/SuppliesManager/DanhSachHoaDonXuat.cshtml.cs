using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Entities.ViewModels;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class DanhSachHoaDonXuatModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public DanhSachHoaDonXuatModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }

        public List<HoaDonXuatViewModel> HoaDonXuats { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public IActionResult OnGet(DateTime? startDate, DateTime? endDate, int pageNumber = 1)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            var query = context.HoaDonXuats
                .Include(x => x.KhoHang)
                .Include(x => x.NguoiNhan)
                .AsQueryable();

            if (StartDate.HasValue)
            {
                query = query.Where(h => h.NgayNhan >= StartDate.Value);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(h => h.NgayNhan <= EndDate.Value);
            }
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(
                    h =>
                        h.NguoiNhan.Fullname.Contains(SearchTerm)
                        || h.Id.ToString().Contains(SearchTerm)
                        || h.KhoHang.Ten.Contains(SearchTerm)
                        || h.LyDoNhan.Contains(SearchTerm)
                        || h.ThanhTien.ToString().Contains(SearchTerm)
                        || h.NguoiNhan.Username.Contains(SearchTerm)
                );
            }

            // Tổng số hóa đơn để xác định số trang
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            // Lấy hóa đơn cho trang hiện tại
            HoaDonXuats = query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(
                    h =>
                        new HoaDonXuatViewModel
                        {
                            Id = h.Id,
                            LyDoNhan = h.LyDoNhan,
                            ThanhTien = h.ThanhTien,
                            NgayNhan = h.NgayNhan,
                            NguoiNhanUsername = h.NguoiNhan.Username,
                            KhoHang = h.KhoHang.Ten,
                            HangHoas = context.XuatKhos
                                .Where(n => n.HoaDonXuatId == h.Id)
                                .Include(n => n.HangHoaHoaDon)
                                .ThenInclude(n => n.KhoHang)
                                .Select(
                                    n =>
                                        new HangHoaViewModel
                                        {
                                            TenHangHoa = n.HangHoaHoaDon.TenHangHoa,
                                            SoLuong = n.HangHoaHoaDon.SoLuong,
                                            DonGiaTruocThue = n.HangHoaHoaDon.DonGiaTruocThue,
                                            DonGiaSauThue = n.HangHoaHoaDon.DonGiaSauThue,
                                            TongGiaTruocThue = n.HangHoaHoaDon.TongGiaTruocThue,
                                            TongGiaSauThue = n.HangHoaHoaDon.TongGiaSauThue
                                        }
                                )
                                .ToList()
                        }
                )
                .ToList();
            return Page();
        }
    }
}
