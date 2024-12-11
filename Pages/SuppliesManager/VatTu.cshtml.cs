using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
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
            int? year = null,
            int? month = null
        )
        {
            NhomHangs = dBContext.NhomHangs.ToList();

            IQueryable<HangHoa> query = dBContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh)
                .Where(h => h.NhomHangId == 2);

            // Lọc theo năm nhập
            if (year.HasValue)
            {
                query = query.Where(
                    h => h.NgayNhap.Month == month.Value && h.NgayNhap.Year == year.Value
                );
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

        public IActionResult OnPostExport(int year, int month)
        {
            // Kiểm tra tính hợp lệ của tham số
            if (year <= 0 || month <= 0 || month > 12)
            {
                ModelState.AddModelError(string.Empty, "Tháng hoặc năm không hợp lệ.");
                return Page();
            }

            try
            {
                // Xác định khoảng thời gian đầu tháng và cuối tháng
                var startOfMonth = new DateTime(year, month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                var currentDate = DateTime.Now;

                // Lấy thông tin kho hàng
                var khoHang = dBContext.KhoHangs.FirstOrDefault();
                var hangHoas = dBContext.HangHoas
                    .Include(h => h.DonViTinh)
                    .Where(
                        h =>
                            h.NgayNhap >= startOfMonth
                            && h.NgayNhap <= endOfMonth
                            && h.NhomHangId == 2
                    )
                    .ToList();

                // Tạo file Excel
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add($"Vật Tư tháng {month}/{year}");

                // Thiết lập font mặc định
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 12;

                // Thông tin tiêu đề
                worksheet.Cells["A1"].Value = $"Đơn vị: {khoHang?.Ten}";
                worksheet.Cells["A2"].Value = $"Địa chỉ: {khoHang?.DiaChi}";
                worksheet.Cells["A3"].Value = $"SỔ THEO DÕI VẬT TƯ";
                worksheet.Cells["A3"].Style.Font.Size = 14;
                worksheet.Cells["A3"].Style.Font.Bold = true;

                worksheet.Cells["A4"].Value = $"Tháng: {month}/{year}";
                worksheet.Cells["A1:A4"].Style.Font.Bold = true;

                // Merge ô tiêu đề
                worksheet.Cells["A1:L1"].Merge = true;
                worksheet.Cells["A2:L2"].Merge = true;
                worksheet.Cells["A3:L3"].Merge = true;
                worksheet.Cells["A4:L4"].Merge = true;

                worksheet.Cells["A1:A4"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Header bảng
                string[] headers =
                {
                    "STT",
                    "Tên Vật Tư",
                    "Tồn đầu kì",
                    "Nhập trong kì",
                    "Xuất trong kì",
                    "Tồn cuối kì"
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[5, i + 1].Value = headers[i];
                }

                worksheet.Cells["A5:F5"].Style.Font.Bold = true;
                worksheet.Cells["A5:F5"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["A5:F5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Thiết lập độ rộng cột
                worksheet.Column(1).Width = 5; // STT
                worksheet.Column(2).Width = 30; // Tên Vật tư
                worksheet.Column(3).Width = 15; // Tồn đầu kì
                worksheet.Column(4).Width = 15; // Nhập trong kì
                worksheet.Column(5).Width = 15; // Xuất trong kì
                worksheet.Column(6).Width = 15; // Tồn cuối kì

                // Border cho tiêu đề
                worksheet.Cells["A5:F5"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Điền dữ liệu
                int rowIndex = 6;
                int stt = 1;

                foreach (var hangHoa in hangHoas)
                {
                    // Tính toán số liệu tồn kho
                    var tonDauKy = hangHoa.NgayNhap < startOfMonth ? hangHoa.SoLuongConLai : 0;
                    var nhapTrongKy = dBContext.HangHoas
                        .Where(
                            h =>
                                h.Id == hangHoa.Id
                                && h.NgayNhap >= startOfMonth
                                && h.NgayNhap <= endOfMonth
                        )
                        .Sum(h => h.SoLuong);
                    var xuatTrongKy = dBContext.HangHoas
                        .Where(
                            h =>
                                h.Id == hangHoa.Id
                                && h.NgayNhap >= startOfMonth
                                && h.NgayNhap <= endOfMonth
                        )
                        .Sum(h => h.SoLuongDaXuat);
                    var tonCuoiKy = hangHoa.NgayNhap <= endOfMonth ? hangHoa.SoLuongConLai : 0;

                    // Điền dữ liệu vào các cột
                    worksheet.Cells[rowIndex, 1].Value = stt++; // STT
                    worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa; // Tên Vật tư
                    worksheet.Cells[rowIndex, 3].Value = tonDauKy; // Tồn đầu kỳ
                    worksheet.Cells[rowIndex, 4].Value = nhapTrongKy; // Nhập trong kỳ
                    worksheet.Cells[rowIndex, 5].Value = xuatTrongKy; // Xuất trong kỳ
                    worksheet.Cells[rowIndex, 6].Value = tonCuoiKy; // Tồn cuối kỳ

                    // Border cho dữ liệu
                    worksheet.Cells[$"A{rowIndex}:F{rowIndex}"].Style.Border.BorderAround(
                        ExcelBorderStyle.Thin
                    );
                    rowIndex++;
                }

                // Footer
                worksheet.Cells[rowIndex + 2, 1].Value =
                    $"Ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}";
                worksheet.Cells[rowIndex + 2, 1].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 2, 1, rowIndex + 2, 6].Merge = true;

                worksheet.Cells[rowIndex + 4, 1].Value = "Người ghi sổ";
                worksheet.Cells[rowIndex + 4, 3].Value = "Phụ trách kế toán";
                worksheet.Cells[rowIndex + 4, 5].Value = "Giám đốc";

                worksheet.Cells[rowIndex + 5, 1].Value = "(Ký, họ tên)";
                worksheet.Cells[rowIndex + 5, 3].Value = "(Ký, họ tên)";
                worksheet.Cells[rowIndex + 5, 5].Value = "(Ký, họ tên)";

                worksheet.Cells[rowIndex + 8, 1].Value = "Dương Mạnh Tuấn";
                worksheet.Cells[rowIndex + 8, 3].Value = "Nguyễn Thị Hảo";
                worksheet.Cells[rowIndex + 8, 5].Value = "Đỗ Công Biên";

                // Xuất file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Sổ Theo Dõi Vật Tư Tháng {month}-{year}.xlsx"
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                return Page();
            }
        }
    }
}
