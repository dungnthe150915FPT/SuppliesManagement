using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SuppliesManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SuppliesManagement.Pages.Reports
{
    public class ViewExcelCongCuDungCuModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ViewExcelCongCuDungCuModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult OnGet(int year, int month)
        {
            if (year <= 0 || month <= 0 || month > 12)
            {
                return BadRequest("Năm không hợp lệ.");
            }

            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var hangHoas = _dbContext.HangHoas
                .Include(h => h.DonViTinh)
                .Where(
                    h => h.NgayNhap >= startOfMonth && h.NgayNhap <= endOfMonth && h.NhomHangId == 2
                )
                .ToList();

            if (!hangHoas.Any())
            {
                return Content(
                    "<h3 style='color:red;text-align:center;'>Không có dữ liệu vật tư cho tháng này</h3>",
                    "text/html"
                );
            }

            // Tạo file Excel
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Vật Tư {month}/{year}");

            // Tiêu đề
            worksheet.Cells["A1"].Value = $"SỔ THEO DÕI VẬT TƯ - Tháng {month}/{year}";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml
                .Style
                .ExcelHorizontalAlignment
                .Center;
            worksheet.Cells["A1:C1"].Merge = true;

            // Header
            string[] headers = { "STT", "Tên Vật Tư", "Số lượng" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.HorizontalAlignment = OfficeOpenXml
                    .Style
                    .ExcelHorizontalAlignment
                    .Center;
            }

            // Điền dữ liệu
            int rowIndex = 4;
            int stt = 1;
            foreach (var hangHoa in hangHoas)
            {
                worksheet.Cells[rowIndex, 1].Value = stt++;
                worksheet.Cells[rowIndex, 2].Value = hangHoa.TenHangHoa;
                worksheet.Cells[rowIndex, 3].Value = hangHoa.SoLuong;
                worksheet.Cells[rowIndex, 3].Style.HorizontalAlignment = OfficeOpenXml
                    .Style
                    .ExcelHorizontalAlignment
                    .Center;
                rowIndex++;
            }

            // Chuyển đổi Excel thành HTML
            var htmlContent = ConvertExcelToHtml(package);
            return Content(htmlContent, "text/html");
        }

        private string ConvertExcelToHtml(ExcelPackage package)
        {
            var sb = new StringBuilder();
            var worksheet = package.Workbook.Worksheets[0];

            sb.Append("<style>");
            sb.Append(
                "table { width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; }"
            );
            sb.Append("th, td { border: 1px solid black; padding: 8px; text-align: center; }");
            sb.Append("th { background-color: #f2f2f2; font-weight: bold; }");
            sb.Append("</style>");

            sb.Append("<table>");
            sb.Append("<tr>");
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                sb.Append($"<th>{worksheet.Cells[3, col].Text}</th>");
            }
            sb.Append("</tr>");

            for (int row = 4; row <= worksheet.Dimension.End.Row; row++)
            {
                sb.Append("<tr>");
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    sb.Append($"<td>{worksheet.Cells[row, col].Text}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
