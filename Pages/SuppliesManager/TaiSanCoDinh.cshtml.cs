using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class TaiSanCoDinhModel : PageModel
    {
        public TaiSanCoDinhModel(SuppliesManagementProjectContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [BindProperty(SupportsGet = true)]
        public string Hanghoa { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; }
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
                .Where(h => h.NhomHangId == 4);

            // Lọc theo năm nhập
            if (year.HasValue)
            {
                query = query.Where(h => h.NgayNhap.Year == year.Value);
            }
            if (!string.IsNullOrEmpty(Hanghoa))
            {
                query = query.Where(t => t.TenHangHoa.Contains(Hanghoa));
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
                .Where(h => h.NgayNhap.Year == year && h.NhomHangId == 4)
                .ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tài Sản Cố Định năm " + year);

            // Setting default font
            worksheet.Cells.Style.Font.Name = "Times New Roman";
            worksheet.Cells.Style.Font.Size = 14;

            // Header Information
            worksheet.Cells["A1:K1"].Merge = true;
            worksheet.Cells["A1:K1"].Style.Font.Bold = true;
            worksheet.Cells["A1:K2"].Style.Font.Size = 13;
            var a1Text = worksheet.Cells["A1"].RichText;
            var khohang = a1Text.Add("Đơn vị: ");
            var tenkhohang = a1Text.Add(khoHang?.Ten);
            worksheet.Cells["A2:K2"].Merge = true;
            worksheet.Cells["A2:K3"].Style.Font.Bold = true;
            var a2Text = worksheet.Cells["A2"].RichText;
            var diachi = a2Text.Add("Địa chỉ: ");
            var tendiachi = a2Text.Add(khoHang?.DiaChi);
            tendiachi.Bold = false;
            tenkhohang.Bold = false;

            worksheet.Cells["A3:K4"].Merge = true;
            worksheet.Cells["A3:K4"].Style.WrapText = true; // Enable text wrapping
            var a3Text = worksheet.Cells["A3"].RichText;
            var tieude = a3Text.Add("SỔ THEO DÕI TÀI SẢN CỐ ĐỊNH");
            a3Text.Add("\n"); // Line break
            var namtieude = a3Text.Add($"Năm: {year}");
            // Center align the text
            worksheet.Cells["A3:K4"]
                .Style
                .HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A3:K4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A1:A3"].Style.Font.Bold = true;

            // Merge header cells

            worksheet.Cells["A1:A2"]
                .Style
                .HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Table Headers
            worksheet.Cells["A6"].Value = "STT";
            worksheet.Cells["B6"].Value = "Tên TSCĐ";
            worksheet.Cells["C6"].Value = "Mã TSCĐ";
            worksheet.Cells["D6"].Value = "Đặc điểm/Thông số kỹ thuật";
            worksheet.Cells["E6"].Value = "ĐVT";
            worksheet.Cells["A6:A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A6:A8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["B6:B8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["B6:B8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["C6:C8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C6:C8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["D6:D8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["D6:D8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["E6:E8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["E6:E8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A6:A8"].Merge = true;
            worksheet.Cells["B6:B8"].Merge = true;
            worksheet.Cells["C6:C8"].Merge = true;
            worksheet.Cells["D6:D8"].Merge = true;
            worksheet.Cells["E6:E8"].Merge = true;

            // Merge cells for "Ghi tăng CCDC"
            worksheet.Cells["F6:G6"].Merge = true;
            worksheet.Cells["F6:G6"].Value = "Ghi tăng TSCĐ";
            worksheet.Cells["F6:G6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F6:G6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells["F7"].Value = "Quyết định";
            worksheet.Cells["F7:G7"].Merge = true;
            worksheet.Cells["F7:G7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F7:G7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["F8"].Value = "Số hiệu";
            worksheet.Cells["G8"].Value = "Ngày tháng";

            // Merge cells for "Ghi giảm CCDC"
            worksheet.Cells["H6:J6"].Merge = true;
            worksheet.Cells["H6:J6"].Value = "Ghi giảm TSCĐ";
            worksheet.Cells["H6:J6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["H6:J6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells["H7"].Value = "Quyết định";
            worksheet.Cells["H7:I7"].Merge = true;
            worksheet.Cells["H7:I7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["H7:I7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["H8"].Value = "Số hiệu";
            worksheet.Cells["I8"].Value = "Ngày tháng";
            worksheet.Cells["J7"].Value = "Lý do giảm";
            worksheet.Cells["J7:J8"].Merge = true;
            worksheet.Cells["J7:J8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["J7:J8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells["K6:K8"].Merge = true;
            worksheet.Cells["K6:K8"].Value = "Ghi chú";
            worksheet.Cells["K6:K8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["K6:K8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Formatting headers
            worksheet.Cells["A6:K8"]
                .Style
                .Font
                .Bold = true;
            worksheet.Cells["A6:K8"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A6:K8"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A6:K8"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A6:K8"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A6:K8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A6:K8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A6:K8"].Style.WrapText = true;

            // Adjust column widths
            worksheet.Column(1).Width = 6; // STT
            worksheet.Column(2).Width = 35; // Tên CCDC
            worksheet.Column(3).Width = 15; // Mã CCDC
            worksheet.Column(4).Width = 25; // Đặc điểm/Thông số kỹ thuật
            worksheet.Column(5).Width = 10; // ĐVT
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 12;
            worksheet.Column(9).Width = 15;
            worksheet.Column(10).Width = 15;
            worksheet.Column(11).Width = 20;

            // Data Rows
            int rowIndex = 9;
            int stt = 1;
            foreach (var hangHoa in hangHoas)
            {
                worksheet.Cells[rowIndex, 1].Value = stt++; // STT
                worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa; // Tên CCDC
                worksheet.Cells[rowIndex, 3].Value = ""; // Mã CCDC
                worksheet.Cells[rowIndex, 4].Value = ""; // Đặc điểm
                worksheet.Cells[rowIndex, 5].Value = hangHoa.DonViTinh?.Name; // ĐVT
                worksheet.Cells[rowIndex, 6].Value = ""; // số hiệu tăng
                worksheet.Cells[rowIndex, 7].Value = hangHoa.NgayNhap.ToString("dd/MM/yyyy"); // ngày tháng tăng
                worksheet.Cells[rowIndex, 8].Value = ""; // Số hiệu giảm
                worksheet.Cells[rowIndex, 9].Value = ""; // ngày tháng giảm
                worksheet.Cells[rowIndex, 10].Value = ""; // Lý do giảm
                worksheet.Cells[rowIndex, 11].Value = ""; // Ghi chú

                for (int col = 1; col <= 11; col++)
                {
                    worksheet.Cells[rowIndex, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                rowIndex++;
            }

            // Footer
            worksheet.Cells[rowIndex + 2, 8].Value =
                $"Ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}";
            worksheet.Cells[rowIndex + 2, 8, rowIndex + 2, 11].Merge = true;
            worksheet.Cells[rowIndex + 2, 8].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            worksheet.Cells[rowIndex + 3, 1].Value = "Người ghi sổ";
            worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 3].Merge = true;
            worksheet.Cells[rowIndex + 3, 1].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 3].Style.Font.Bold = true;
            worksheet.Cells[rowIndex + 3, 4].Value = "Phụ trách kế toán";
            worksheet.Cells[rowIndex + 3, 4, rowIndex + 3, 7].Style.Font.Bold = true;
            worksheet.Cells[rowIndex + 3, 4, rowIndex + 3, 7].Merge = true;
            worksheet.Cells[rowIndex + 3, 4].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex + 3, 8].Value = "Giám đốc Đài TTXLTTHH Hà Nội";
            worksheet.Cells[rowIndex + 3, 8].Style.Font.Bold = true;
            worksheet.Cells[rowIndex + 3, 8, rowIndex + 3, 11].Merge = true;
            worksheet.Cells[rowIndex + 3, 8].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            worksheet.Cells[rowIndex + 8, 1].Value = "Dương Mạnh Tuấn";
            worksheet.Cells[rowIndex + 8, 1, rowIndex + 8, 3].Merge = true;
            worksheet.Cells[rowIndex + 8, 1].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex + 8, 4].Value = "Nguyễn Thị Hảo";
            worksheet.Cells[rowIndex + 8, 4, rowIndex + 8, 7].Merge = true;
            worksheet.Cells[rowIndex + 8, 4].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex + 8, 8].Value = "Đỗ Công Biên";
            worksheet.Cells[rowIndex + 8, 8, rowIndex + 8, 11].Merge = true;
            worksheet.Cells[rowIndex + 8, 8].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            // Save and return file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sổ_TSCĐ_{year}.xlsx"
            );
        }
    }
}
