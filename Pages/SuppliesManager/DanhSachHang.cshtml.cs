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
        public IActionResult OnGet(
    string hanghoa,
    int? NhomHangHoaId,
    string sortOrder,
    int pageNumber = 1,
    bool exportExcelVatTu = false,
    bool exportExcelPTTT = false,
    bool exportExcelCCDC = false,
    bool exportExcelTSCD = false,
    int? year = null)
        {
            NhomHangs = dBContext.NhomHangs.ToList();

            IQueryable<HangHoa> query = dBContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh);

            // Lọc theo nhóm hàng
            if (NhomHangHoaId.HasValue)
            {
                query = query.Where(t => t.NhomHangId == NhomHangHoaId.Value);
            }

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

            // Kiểm tra xuất Excel
            /*            if (exportExcelVatTu)
                        {
                            var vatTu = query.Where(h => h.NhomHangId == 2).ToList();
                            return ExportExcel(vatTu, $"VatTu_{year}", NhomHangHoaId);
                        }
                        if (exportExcelPTTT)
                        {
                            var phuTung = query.Where(h => h.NhomHangId == 3).ToList();
                            return ExportExcel(phuTung, $"PTTT_{year}", NhomHangHoaId);
                        }
                        if (exportExcelCCDC)
                        {
                            var ccdc = query.Where(h => h.NhomHangId == 1).ToList();
                            return ExportExcel(ccdc, $"CCDC_{year}", NhomHangHoaId);
                        }
                        if (exportExcelTSCD)
                        {
                            var tscd = query.Where(h => h.NhomHangId == 4).ToList();
                            return ExportExcel(tscd, $"TSCD_{year}", NhomHangHoaId);
                        }*/

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

        public IActionResult ExportExcel(string hanghoa, int? year, int? NhomHangHoaId)
        {
            var query = dBContext.HangHoas.AsQueryable();

            if (!string.IsNullOrEmpty(hanghoa))
            {
                query = query.Where(h => h.TenHangHoa.Contains(hanghoa));
            }

            if (year.HasValue)
            {
                query = query.Where(h => h.NgayNhap.Year == year.Value);
            }

            if (NhomHangHoaId.HasValue)
            {
                query = query.Where(h => h.NhomHangId == NhomHangHoaId.Value);
            }

            var hangHoaList = query.ToList();  // Lấy danh sách sau khi lọc

            // Tạo Excel với EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách hàng hóa");

                worksheet.Cells[1, 1].Value = "Mã hàng hóa";
                worksheet.Cells[1, 2].Value = "Tên hàng hóa";
                worksheet.Cells[1, 3].Value = "Nhóm hàng";
                worksheet.Cells[1, 4].Value = "Năm";

                int row = 2;
                foreach (var hangHoa in hangHoaList)
                {
                    worksheet.Cells[row, 1].Value = hangHoa.Id;
                    worksheet.Cells[row, 2].Value = hangHoa.TenHangHoa;
                    worksheet.Cells[row, 3].Value = hangHoa.NhomHang.Name;  // Lấy tên nhóm hàng
                    worksheet.Cells[row, 4].Value = hangHoa.NgayNhap.Year;
                    row++;
                }

                // Trả về file Excel
                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachHangHoa.xlsx");
            }
        }



    }
}
