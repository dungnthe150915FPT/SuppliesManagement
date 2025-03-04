using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SuppliesManagement.Models;
using System.IO;
using SkiaSharp;
using System.Globalization;

namespace SuppliesManagement.Pages
{
    public class DanhSachHangModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        public DanhSachHangModel(SuppliesManagementProjectContext dBContext)
        {
            this.dBContext = dBContext;
        }
        public List<NhapKho> NhapKhos { get; set; }
        public List<HangHoa> HangHoas { get; set; }
        public List<NhomHang> NhomHangs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? NhomHangHoaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public IActionResult OnGet(int pageNumber = 1)
        {
            NhomHangs = dBContext.NhomHangs.ToList();

            IQueryable<NhapKho> query = dBContext.NhapKhos
                .Include(nk => nk.HangHoa) // Lấy thông tin hàng hóa
                .ThenInclude(h => h.KhoHang) // Từ hàng hóa, lấy thông tin Kho hàng
                .Include(nk => nk.HangHoa)
                .ThenInclude(h => h.NhomHang) // Từ hàng hóa, lấy thông tin Nhóm hàng
                .Include(nk => nk.HangHoa)
                .ThenInclude(h => h.DonViTinh) // Từ hàng hóa, lấy thông tin Đơn vị tính
                .Include(nk => nk.HoaDonNhap); // Lấy thông tin hóa đơn nhập

            // .ThenInclude(nk => nk.HangHoaHoaDons)
            // .ThenInclude(nk => nk.HoaDonNhaps);

            // Filtering
            if (NhomHangHoaId.HasValue)
            {
                query = query.Where(t => t.HangHoa.NhomHangId == NhomHangHoaId.Value);
            }

            if (Year.HasValue)
            {
                query = query.Where(h => h.HangHoa.NgayNhap.Year == Year.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(h => h.HangHoa.NgayNhap >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(h => h.HangHoa.NgayNhap <= EndDate.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(
                    t =>
                        t.HangHoa.TenHangHoa.Contains(SearchTerm)
                        || t.HangHoa.NhomHang.Name.Contains(SearchTerm)
                        || t.HangHoa.DonViTinh.Name.Contains(SearchTerm)
                );
            }

            // Sorting
            switch (SortOrder)
            {
                case "SoLuongAsc":
                    query = query.OrderBy(h => h.HangHoa.SoLuong);
                    break;
                case "SoLuongDesc":
                    query = query.OrderByDescending(h => h.HangHoa.SoLuong);
                    break;
                case "DonGiaAsc":
                    query = query.OrderBy(h => h.HangHoa.DonGiaTruocThue);
                    break;
                case "DonGiaDesc":
                    query = query.OrderByDescending(h => h.HangHoa.DonGiaTruocThue);
                    break;
                case "ThanhTienAsc":
                    query = query.OrderBy(h => h.HangHoa.TongGiaTruocThue);
                    break;
                case "ThanhTienDesc":
                    query = query.OrderByDescending(h => h.HangHoa.TongGiaTruocThue);
                    break;
                default:
                    query = query.OrderByDescending(h => h.HangHoa.SoLuong);
                    break;
            }

            // Pagination
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            NhapKhos = query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            return Page();
        }
    }
}
