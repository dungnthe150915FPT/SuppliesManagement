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

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            var hangHoas = await dBContext.NhapKhos
                .Where(n => n.HoaDonNhapId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(n => n.KhoHang)
                .Select(n => new HangHoaViewModel
                {
                    TenHangHoa = n.HangHoaHoaDon.TenHangHoa,
                    DonViTinh = n.HangHoaHoaDon.DonViTinh.Name,
                    SoLuong = n.HangHoaHoaDon.SoLuong,
                    DonGiaTruocThue = n.HangHoaHoaDon.DonGiaTruocThue,
                    DonGiaSauThue = n.HangHoaHoaDon.DonGiaSauThue,
                    TongGiaTruocThue = n.HangHoaHoaDon.TongGiaTruocThue,
                    TongGiaSauThue = n.HangHoaHoaDon.TongGiaSauThue
                })
                .ToListAsync();
            HoaDonNhap = new HoaDonNhapDetailViewModel
            {
                ID = hoaDon.Id,
                NhaCungCap = hoaDon.NhaCungCap,
                /*                KhoHang = hoaDon.KhoHang.TenKho,*/
                NgayNhap = hoaDon.NgayNhap,
                SoHoaDon = hoaDon.SoHoaDon,
                ThanhTien = hoaDon.ThanhTien,
                Serial = hoaDon.Serial,
                HangHoas = hangHoas
            };
            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonNhaps.FindAsync(id);
            if (hoaDon == null) return NotFound();

            var hangHoas = await dBContext.NhapKhos
                .Where(n => n.HoaDonNhapId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(n => n.KhoHang)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu Nhập Kho");

                // Định dạng chung
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 12;

                // Merge và định dạng tiêu đề
                worksheet.Cells["C2:H2"].Merge = true;
                worksheet.Cells["C2:H2"].Value = "PHIẾU NHẬP KHO";
                worksheet.Cells["C2:H2"].Style.Font.Bold = true;
                worksheet.Cells["C2:H2"].Style.Font.Size = 16;
                worksheet.Cells["C2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["C3:H3"].Merge = true;
                worksheet.Cells["C3:H3"].Value = $"Ngày {hoaDon.NgayNhap.Day} tháng {hoaDon.NgayNhap.Month} năm {hoaDon.NgayNhap.Year}";
                worksheet.Cells["C3:H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Định dạng phần thông tin
                worksheet.Cells["B5"].Value = "- Bên cung cấp:";
                worksheet.Cells["B5:C5"].Merge = true;
                worksheet.Cells["D5:H5"].Merge = true;
                worksheet.Cells["D5"].Value = hoaDon.NhaCungCap;

                worksheet.Cells["B6"].Value = "- Theo hóa đơn số: ";
                worksheet.Cells["D6:H6"].Merge = true;
                worksheet.Cells["D6"].Value = hoaDon.SoHoaDon;

                worksheet.Cells["B7"].Value = "- Nhập vào kho (ngăn lô):";
                worksheet.Cells["D7:H7"].Merge = true;
                worksheet.Cells["D7"].Value = hoaDon.KhoHang.Ten;

                // Tạo bảng header
                worksheet.Cells["B10"].Value = "Stt";
                worksheet.Cells["C10"].Value = "Tên, nhãn hiệu, quy cách, phẩm chất vật tư, \ndụng cụ, sản phẩm, hàng hóa";
                worksheet.Cells["D10"].Value = "Mã số";
                worksheet.Cells["E10"].Value = "Đơn vị tính";
                worksheet.Cells["F10"].Value = "Số lượng yêu cầu";
                worksheet.Cells["G10"].Value = "Số lượng thực nhập";
                worksheet.Cells["H10"].Value = "Đơn giá";
                worksheet.Cells["I10"].Value = "Thành tiền";

                // Định dạng header
                worksheet.Cells["B10:I10"].Style.Font.Bold = true;
                worksheet.Cells["B10:I10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B10:I10"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B10:I10"].Style.WrapText = true; // Cho phép xuống dòng
                worksheet.Cells["B10:I10"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B10:I10"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells["B10:I10"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Đặt chiều cao hàng để hiển thị đủ nội dung
                worksheet.Row(10).Height = 30; // Tăng chiều cao để chứa 2 dòng


                // Điền dữ liệu hàng hóa
                int startRow = 11;
                for (int i = 0; i < hangHoas.Count; i++)
                {
                    var item = hangHoas[i];
                    worksheet.Cells[startRow + i, 2].Value = i + 1; // Stt
                    worksheet.Cells[startRow + i, 3].Value = item.HangHoaHoaDon.TenHangHoa;
                    worksheet.Cells[startRow + i, 5].Value = item.HangHoaHoaDon.DonViTinhId;
                    worksheet.Cells[startRow + i, 6].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 7].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 8].Value = item.HangHoaHoaDon.DonGiaTruocThue;
                    worksheet.Cells[startRow + i, 9].Value = item.HangHoaHoaDon.TongGiaTruocThue;

                    // Định dạng border cho mỗi ô
                    worksheet.Cells[startRow + i, 2, startRow + i, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Định dạng cột rộng
                worksheet.Column(2).Width = 5;  // Stt
                worksheet.Column(3).Width = 40; // Tên hàng hóa
                worksheet.Column(4).Width = 15; // Mã số
                worksheet.Column(5).Width = 12; // Đơn vị tính
                worksheet.Column(6).Width = 15; // Số lượng yêu cầu
                worksheet.Column(7).Width = 15; // Số lượng thực nhập
                worksheet.Column(8).Width = 15; // Đơn giá
                worksheet.Column(9).Width = 15; // Thành tiền
                worksheet.Column(10).Width = 15; // Serial

                // Tổng tiền
                worksheet.Cells[startRow + hangHoas.Count + 2, 8].Value = "Cộng:";
                worksheet.Cells[startRow + hangHoas.Count + 2, 9].Value = hoaDon.ThanhTien;
                worksheet.Cells[startRow + hangHoas.Count + 2, 9].Style.Font.Bold = true;

                worksheet.Cells[startRow + hangHoas.Count + 3, 2].Value = $"- Tổng số tiền (viết bằng chữ): {NumberToWords((long)hoaDon.ThanhTien)} đồng.";

                // Xuất file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"PhieuNhapKho_{hoaDon.SoHoaDon}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }

            /*using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu Nhập Kho");

                // Định dạng chung
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 12;

                // Đơn vị và bộ phận
                worksheet.Cells["B2:C2"].Merge = true;
                worksheet.Cells["B2:C2"].Value = "Đơn vị: Đài TXLTTH Hà Nội";
                worksheet.Cells["B3:C3"].Merge = true;
                worksheet.Cells["B3:C3"].Value = "Bộ phận: ………………..";

                // Tiêu đề phiếu nhập kho
                worksheet.Cells["E2:H2"].Merge = true;
                worksheet.Cells["E2:H2"].Value = "PHIẾU NHẬP KHO";
                worksheet.Cells["E2:H2"].Style.Font.Bold = true;
                worksheet.Cells["E2:H2"].Style.Font.Size = 16;
                worksheet.Cells["E2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Ngày tháng
                worksheet.Cells["E3:H3"].Merge = true;
                worksheet.Cells["E3:H3"].Value = "Ngày 6 tháng 9 năm 2024";
                worksheet.Cells["E3:H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Mẫu số
                worksheet.Cells["I2:J2"].Merge = true;
                worksheet.Cells["I2:J2"].Value = "Mẫu số: 01 – VT";
                worksheet.Cells["I3:J3"].Merge = true;
                worksheet.Cells["I3:J3"].Value = "(Ban hành theo Thông tư số: 200/2014/TT-BTC ngày 22/12/2014 của Bộ Tài chính)";

                // Địa chỉ bộ phận và địa điểm
                worksheet.Cells["I5:J5"].Merge = true;
                worksheet.Cells["I5:J5"].Value = "Địa chỉ bộ phận: số 34 ngõ 60 Dương Khuê";
                worksheet.Cells["I6:J6"].Merge = true;
                worksheet.Cells["I6:J6"].Value = "Địa điểm:";

                // Thông tin hóa đơn
                worksheet.Cells["B5:C5"].Merge = true;
                worksheet.Cells["B5:C5"].Value = "- Họ và tên người giao hàng: Dương Mạnh Tuấn";

                worksheet.Cells["B6:C6"].Merge = true;
                worksheet.Cells["B6:C6"].Value = "- Theo: hóa đơn số 00004582";

                worksheet.Cells["B7:C7"].Merge = true;
                worksheet.Cells["B7:C7"].Value = "- Nhập tại kho (ngăn lô): Đài TXLTTH Hà Nội";

                // Header bảng
                worksheet.Cells["B9"].Value = "Stt";
                worksheet.Cells["C9:G9"].Merge = true; // Tăng số cột được merge để tạo không gian cho tiêu đề
                worksheet.Cells["C9:G9"].Value = "Tên, nhãn hiệu, quy cách, phẩm chất vật tư, dụng cụ, sản phẩm, hàng hóa";
                worksheet.Cells["H9"].Value = "Mã số";
                worksheet.Cells["I9"].Value = "Đơn vị tính";
                worksheet.Cells["J9"].Value = "Số lượng yêu cầu";
                worksheet.Cells["K9"].Value = "Số lượng thực nhập";
                worksheet.Cells["L9"].Value = "Đơn giá";
                worksheet.Cells["M9"].Value = "Thành tiền";
                worksheet.Cells["N9"].Value = "Serial";

                // Định dạng header
                worksheet.Cells["B9:N9"].Style.Font.Bold = true;
                worksheet.Cells["B9:N9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B9:N9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B9:N9"].Style.WrapText = true; // Bật tính năng tự xuống dòng
                worksheet.Cells["B9:N9"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B9:N9"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells["B9:N9"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Điều chỉnh chiều rộng cột
                worksheet.Column(2).Width = 5;  // Stt
                worksheet.Column(3).Width = 50; // Tên, nhãn hiệu, quy cách, phẩm chất...
                worksheet.Column(8).Width = 10; // Mã số
                worksheet.Column(9).Width = 12; // Đơn vị tính
                worksheet.Column(10).Width = 15; // Số lượng yêu cầu
                worksheet.Column(11).Width = 15; // Số lượng thực nhập
                worksheet.Column(12).Width = 15; // Đơn giá
                worksheet.Column(13).Width = 15; // Thành tiền
                worksheet.Column(14).Width = 15; // Serial

                // Nội dung hàng hóa (ví dụ mẫu)
                worksheet.Cells["B10"].Value = 1;
                worksheet.Cells["C10:G10"].Merge = true; // Merge theo header để giữ định dạng
                worksheet.Cells["C10:G10"].Value = "Ổ cứng Seagate Ironwolf 4TB ST4000VN006 (3.5Inch/ 5400rpm/ 256MB/ SATA3/ Ổ NAS)";
                worksheet.Cells["H10"].Value = "Chiếc";
                worksheet.Cells["I10"].Value = 3;
                worksheet.Cells["J10"].Value = 3;
                worksheet.Cells["K10"].Value = "3,527,273";
                worksheet.Cells["L10"].Value = "10,581,818";

                // Điều chỉnh chiều cao tự động để chữ hiển thị rõ ràng
                worksheet.Row(9).Height = 30; // Header cao hơn để rõ nội dung
                worksheet.Cells.AutoFitColumns(); // Tự động điều chỉnh chiều rộng các cột


                // Cộng dòng tổng
                worksheet.Cells["H14"].Value = "Cộng:";
                worksheet.Cells["I14"].Value = "10,581,818";
                worksheet.Cells["I14"].Style.Font.Bold = true;

                // Dòng chú thích
                worksheet.Cells["B16:I16"].Merge = true;
                worksheet.Cells["B16:I16"].Value = "Đơn giá chưa bao gồm VAT";

                worksheet.Cells["B17:I17"].Merge = true;
                worksheet.Cells["B17:I17"].Value = "- Tổng số tiền (viết bằng chữ): Mười triệu năm trăm tám mươi mốt nghìn tám trăm mười tám đồng./.";

                worksheet.Cells["B18:I18"].Merge = true;
                worksheet.Cells["B18:I18"].Value = "- Số chứng từ gốc kèm theo: hóa đơn số 00004582";

                // Xuất file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"PhieuNhapKho_{hoaDon.SoHoaDon}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }*/

        }


        static string NumberToWords(long number)
        {
            if (number == 0)
                return "không";

            if (number < 0)
                return "âm " + NumberToWords(Math.Abs(number));

            string[] unitsMap = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] tensMap = { "không", "mười", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };

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
                words += NumberToWords(number / 1_000) + " nghìn ";
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
                    words += "và ";

                if (number < 10)
                    words += unitsMap[number];
                else if (number < 20)
                    words += "mười " + unitsMap[number % 10];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }
    }
}
