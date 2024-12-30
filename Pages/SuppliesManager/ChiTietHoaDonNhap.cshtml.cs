using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SuppliesManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SuppliesManagement.Models;
using System.Globalization;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class ChiTietHoaDonNhapModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        public ChiTietHoaDonNhapModel(SuppliesManagementProjectContext dbContext)
        {
            dBContext = dbContext;
        }

        public HoaDonNhapDetailViewModel HoaDonNhap { get; set; }
        public bool HasPDF { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            var hoaDon = await dBContext.HoaDonNhaps
                .Include(h => h.KhoHang)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            var hangHoas = await dBContext.NhapKhos
                .Where(n => n.HoaDonNhapId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(n => n.NhomHang)
                .Select(
                    n =>
                        new HangHoaViewModel
                        {
                            TenKhoHang = n.HangHoaHoaDon.KhoHang.Ten,
                            TenHangHoa = n.HangHoaHoaDon.TenHangHoa,
                            DonViTinh = n.HangHoaHoaDon.DonViTinh.Name,
                            SoLuong = n.HangHoaHoaDon.SoLuong,
                            DonGiaTruocThue = n.HangHoaHoaDon.DonGiaTruocThue,
                            DonGiaSauThue = n.HangHoaHoaDon.DonGiaSauThue,
                            TongGiaTruocThue = n.HangHoaHoaDon.TongGiaTruocThue,
                            TongGiaSauThue = n.HangHoaHoaDon.TongGiaSauThue,
                            NhomHangName = n.HangHoaHoaDon.NhomHang.Name,
                            // Image1 = n.HangHoaHoaDon.Image1
                        }
                )
                .ToListAsync();

            HoaDonNhap = new HoaDonNhapDetailViewModel
            {
                ID = hoaDon.Id,
                NhaCungCap = hoaDon.NhaCungCap,
                NgayNhap = hoaDon.NgayNhap,
                SoHoaDon = hoaDon.SoHoaDon,
                ThanhTien = hoaDon.ThanhTien,
                Serial = hoaDon.Serial,
                KhoHang = hoaDon.KhoHang.Ten, // Gán thông tin kho hàng
                HangHoas = hangHoas
            };
            HasPDF = hoaDon.PDFFile != null && hoaDon.PDFFile.Length > 0;
            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            if (hoaDon == null)
                return NotFound();

            var hangHoas = await dBContext.NhapKhos
                .Where(n => n.HoaDonNhapId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.DonViTinh)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.KhoHang)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.NhomHang)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu Nhập Kho");

                // General formatting
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 14;

                // Set row heights
                worksheet.Row(2).Height = 30;
                worksheet.Row(3).Height = 30;

                // Merge cells and add content
                worksheet.Cells["A2:B3"]
                    .Style
                    .Font
                    .Size = 13;
                worksheet.Cells["A2:B3"].Merge = true;
                worksheet.Cells["A2:B3"].Value =
                    $"Đơn vị: {hoaDon.KhoHang.Ten}{Environment.NewLine}Bộ phận: ..........";
                worksheet.Cells["A2:B3"].Style.WrapText = true;
                worksheet.Cells["A2:B3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Ô tiêu đề "PHIẾU XUẤT KHO"
                worksheet.Cells["C2:D3"].Merge = true;
                worksheet.Cells["C2:D3"].Style.WrapText = true;
                worksheet.Cells["C2:D3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["C2:D3"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["C2:D3"].Style.Font.Bold = true;

                var richText = worksheet.Cells["C2"].RichText;

                var titleText = richText.Add("PHIẾU NHẬP KHO");
                titleText.Size = 16;

                richText.Add("\n");
                var dateText = richText.Add(
                    $"Ngày {hoaDon.NgayNhap.Day} tháng {hoaDon.NgayNhap.Month} năm {hoaDon.NgayNhap.Year}"
                );
                dateText.Size = 13;
                dateText.Bold = false;

                worksheet.Cells["E2:H3"].Style.Font.Size = 13;
                worksheet.Cells["E2:H3"].Merge = true;
                worksheet.Cells["E2:H3"].Value =
                    "Mẫu số: 02 – VT"
                    + Environment.NewLine
                    + "(Ban hành theo Thông tư số: 200/2014/TT-BTC Ngày 22/12/2014 của Bộ Tài chính)";
                worksheet.Cells["E2:H3"].Style.WrapText = true;
                worksheet.Cells["E2:H3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["E2:H3"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Information section
                worksheet.Cells["B5"].Value = "- Họ và tên người giao hàng: Dương Mạnh Tuấn";
                worksheet.Cells["B5:D5"].Merge = true;
                worksheet.Cells["B6"].Value = "- Theo hóa đơn số: " + hoaDon.SoHoaDon;
                worksheet.Cells["B6:D6"].Merge = true;
                worksheet.Cells["B7"].Value = "- Nhập tại kho (ngăn lô): " + hoaDon.KhoHang.Ten;
                worksheet.Cells["B7:D7"].Merge = true;
                worksheet.Cells["E5"].Value = "- Địa chỉ bộ phận: " + hoaDon.KhoHang.DiaChi;
                worksheet.Cells["E5:H5"].Merge = true;
                worksheet.Cells["B5:F7"].Style.Font.Name = "Times New Roman";
                worksheet.Cells["B5:F7"].Style.Font.Size = 12;

                // Table header
                var headerCells = new[]
                {
                    ("A8:A9", "Stt"),
                    (
                        "B8:B9",
                        "Tên, nhãn hiệu, quy cách, phẩm chất vật tư, dụng cụ, sản phẩm, hàng hóa"
                    ),
                    ("C8:C9", "Mã số"),
                    ("D8:D9", "Đơn vị tính"),
                    ("E8:F8", "Số lượng"),
                    ("G8:G9", "Đơn giá"),
                    ("H8:H9", "Thành tiền")
                };

                foreach (var (range, value) in headerCells)
                {
                    worksheet.Cells[range].Merge = true;
                    worksheet.Cells[range].Value = value;
                }

                worksheet.Cells["E9"].Value = "Yêu cầu";
                worksheet.Cells["F9"].Value = "Thực nhập";

                // Header formatting
                var headerRange = worksheet.Cells["A8:H9"];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.WrapText = true;
                // headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Add borders to all cells in the header
                foreach (var cell in headerRange)
                {
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Add thicker border for the outer edge of the header
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Add thicker border between "Số lượng" and its sub-headers
                worksheet.Cells["E9:F9"]
                    .Style
                    .Border
                    .Top
                    .Style = ExcelBorderStyle.Thin;

                // Set column widths
                var columnWidths = new Dictionary<int, double>
                {
                    { 1, 5 },
                    { 2, 35 },
                    { 3, 25 },
                    { 4, 20 },
                    { 5, 12 },
                    { 6, 12 },
                    { 7, 20 },
                    { 8, 20 }
                };
                foreach (var (col, width) in columnWidths)
                {
                    worksheet.Column(col).Width = width;
                }

                // Fill in data
                int startRow = 10;
                for (int i = 0; i < hangHoas.Count; i++)
                {
                    var item = hangHoas[i];
                    worksheet.Cells[startRow + i, 1].Value = i + 1; // Stt
                    worksheet.Cells[startRow + i, 2].Value = item.HangHoaHoaDon.TenHangHoa;
                    worksheet.Cells[startRow + i, 3].Value = "";
                    worksheet.Cells[startRow + i, 4].Value = item.HangHoaHoaDon.DonViTinh.Name;
                    worksheet.Cells[startRow + i, 5].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 6].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 7].Value =
                        item.HangHoaHoaDon.DonGiaTruocThue.ToString("N0", new CultureInfo("vi-VN"));
                    worksheet.Cells[startRow + i, 8].Value =
                        item.HangHoaHoaDon.TongGiaTruocThue.ToString(
                            "N0",
                            new CultureInfo("vi-VN")
                        );

                    // Định dạng border cho mỗi ô
                    for (int j = 1; j <= 8; j++)
                    {
                        worksheet.Cells[startRow + i, j].Style.Border.Top.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Left.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Right.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Bottom.Style =
                            ExcelBorderStyle.Thin;
                    }
                    // Căn giữa các cột Số lượng, Đơn giá, Tổng giá
                    worksheet.Cells[startRow + i, 1]
                        .Style
                        .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 4].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 5].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 6].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 7].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 8].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Right;
                }
                // Add thicker border for the outer edge of the data
                worksheet.Cells[
                    startRow,
                    1,
                    startRow + hangHoas.Count,
                    8
                ].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Tổng tiền
                int totalRow = startRow + hangHoas.Count + 1;
                worksheet.Cells[totalRow - 1, 2].Value = "Tổng tiền (Chưa có VAT):";
                worksheet.Cells[totalRow - 1, 2].Style.Font.Bold = true;
                worksheet.Cells[totalRow - 1, 8].Value = hoaDon.ThanhTien.ToString(
                    "N0",
                    new CultureInfo("vi-VN")
                );
                worksheet.Cells[totalRow - 1, 8].Style.Font.Bold = true;
                worksheet.Cells[totalRow - 1, 8].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Right;
                for (int i = 1; i <= 8; i++)
                {
                    worksheet.Cells[totalRow - 1, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Left.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Right.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                }

                // Tổng số tiền bằng chữ
                worksheet.Cells[totalRow, 2, totalRow, 8].Merge = true;
                worksheet.Cells[totalRow, 2].Value =
                    $"- Tổng số tiền (viết bằng chữ): {CapitalizeFirstLetter(NumberToWords((long)hoaDon.ThanhTien))} đồng.";
                worksheet.Cells[totalRow, 2].Style.WrapText = true;

                worksheet.Cells[totalRow + 1, 2, totalRow + 1, 8].Merge = true;
                worksheet.Cells[totalRow + 1, 2].Value =
                    "- Số chứng từ gốc kèm theo: hóa đơn số " + hoaDon.SoHoaDon;
                worksheet.Cells[totalRow + 1, 2].Style.WrapText = true;

                // Ngày tháng năm
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Merge = true;
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Value =
                    $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Footer
                string[] footerTitles =
                {
                    "Người lập phiếu",
                    "Người nhận hàng",
                    "Thủ kho",
                    "Kế toán",
                    "Giám đốc"
                };

                string[] signers =
                {
                    "Dương Mạnh Tuấn",
                    "Dương Mạnh Tuấn",
                    "Dương Mạnh Tuấn",
                    "Nguyễn Thị Hảo",
                    "Đỗ Công Biên"
                };

                int currentColumn = 1;
                int footerTitleRow = totalRow + 5;
                int signerRow = footerTitleRow + 6; // 5 hàng trống giữa footerTitles và chữ ký

                for (int i = 0; i < footerTitles.Length; i++)
                {
                    int columnSpan =
                        (footerTitles[i] == "Người nhận hàng" || footerTitles[i] == "Thủ kho")
                            ? 1
                            : 2;

                    // Tiêu đề chữ ký
                    var titleRange = worksheet.Cells[
                        footerTitleRow,
                        currentColumn,
                        footerTitleRow,
                        currentColumn + columnSpan - 1
                    ];
                    titleRange.Merge = true;
                    titleRange.Value = footerTitles[i];
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    titleRange.Style.Font.Bold = true;

                    // Tên người ký
                    var signerRange = worksheet.Cells[
                        signerRow,
                        currentColumn,
                        signerRow,
                        currentColumn + columnSpan - 1
                    ];
                    signerRange.Merge = true;
                    signerRange.Value = signers[i];
                    signerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Cập nhật cột hiện tại cho lần lặp tiếp theo
                    currentColumn += columnSpan;
                }

                // Xuất file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"PhieuNhapKho_{hoaDon.SoHoaDon}.xlsx";
                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    excelName
                );
            }
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        static string NumberToWords(long number)
        {
            if (number == 0)
                return "không";

            if (number < 0)
                return "âm " + NumberToWords(Math.Abs(number));

            string[] unitsMap =
            {
                "không",
                "một",
                "hai",
                "ba",
                "bốn",
                "năm",
                "sáu",
                "bảy",
                "tám",
                "chín"
            };
            string[] tensMap =
            {
                "không",
                "mười",
                "hai mươi",
                "ba mươi",
                "bốn mươi",
                "năm mươi",
                "sáu mươi",
                "bảy mươi",
                "tám mươi",
                "chín mươi"
            };

            string words = "";

            if ((number / 1_000_000_000) > 0)
            {
                words += NumberToWords(number / 1_000_000_000) + " tỷ ";
                number %= 1_000_000_000;
            }

            if ((number / 1_000_000) > 0)
            {
                words += NumberToWords(number / 1_000_000) + " triệu ";
                number %= 1_000_000;
            }

            if ((number / 1_000) > 0)
            {
                long remainder = number % 1_000;
                if (remainder == 0)
                {
                    words += NumberToWords(number / 1_000) + " nghìn ";
                }
                else
                {
                    long lastDigit = remainder % 10;
                    words += NumberToWords(number / 1_000) + " nghìn ";
                    if (lastDigit == 1 && remainder / 10 % 10 != 1)
                    {
                        words += "mốt "; // Sử dụng "mốt" thay vì "một" trong trường hợp đặc biệt
                    }
                }
                number %= 1_000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " trăm ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "";

                if (number < 10)
                    words += unitsMap[number];
                else if (number < 20)
                    words += "mười " + unitsMap[number % 10];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        if (number % 10 == 1)
                            words += " mốt"; // Xử lý trường hợp đặc biệt khi số cuối là "mốt"
                        else
                            words += " " + unitsMap[number % 10];
                    }
                }
            }

            return words.Trim();
        }

        public async Task<IActionResult> OnGetDownloadPDFAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            if (hoaDon == null || hoaDon.PDFFile == null || hoaDon.PDFFile.Length == 0)
            {
                return NotFound();
            }

            return File(hoaDon.PDFFile, "application/pdf", $"HoaDon_{hoaDon.SoHoaDon}.pdf");
        }

        public async Task<IActionResult> OnGetDisplayPDFAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            if (hoaDon == null || hoaDon.PDFFile == null || hoaDon.PDFFile.Length == 0)
            {
                return NotFound();
            }

            return File(hoaDon.PDFFile, "application/pdf");
        }

        public async Task<IActionResult> OnGetViewExcelAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            if (hoaDon == null)
                return NotFound();

            var hangHoas = await dBContext.NhapKhos
                .Where(n => n.HoaDonNhapId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.DonViTinh)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.KhoHang)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(h => h.NhomHang)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu Nhập Kho");

                // General formatting
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 14;

                // Set row heights
                worksheet.Row(2).Height = 30;
                worksheet.Row(3).Height = 30;

                // Merge cells and add content
                worksheet.Cells["A2:B3"]
                    .Style
                    .Font
                    .Size = 13;
                worksheet.Cells["A2:B3"].Merge = true;
                worksheet.Cells["A2:B3"].Value =
                    $"Đơn vị: {hoaDon.KhoHang.Ten}{Environment.NewLine}Bộ phận: ..........";
                worksheet.Cells["A2:B3"].Style.WrapText = true;
                worksheet.Cells["A2:B3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Ô tiêu đề "PHIẾU XUẤT KHO"
                worksheet.Cells["C2:D3"].Merge = true;
                worksheet.Cells["C2:D3"].Style.WrapText = true;
                worksheet.Cells["C2:D3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["C2:D3"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["C2:D3"].Style.Font.Bold = true;

                var richText = worksheet.Cells["C2"].RichText;

                var titleText = richText.Add("PHIẾU NHẬP KHO");
                titleText.Size = 16;

                richText.Add("\n");
                var dateText = richText.Add(
                    $"Ngày {hoaDon.NgayNhap.Day} tháng {hoaDon.NgayNhap.Month} năm {hoaDon.NgayNhap.Year}"
                );
                dateText.Size = 13;
                dateText.Bold = false;

                worksheet.Cells["E2:H3"].Style.Font.Size = 13;
                worksheet.Cells["E2:H3"].Merge = true;
                worksheet.Cells["E2:H3"].Value =
                    "Mẫu số: 02 – VT"
                    + Environment.NewLine
                    + "(Ban hành theo Thông tư số: 200/2014/TT-BTC Ngày 22/12/2014 của Bộ Tài chính)";
                worksheet.Cells["E2:H3"].Style.WrapText = true;
                worksheet.Cells["E2:H3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["E2:H3"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Information section
                worksheet.Cells["B5"].Value = "- Họ và tên người giao hàng: Dương Mạnh Tuấn";
                worksheet.Cells["B5:D5"].Merge = true;
                worksheet.Cells["B6"].Value = "- Theo hóa đơn số: " + hoaDon.SoHoaDon;
                worksheet.Cells["B6:D6"].Merge = true;
                worksheet.Cells["B7"].Value = "- Nhập tại kho (ngăn lô): " + hoaDon.KhoHang.Ten;
                worksheet.Cells["B7:D7"].Merge = true;
                worksheet.Cells["E5"].Value = "- Địa chỉ bộ phận: " + hoaDon.KhoHang.DiaChi;
                worksheet.Cells["E5:H5"].Merge = true;
                worksheet.Cells["B5:F7"].Style.Font.Name = "Times New Roman";
                worksheet.Cells["B5:F7"].Style.Font.Size = 12;

                // Table header
                var headerCells = new[]
                {
                    ("A8:A9", "Stt"),
                    (
                        "B8:B9",
                        "Tên, nhãn hiệu, quy cách, phẩm chất vật tư, dụng cụ, sản phẩm, hàng hóa"
                    ),
                    ("C8:C9", "Mã số"),
                    ("D8:D9", "Đơn vị tính"),
                    ("E8:F8", "Số lượng"),
                    ("G8:G9", "Đơn giá"),
                    ("H8:H9", "Thành tiền")
                };

                foreach (var (range, value) in headerCells)
                {
                    worksheet.Cells[range].Merge = true;
                    worksheet.Cells[range].Value = value;
                }

                worksheet.Cells["E9"].Value = "Yêu cầu";
                worksheet.Cells["F9"].Value = "Thực nhập";

                // Header formatting
                var headerRange = worksheet.Cells["A8:H9"];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.WrapText = true;
                // headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Add borders to all cells in the header
                foreach (var cell in headerRange)
                {
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Add thicker border for the outer edge of the header
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Add thicker border between "Số lượng" and its sub-headers
                worksheet.Cells["E9:F9"]
                    .Style
                    .Border
                    .Top
                    .Style = ExcelBorderStyle.Thin;

                // Set column widths
                var columnWidths = new Dictionary<int, double>
                {
                    { 1, 5 },
                    { 2, 35 },
                    { 3, 25 },
                    { 4, 20 },
                    { 5, 12 },
                    { 6, 12 },
                    { 7, 20 },
                    { 8, 20 }
                };
                foreach (var (col, width) in columnWidths)
                {
                    worksheet.Column(col).Width = width;
                }

                // Fill in data
                int startRow = 10;
                for (int i = 0; i < hangHoas.Count; i++)
                {
                    var item = hangHoas[i];
                    worksheet.Cells[startRow + i, 1].Value = i + 1; // Stt
                    worksheet.Cells[startRow + i, 2].Value = item.HangHoaHoaDon.TenHangHoa;
                    worksheet.Cells[startRow + i, 3].Value = "";
                    worksheet.Cells[startRow + i, 4].Value = item.HangHoaHoaDon.DonViTinh.Name;
                    worksheet.Cells[startRow + i, 5].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 6].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 7].Value =
                        item.HangHoaHoaDon.DonGiaTruocThue.ToString("N0", new CultureInfo("vi-VN"));
                    worksheet.Cells[startRow + i, 8].Value =
                        item.HangHoaHoaDon.TongGiaTruocThue.ToString(
                            "N0",
                            new CultureInfo("vi-VN")
                        );

                    // Định dạng border cho mỗi ô
                    for (int j = 1; j <= 8; j++)
                    {
                        worksheet.Cells[startRow + i, j].Style.Border.Top.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Left.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Right.Style =
                            ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i, j].Style.Border.Bottom.Style =
                            ExcelBorderStyle.Thin;
                    }
                    // Căn giữa các cột Số lượng, Đơn giá, Tổng giá
                    worksheet.Cells[startRow + i, 1]
                        .Style
                        .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 4].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 5].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 6].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 7].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow + i, 8].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Right;
                }
                // Add thicker border for the outer edge of the data
                worksheet.Cells[
                    startRow,
                    1,
                    startRow + hangHoas.Count,
                    8
                ].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Tổng tiền
                int totalRow = startRow + hangHoas.Count + 1;
                worksheet.Cells[totalRow - 1, 2].Value = "Tổng tiền (Chưa có VAT):";
                worksheet.Cells[totalRow - 1, 2].Style.Font.Bold = true;
                worksheet.Cells[totalRow - 1, 8].Value = hoaDon.ThanhTien.ToString(
                    "N0",
                    new CultureInfo("vi-VN")
                );
                worksheet.Cells[totalRow - 1, 8].Style.Font.Bold = true;
                worksheet.Cells[totalRow - 1, 8].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Right;
                for (int i = 1; i <= 8; i++)
                {
                    worksheet.Cells[totalRow - 1, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Left.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Right.Style =
                        ExcelBorderStyle.Thin;
                    worksheet.Cells[totalRow - 1, i].Style.Border.Bottom.Style =
                        ExcelBorderStyle.Thin;
                }

                // Tổng số tiền bằng chữ
                worksheet.Cells[totalRow, 2, totalRow, 8].Merge = true;
                worksheet.Cells[totalRow, 2].Value =
                    $"- Tổng số tiền (viết bằng chữ): {CapitalizeFirstLetter(NumberToWords((long)hoaDon.ThanhTien))} đồng.";
                worksheet.Cells[totalRow, 2].Style.WrapText = true;

                worksheet.Cells[totalRow + 1, 2, totalRow + 1, 8].Merge = true;
                worksheet.Cells[totalRow + 1, 2].Value =
                    "- Số chứng từ gốc kèm theo: hóa đơn số " + hoaDon.SoHoaDon;
                worksheet.Cells[totalRow + 1, 2].Style.WrapText = true;

                // Ngày tháng năm
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Merge = true;
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Value =
                    $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                worksheet.Cells[totalRow + 3, 5, totalRow + 3, 8].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Footer
                string[] footerTitles =
                {
                    "Người lập phiếu",
                    "Người nhận hàng",
                    "Thủ kho",
                    "Kế toán",
                    "Giám đốc"
                };

                string[] signers =
                {
                    "Dương Mạnh Tuấn",
                    "Dương Mạnh Tuấn",
                    "Dương Mạnh Tuấn",
                    "Nguyễn Thị Hảo",
                    "Đỗ Công Biên"
                };

                int currentColumn = 1;
                int footerTitleRow = totalRow + 5;
                int signerRow = footerTitleRow + 6; // 5 hàng trống giữa footerTitles và chữ ký

                for (int i = 0; i < footerTitles.Length; i++)
                {
                    int columnSpan =
                        (footerTitles[i] == "Người nhận hàng" || footerTitles[i] == "Thủ kho")
                            ? 1
                            : 2;

                    // Tiêu đề chữ ký
                    var titleRange = worksheet.Cells[
                        footerTitleRow,
                        currentColumn,
                        footerTitleRow,
                        currentColumn + columnSpan - 1
                    ];
                    titleRange.Merge = true;
                    titleRange.Value = footerTitles[i];
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    titleRange.Style.Font.Bold = true;

                    // Tên người ký
                    var signerRange = worksheet.Cells[
                        signerRow,
                        currentColumn,
                        signerRow,
                        currentColumn + columnSpan - 1
                    ];
                    signerRange.Merge = true;
                    signerRange.Value = signers[i];
                    signerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Cập nhật cột hiện tại cho lần lặp tiếp theo
                    currentColumn += columnSpan;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"HoaDonNhap_{id}.xlsx"
                );
            }
        }
    }
}
