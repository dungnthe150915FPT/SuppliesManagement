using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;

public class ExcelExport
{
    public void ExportToExcel(string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Hóa Đơn Nhập");

            // Set title
            worksheet.Cells["B2"].Value = "Đơn vị: Đài TTXLTTHH Hà Nội\nBộ phận: ………………..";
            worksheet.Cells["B2"].Style.WrapText = true;

            worksheet.Cells["C2"].Value = "PHIẾU NHẬP KHO\nNgày 6 tháng 9 năm 2024";
            worksheet.Cells["C2"].Style.WrapText = true;
            worksheet.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C2"].Style.Font.Bold = true;

            worksheet.Cells["E2"].Value = "Mẫu số: 01 – VT\n( Ban hành theo Thông tư số: …)";
            worksheet.Cells["E2"].Style.WrapText = true;

            worksheet.Cells["F5"].Value = "Địa chỉ bộ phận: số 34 ngõ 60 Dương Khuê";

            // Merge cells as per template
            worksheet.Cells["B2:B3"].Merge = true;
            worksheet.Cells["C2:D3"].Merge = true;
            worksheet.Cells["E2:E3"].Merge = true;

            // Adjust column widths
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 40;
            worksheet.Column(5).Width = 25;

            // Save the file
            FileInfo file = new FileInfo(filePath);
            package.SaveAs(file);
        }
    }
}
