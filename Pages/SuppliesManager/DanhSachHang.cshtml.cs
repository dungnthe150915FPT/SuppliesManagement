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

            IQueryable<HangHoa> query = dBContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh);

            // Filtering
            if (NhomHangHoaId.HasValue)
            {
                query = query.Where(t => t.NhomHangId == NhomHangHoaId.Value);
            }

            if (Year.HasValue)
            {
                query = query.Where(h => h.NgayNhap.Year == Year.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap <= EndDate.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(
                    t =>
                        t.TenHangHoa.Contains(SearchTerm)
                        || t.NhomHang.Name.Contains(SearchTerm)
                        || t.DonViTinh.Name.Contains(SearchTerm)
                );
            }

            // Sorting
            switch (SortOrder)
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

            // Pagination
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            HangHoas = query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            return Page();
        }
    }
}
