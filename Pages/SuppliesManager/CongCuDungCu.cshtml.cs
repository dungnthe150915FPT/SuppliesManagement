using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class CongCuDungCuModel : PageModel
    {
        public CongCuDungCuModel(SuppliesManagementProjectContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public List<HangHoa> HangHoas { get; set; }
        public List<NhomHang> NhomHangs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;
        private readonly SuppliesManagementProjectContext dBContext;

        public IActionResult OnGet(
            string hanghoa,
            string sortOrder,
            int pageNumber = 1,
            int? year = null
        )
        {
            NhomHangs = dBContext.NhomHangs.ToList();

            IQueryable<HangHoa> query = dBContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh)
                .Where(h => h.NhomHangId == 1);

            // Lọc theo năm nhập
            if (year.HasValue)
            {
                query = query.Where(h => h.NgayNhap.Year == year.Value);
            }

            // Tìm kiếm theo tên hàng hóa
            if (!string.IsNullOrEmpty(hanghoa))
            {
                query = query.Where(
                    t =>
                        t.TenHangHoa.Contains(hanghoa)
                        || t.NhomHang.Name.Contains(hanghoa)
                        || t.DonViTinh.Name.Contains(hanghoa)
                );
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

            HangHoas = query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            return Page();
        }

        public IActionResult OnPostExport(int year)
        {
            var currentDate = DateTime.Now;
            var khoHang = dBContext.KhoHangs.FirstOrDefault(); // Assuming single record
            var hangHoas = dBContext.HangHoas
                .Include(h => h.DonViTinh)
                .Where(h => h.NgayNhap.Year == year && h.NhomHangId == 1)
                .ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Công Cụ Dụng Cụ năm " +year);

            // Setting default font
            worksheet.Cells.Style.Font.Name = "Times New Roman";
            worksheet.Cells.Style.Font.Size = 12;

            // Header Information
            worksheet.Cells["A1"].Value = $"Đơn vị: {khoHang?.Ten}";
            worksheet.Cells["A2"].Value = $"Địa chỉ: {khoHang?.DiaChi}";
            worksheet.Cells["A3"].Value = $"SỔ THEO DÕI CÔNG CỤ DỤNG CỤ";
            worksheet.Cells["A3"].Style.Font.Size = 14;
            worksheet.Cells["A3"].Style.Font.Bold = true;

            worksheet.Cells["A4"].Value = $"Năm: {year}";
            worksheet.Cells["A1:A4"].Style.Font.Bold = true;

            // Merge header cells
            worksheet.Cells["A1:L1"].Merge = true;
            worksheet.Cells["A2:L2"].Merge = true;
            worksheet.Cells["A3:L3"].Merge = true;
            worksheet.Cells["A4:L4"].Merge = true;

            worksheet.Cells["A1:A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Table Headers
            string[] headers =
            {
                "STT",
                "Tên CCDC",
                "Mã CCDC",
                "Đặc điểm/TSKT",
                "ĐVT",
                "QĐ tăng",
                "N.Tháng Tăng",
                "QĐ giảm",
                "N.Tháng Giảm",
                "Lý do giảm",
                "Ghi chú"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[5, i + 1].Value = headers[i];
            }

            worksheet.Cells["A5:L5"].Style.Font.Bold = true;
            worksheet.Cells["A5:L5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A5:L5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Adjust column widths
            worksheet.Column(1).Width = 5; // STT
            worksheet.Column(2).Width = 30; // Tên CCDC
            worksheet.Column(3).Width = 20; // Mã CCDC
            worksheet.Column(4).Width = 15; // Đặc điểm
            worksheet.Column(5).Width = 10; // ĐVT
            worksheet.Column(6).Width = 15; // Quyết định ghi tăng
            worksheet.Column(7).Width = 15; // Ngày tháng tăng
            worksheet.Column(8).Width = 15; // Quyết định ghi giảm
            worksheet.Column(9).Width = 15; // Ngày tháng giảm
            worksheet.Column(10).Width = 15; // Lý do giảm
            worksheet.Column(11).Width = 15; // Ghi chú

            // Add border around the table header
            worksheet.Cells["A5:K5"]
                .Style
                .Border
                .Top
                .Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A5:K5"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A5:K5"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A5:K5"].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            // Data Rows
            int rowIndex = 6;
            int stt = 1;
            foreach (var hangHoa in hangHoas)
            {
                worksheet.Cells[rowIndex, 1].Value = stt++; // STT
                worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa; // Tên CCDC
                worksheet.Cells[rowIndex, 3].Value = hangHoa.Id.ToString(); // Mã CCDC (GUID)
                worksheet.Cells[rowIndex, 4].Value = ""; // Đặc điểm
                worksheet.Cells[rowIndex, 5].Value = hangHoa.DonViTinh?.Name; // ĐVT
                worksheet.Cells[rowIndex, 6].Value = ""; // Quyết định ghi tăng
                worksheet.Cells[rowIndex, 7].Value = hangHoa.NgayNhap.ToString("dd/MM/yyyy"); // Ngày tháng tăng
                worksheet.Cells[rowIndex, 8].Value = ""; // Quyết định ghi giảm
                worksheet.Cells[rowIndex, 9].Value = ""; // Ngày tháng giảm
                worksheet.Cells[rowIndex, 10].Value = ""; // Lý do giảm
                worksheet.Cells[rowIndex, 11].Value = ""; // Ghi chú

                // Add border for each data row
                worksheet.Cells[$"A{rowIndex}:K{rowIndex}"]
                    .Style
                    .Border
                    .Top
                    .Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{rowIndex}:K{rowIndex}"].Style.Border.Bottom.Style =
                    ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{rowIndex}:K{rowIndex}"].Style.Border.Left.Style =
                    ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{rowIndex}:K{rowIndex}"].Style.Border.Right.Style =
                    ExcelBorderStyle.Thin;

                rowIndex++;
            }

            // Footer
            worksheet.Cells[rowIndex + 2, 1].Value =
                $"Ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}";
            worksheet.Cells[rowIndex + 2, 1].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, 11].Merge = true;

            worksheet.Cells[rowIndex + 4, 1].Value = "Người ghi sổ";
            worksheet.Cells[rowIndex + 4, 5].Value = "Phụ trách kế toán";
            worksheet.Cells[rowIndex + 4, 10].Value = "Giám đốc";

            worksheet.Cells[rowIndex + 5, 1].Value = "(Ký, họ tên)";
            worksheet.Cells[rowIndex + 5, 5].Value = "(Ký, họ tên)";
            worksheet.Cells[rowIndex + 5, 10].Value = "(Ký, họ tên)";

            worksheet.Cells[rowIndex + 8, 1].Value = "Dương Mạnh Tuấn";
            worksheet.Cells[rowIndex + 8, 5].Value = "Nguyễn Thị Hảo";
            worksheet.Cells[rowIndex + 8, 10].Value = "Đỗ Công Biên";

            // Save and return file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sổ_CCDC_{year}.xlsx"
            );
        }
    }
}
