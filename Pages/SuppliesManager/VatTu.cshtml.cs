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
                worksheet.Cells.Style.Font.Size = 14;

                // Header Information
                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1:F1"].Style.Font.Bold = true;
                worksheet.Cells["A1:F2"].Style.Font.Size = 13;
                var a1Text = worksheet.Cells["A1"].RichText;
                var khohang = a1Text.Add("Đơn vị: ");
                var tenkhohang = a1Text.Add(khoHang?.Ten);
                worksheet.Cells["A2:F2"].Merge = true;
                worksheet.Cells["A2:F3"].Style.Font.Bold = true;
                var a2Text = worksheet.Cells["A2"].RichText;
                var diachi = a2Text.Add("Địa chỉ: ");
                var tendiachi = a2Text.Add(khoHang?.DiaChi);
                tendiachi.Bold = false;
                tenkhohang.Bold = false;

                worksheet.Cells["A4:F6"].Merge = true;
                worksheet.Cells["A4:F6"].Style.WrapText = true; // Enable text wrapping
                var a4Text = worksheet.Cells["A4"].RichText;
                var tieude1 = a4Text.Add("SỔ THEO DÕI VẬT TƯ");
                tieude1.Size = 16;
                tieude1.Bold = true;
                a4Text.Add("\n");
                var tieude2 = a4Text.Add(
                    "(Phục vụ hoạt động cung cấp dịch vụ sự nghiệp công TTDH)"
                );
                tieude2.Size = 13;
                a4Text.Add("\n");
                var namtieude = a4Text.Add($"Tháng {month} năm {year}");
                namtieude.Size = 13;

                worksheet.Cells["A4:F6"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["A4:F6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Adjust row height to accommodate the larger content
                worksheet.Row(4).Height = 30;
                // namtieude.
                // Center align the text
                worksheet.Cells["A3:F4"]
                    .Style
                    .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A3:F4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A1:A3"].Style.Font.Bold = true;

                // Merge header cells

                worksheet.Cells["A1:A2"]
                    .Style
                    .HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Header bảng
                string[] headers =
                {
                    "STT",
                    "Tên Vật Tư",
                    // "Tồn đầu kì",
                    // "Nhập trong kì",
                    // "Xuất trong kì",
                    // "Tồn cuối kì"
                    "Số lượng"
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[7, i + 1].Value = headers[i];
                }

                worksheet.Cells["A7:F8"].Style.Font.Bold = true;
                worksheet.Cells["A7:F8"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["A7:F8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A7:A8"].Merge = true;
                worksheet.Cells["B7:B8"].Merge = true;
                worksheet.Cells["C7:F7"].Merge = true;
                worksheet.Cells["C8"].Value = "Tồn đầu kì";
                worksheet.Cells["D8"].Value = "Nhập trong kì";
                worksheet.Cells["E8"].Value = "Xuất trong kì";
                worksheet.Cells["F8"].Value = "Tồn cuối kì";

                var modelCells1 = worksheet.Cells["C8"];
                var border1 =
                    modelCells1.Style.Border.Top.Style =
                    modelCells1.Style.Border.Left.Style =
                    modelCells1.Style.Border.Right.Style =
                    modelCells1.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                var modelCells2 = worksheet.Cells["D8"];
                var border2 =
                    modelCells2.Style.Border.Top.Style =
                    modelCells2.Style.Border.Left.Style =
                    modelCells2.Style.Border.Right.Style =
                    modelCells2.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                var modelCells3 = worksheet.Cells["E8"];
                var border3 =
                    modelCells3.Style.Border.Top.Style =
                    modelCells3.Style.Border.Left.Style =
                    modelCells3.Style.Border.Right.Style =
                    modelCells3.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                var modelCells4 = worksheet.Cells["F8"];
                var border4 =
                    modelCells4.Style.Border.Top.Style =
                    modelCells4.Style.Border.Left.Style =
                    modelCells4.Style.Border.Right.Style =
                    modelCells4.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                var modelCells5 = worksheet.Cells["A7:A8"];
                var border5 =
                    modelCells5.Style.Border.Top.Style =
                    modelCells5.Style.Border.Left.Style =
                    modelCells5.Style.Border.Right.Style =
                    modelCells5.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                var modelCells6 = worksheet.Cells["B7:B8"];
                var border6 =
                    modelCells6.Style.Border.Top.Style =
                    modelCells6.Style.Border.Left.Style =
                    modelCells6.Style.Border.Right.Style =
                    modelCells6.Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                worksheet.Row(8).Height = 30;
                // Thiết lập độ rộng cột
                worksheet.Column(1).Width = 5; // STT
                worksheet.Column(2).Width = 40; // Tên Vật tư
                worksheet.Column(3).Width = 20; // Tồn đầu kì
                worksheet.Column(4).Width = 20; // Nhập trong kì
                worksheet.Column(5).Width = 20; // Xuất trong kì
                worksheet.Column(6).Width = 20; // Tồn cuối kì

                // Border cho tiêu đề
                worksheet.Cells["A7:F8"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Điền dữ liệu
                int rowIndex = 9;
                int stt = 1;
                decimal totalTonDauKy = 0;
                decimal totalNhapTrongKy = 0;
                decimal totalXuatTrongKy = 0;
                decimal totalTonCuoiKy = 0;
                worksheet.Cells["A8:F8"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A8:F8"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A8:F8"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A8:F8"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
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
                    totalTonDauKy += tonDauKy;
                    totalNhapTrongKy += nhapTrongKy;
                    totalXuatTrongKy += xuatTrongKy;
                    totalTonCuoiKy += tonCuoiKy;
                    // Điền dữ liệu vào các cột
                    worksheet.Cells[rowIndex, 1].Value = stt++; // STT
                    worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa; // Tên Vật tư
                    worksheet.Cells[rowIndex, 3].Value = tonDauKy; // Tồn đầu kỳ
                    worksheet.Cells[rowIndex, 4].Value = nhapTrongKy; // Nhập trong kỳ
                    worksheet.Cells[rowIndex, 5].Value = xuatTrongKy; // Xuất trong kỳ
                    worksheet.Cells[rowIndex, 6].Value = tonCuoiKy; // Tồn cuối kỳ

                    worksheet.Cells[rowIndex, 3, rowIndex, 6].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.VerticalAlignment =
                        ExcelVerticalAlignment.Center;
                    // worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.HorizontalAlignment =
                    //     ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6]
                        .Style
                        .Border
                        .Top
                        .Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Border.Left.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Border.Right.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[rowIndex, 1, rowIndex, 6].Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                    // Border cho dữ liệu
                    worksheet.Cells[$"A{rowIndex}:F{rowIndex}"].Style.Border.BorderAround(
                        ExcelBorderStyle.Thin
                    );
                    rowIndex++;
                }

                var tableRange = worksheet.Cells[8, 1, rowIndex - 1, 6];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // AutoFit the "Tên Vật Tư" column
                worksheet.Column(2).AutoFit();

                // Set a maximum width for the "Tên Vật Tư" column (optional)
                double maxWidth = 50; // Set your desired maximum width
                if (worksheet.Column(2).Width > maxWidth)
                {
                    worksheet.Column(2).Width = maxWidth;
                    worksheet.Column(2).Style.WrapText = true;
                }
                worksheet.Cells[rowIndex, 2].Value = "Tổng cộng: ";
                worksheet.Cells[rowIndex, 2].Style.Font.Bold = true;
                worksheet.Cells[rowIndex, 3].Value = totalTonDauKy;
                worksheet.Cells[rowIndex, 4].Value = totalNhapTrongKy;
                worksheet.Cells[rowIndex, 5].Value = totalXuatTrongKy;
                worksheet.Cells[rowIndex, 6].Value = totalTonCuoiKy;
                worksheet.Cells[rowIndex, 3, rowIndex, 6].Style.Numberformat.Format = "#,##0";

                // Apply center alignment and bold font to total cells
                worksheet.Cells[rowIndex, 3, rowIndex, 6]
                    .Style
                    .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, 3, rowIndex, 6].Style.Font.Bold = true;

                worksheet.Cells[rowIndex, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[rowIndex, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[rowIndex, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[rowIndex, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[rowIndex, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[rowIndex, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Footer
                worksheet.Cells[rowIndex + 2, 5].Value =
                    $"Ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}";
                worksheet.Cells[rowIndex + 2, 5].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 2, 5, rowIndex + 2, 6].Merge = true;

                worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 2].Value = "Người ghi sổ";
                worksheet.Cells[rowIndex + 3, 3, rowIndex + 3, 4].Value = "Phụ trách kế toán";
                worksheet.Cells[rowIndex + 3, 5, rowIndex + 3, 6].Value =
                    "Giám đốc Đài TTXLTTHH Hà Nội";
                worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 2].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 3, 3, rowIndex + 3, 4].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 3, 5, rowIndex + 3, 6].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 3, 5, rowIndex + 3, 6].Merge = true;
                worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 2].Merge = true;
                worksheet.Cells[rowIndex + 3, 3, rowIndex + 3, 4].Merge = true;
                worksheet.Cells[rowIndex + 3, 1, rowIndex + 3, 6].Style.Font.Bold = true;
                // worksheet.Cells[rowIndex + 5, 1].Value = "(Ký, họ tên)";
                // worksheet.Cells[rowIndex + 5, 3].Value = "(Ký, họ tên)";
                // worksheet.Cells[rowIndex + 5, 5].Value = "(Ký, họ tên)";

                worksheet.Cells[rowIndex + 9, 1, rowIndex + 9, 2].Value = "Dương Mạnh Tuấn";
                worksheet.Cells[rowIndex + 9, 3, rowIndex + 9, 4].Value = "Nguyễn Thị Hảo";
                worksheet.Cells[rowIndex + 9, 5, rowIndex + 9, 6].Value = "Đỗ Công Biên";
                worksheet.Cells[rowIndex + 9, 5, rowIndex + 9, 6].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 9, 1, rowIndex + 9, 2].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 9, 3, rowIndex + 9, 4].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex + 9, 3, rowIndex + 9, 4].Merge = true;
                worksheet.Cells[rowIndex + 9, 1, rowIndex + 9, 2].Merge = true;
                worksheet.Cells[rowIndex + 9, 5, rowIndex + 9, 6].Merge = true;

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
