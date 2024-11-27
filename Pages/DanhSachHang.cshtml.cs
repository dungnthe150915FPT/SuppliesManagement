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
        public int CurrentPage { get; set; } // Trang hiện tại
        public int TotalPages { get; set; }  // Tổng số trang
        public const int PageSize = 10;      // Số hàng hóa trên mỗi trang

        public IActionResult OnGet(
    string hanghoa,
    int? NhomHangHoaId,
    string sortOrder,
    int pageNumber = 1,
    bool exportExcelVatTu = false,
    bool exportExcelPTTT = false,
    bool exportExcelCCDC = false)
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

            // Tìm kiếm theo tên hàng hóa
            if (!string.IsNullOrEmpty(hanghoa))
            {
                query = query.Where(t => t.TenHangHoa.Contains(hanghoa));
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

            if (exportExcelVatTu)
            {
                var vatTu = query.Where(h => h.NhomHangId == 2).ToList();
                return ExportExcel(vatTu, "VatTu");
            }
            if (exportExcelPTTT)
            {
                var phuTung = query.Where(h => h.NhomHangId == 3).ToList();
                return ExportExcel(phuTung, "PTTT");
            }
            if (exportExcelCCDC)
            {
                var ccdc = query.Where(h => h.NhomHangId == 1).ToList();
                return ExportExcel(ccdc, "CCDC");
            }

            // Phân trang
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            HangHoas = query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }


        private IActionResult ExportExcel(List<HangHoa> hangHoas)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 

            using var package = new ExcelPackage();

            // Tạo sheet trong file Excel
            var worksheet = package.Workbook.Worksheets.Add("Hàng hóa");

            // Tiêu đề
            worksheet.Cells["A1"].Value = "Mã hàng hóa";
            worksheet.Cells["B1"].Value = "Tên hàng hóa";
            worksheet.Cells["C1"].Value = "Loại hàng";
            worksheet.Cells["D1"].Value = "Đơn vị tính";
            worksheet.Cells["E1"].Value = "Số lượng nhập";
            worksheet.Cells["F1"].Value = "Đơn giá";
            worksheet.Cells["G1"].Value = "Thành tiền";

            // Styling tiêu đề
            using (var range = worksheet.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Dữ liệu
            int row = 2;
            foreach (var hanghoa in hangHoas)
            {
                worksheet.Cells[row, 1].Value = hanghoa.Id;
                worksheet.Cells[row, 2].Value = hanghoa.TenHangHoa;
                worksheet.Cells[row, 3].Value = hanghoa.NhomHang.Name;
                worksheet.Cells[row, 4].Value = hanghoa.DonViTinh.Name;
                worksheet.Cells[row, 5].Value = hanghoa.SoLuong;
                worksheet.Cells[row, 6].Value = hanghoa.DonGiaTruocThue;
                worksheet.Cells[row, 7].Value = hanghoa.TongGiaTruocThue;

                using (var range = worksheet.Cells[row, 1, row, 7])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Sổ_CCDC.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        private IActionResult ExportExcel(List<HangHoa> hangHoas, string fileNameSuffix)
        {
            var package = new ExcelPackage();

            // Tạo sheet trong file Excel
            var worksheet = package.Workbook.Worksheets.Add("Hàng hóa");

            // Tiêu đề
            worksheet.Cells["A1"].Value = "Mã hàng hóa";
            worksheet.Cells["B1"].Value = "Tên hàng hóa";
            worksheet.Cells["C1"].Value = "Loại hàng";
            worksheet.Cells["D1"].Value = "Đơn vị tính";
            worksheet.Cells["E1"].Value = "Số lượng";
            worksheet.Cells["F1"].Value = "Đơn giá";
            worksheet.Cells["G1"].Value = "Thành tiền";

            // Styling tiêu đề
            using (var range = worksheet.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Dữ liệu
            int row = 2;
            foreach (var hanghoa in hangHoas)
            {
                worksheet.Cells[row, 1].Value = hanghoa.Id;
                worksheet.Cells[row, 2].Value = hanghoa.TenHangHoa;
                worksheet.Cells[row, 3].Value = hanghoa.NhomHang.Name;
                worksheet.Cells[row, 4].Value = hanghoa.DonViTinh.Name;
                worksheet.Cells[row, 5].Value = hanghoa.SoLuong;
                worksheet.Cells[row, 6].Value = hanghoa.DonGiaTruocThue;
                worksheet.Cells[row, 7].Value = hanghoa.TongGiaTruocThue;
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Xuất file Excel ra trình duyệt
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"DanhSach_{fileNameSuffix}_TrongKhoNgay{DateTime.Now:dd/MM/yyyy}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

    }
}
