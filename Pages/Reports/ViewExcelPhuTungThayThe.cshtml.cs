using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SuppliesManagement.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SuppliesManagement.Pages.Reports
{
    public class ViewExcelPhuTungThayTheModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ViewExcelPhuTungThayTheModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int Year { get; set; }
        public string ExcelHtml { get; set; } = string.Empty;

        public IActionResult OnGet(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Năm không hợp lệ.");
            }

            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31);
            var currentDate = DateTime.Now;

            var khoHang = _dbContext.KhoHangs.FirstOrDefault();
            var hangHoas = _dbContext.HangHoas
                .Include(h => h.DonViTinh)
                .Where(
                    h => h.NgayNhap >= startOfYear && h.NgayNhap <= endOfYear && h.NhomHangId == 3
                )
                .ToList();

            if (!hangHoas.Any())
            {
                return Content(
                    "<h3 style='color:red;text-align:center;'>Không có dữ liệu Phụ Tùng Thay Thế cho năm này</h3>",
                    "text/html"
                );
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Phụ Tùng Thay Thế {year}");
            GenerateExcelContent(worksheet, khoHang, hangHoas, year, currentDate);
            ExcelHtml = ConvertExcelToHtml(worksheet, khoHang, year, currentDate);
            Year = year;
            return Page();
        }

        private void GenerateExcelContent(
            ExcelWorksheet worksheet,
            KhoHang khoHang,
            List<HangHoa> hangHoas,
            int year,
            DateTime currentDate
        )
        {
            worksheet.Cells.Style.Font.Name = "Times New Roman";
            worksheet.Cells.Style.Font.Size = 14;

            // Header Information
            worksheet.Cells["A1:K1"].Merge = true;
            worksheet.Cells["A1:K1"].Style.Font.Bold = true;
            worksheet.Cells["A1:K2"].Style.Font.Size = 13;
            var a1Text = worksheet.Cells["A1"].RichText;
            var khohang = a1Text.Add("Đơn vị: ");
            khohang.Bold = true;
            var tenkhohang = a1Text.Add(khoHang?.Ten);
            worksheet.Cells["A2:K2"].Merge = true;
            worksheet.Cells["A2:K3"].Style.Font.Bold = true;
            var a2Text = worksheet.Cells["A2"].RichText;
            var diachi = a2Text.Add("Địa chỉ: ");
            diachi.Bold = true;
            var tendiachi = a2Text.Add(khoHang?.DiaChi);
            tendiachi.Bold = false;
            tenkhohang.Bold = false;

            worksheet.Cells["A3:K4"].Merge = true;
            worksheet.Cells["A3:K4"].Style.WrapText = true; // Enable text wrapping
            var a3Text = worksheet.Cells["A3"].RichText;
            var tieude = a3Text.Add("SỔ THEO DÕI Phụ Tùng Thay Thế");
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
            worksheet.Cells["B6"].Value = "Tên CCDC";
            worksheet.Cells["C6"].Value = "Mã CCDC";
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
            worksheet.Cells["F6:G6"].Value = "Ghi tăng CCDC";
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
            worksheet.Cells["H6:J6"].Value = "Ghi giảm CCDC";
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

            int rowIndex = 9;
            int stt = 1;
            foreach (var hangHoa in hangHoas)
            {
                worksheet.Cells[rowIndex, 1].Value = stt++;
                worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa;
                worksheet.Cells[rowIndex, 5].Value = hangHoa.DonViTinh?.Name;
                worksheet.Cells[rowIndex, 7].Value = hangHoa.NgayNhap.ToString("dd/MM/yyyy");
                for (int col = 1; col <= 11; col++)
                {
                    worksheet.Cells[rowIndex, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                rowIndex++;
            }
        }

        private string ConvertExcelToHtml(
            ExcelWorksheet worksheet,
            KhoHang khoHang,
            int year,
            DateTime currentDate
        )
        {
            var sb = new StringBuilder();
            sb.Append("<style>");
            sb.Append("table { border-collapse: collapse; width: 100%; }");
            sb.Append("th, td { border: 1px solid black; padding: 5px; text-align: center; }");
            sb.Append("th { background-color: #f2f2f2; }");
            sb.Append(
                ".header { font-size: 18px; font-weight: bold; text-align: center; margin-bottom: 10px; }"
            );
            sb.Append(".subheader { font-size: 14px; margin-bottom: 5px; }");
            sb.Append("</style>");
            sb.Append("<style>");
            sb.Append("table { border-collapse: collapse; width: 100%; }");
            sb.Append("th, td { border: 1px solid black; padding: 5px; text-align: center; }");
            sb.Append("th { background-color: #f2f2f2; }");
            sb.Append(
                ".header { font-size: 18px; font-weight: bold; text-align: center; margin-bottom: 10px; }"
            );
            sb.Append(".subheader { font-size: 14px; margin-bottom: 5px; }");
            sb.Append(".subheader strong { font-weight: bold; }");
            sb.Append("</style>");
            sb.Append($"<div class='subheader'><strong>Đơn vị:</strong> {khoHang?.Ten}</div>");
            sb.Append($"<div class='subheader'><strong>Địa chỉ:</strong> {khoHang?.DiaChi}</div>");
            sb.Append($"<div class='header'>SỔ THEO DÕI PHỤ TÙNG THAY THẾ</div>");
            sb.Append($"<div class='header'>Năm {year}</div>");
            sb.Append("<table>");

            // Generate header rows
            for (int row = 6; row <= 8; row++)
            {
                sb.Append("<tr>");
                for (int col = 1; col <= 11; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    int rowspan = 1;
                    int colspan = 1;

                    var mergedRange = worksheet.MergedCells[row, col];
                    if (mergedRange != null)
                    {
                        var mergedAddress = new ExcelAddress(mergedRange);
                        rowspan = mergedAddress.End.Row - mergedAddress.Start.Row + 1;
                        colspan = mergedAddress.End.Column - mergedAddress.Start.Column + 1;

                        if (
                            cell.Start.Row == mergedAddress.Start.Row
                            && cell.Start.Column == mergedAddress.Start.Column
                        )
                        {
                            sb.Append(
                                $"<th rowspan='{rowspan}' colspan='{colspan}'>{cell.Text}</th>"
                            );
                        }
                    }
                    else
                    {
                        sb.Append($"<th>{cell.Text}</th>");
                    }
                }
                sb.Append("</tr>");
            }

            // Generate data rows
            for (int row = 9; row <= worksheet.Dimension.End.Row; row++)
            {
                sb.Append("<tr>");
                for (int col = 1; col <= 11; col++)
                {
                    sb.Append($"<td>{worksheet.Cells[row, col].Text}</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            // Add footer
            sb.Append("<div style='margin-top: 20px; text-align: right; margin-right: 10%;'>");
            sb.Append($"Ngày {currentDate.Day} tháng {currentDate.Month} năm {currentDate.Year}");
            sb.Append("</div>");

            sb.Append("<table style='width: 100%; border: none; margin-top: 20px;'>");
            sb.Append("<tr>");
            sb.Append(
                "<td style='width: 33%; text-align: center; vertical-align: top; border: none;'>"
            );
            sb.Append(
                "<strong>Người ghi sổ</strong><br>(Ký, họ tên)<br><br><br><br><br><br><br><br>Dương Mạnh Tuấn"
            );
            sb.Append("</td>");
            sb.Append(
                "<td style='width: 33%; text-align: center; vertical-align: top; border: none;'>"
            );
            sb.Append(
                "<strong>Phụ trách kế toán</strong><br>(Ký, họ tên)<br><br><br><br><br><br><br><br>Nguyễn Thị Hảo"
            );
            sb.Append("</td>");
            sb.Append(
                "<td style='width: 33%; text-align: center; vertical-align: top; border: none;'>"
            );
            sb.Append(
                "<strong>Giám đốc Đài TTXLTTHH Hà Nội</strong><br>(Ký, họ tên, đóng dấu)<br><br><br><br><br><br><br><br>Đỗ Công Biên"
            );
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
