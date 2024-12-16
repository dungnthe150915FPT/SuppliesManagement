using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class ChiTietHoaDonXuatModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        public ChiTietHoaDonXuatModel(SuppliesManagementProjectContext dbContext)
        {
            dBContext = dbContext;
        }

        public HoaDonXuatDetailViewModel HoaDonXuat { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            var hoaDon = await dBContext.HoaDonXuats
                .Include(h => h.KhoHang)
                .Include(h => h.NguoiNhan)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            var hangHoas = await dBContext.XuatKhos
                .Where(x => x.HoaDonXuatId == id)
                .Include(x => x.HangHoaHoaDon)
                .ThenInclude(h => h.NhomHang)
                .Select(
                    x =>
                        new HangHoaViewModel
                        {
                            TenHangHoa = x.HangHoaHoaDon.TenHangHoa,
                            DonViTinh = x.HangHoaHoaDon.DonViTinh.Name,
                            SoLuong = x.HangHoaHoaDon.SoLuong,
                            DonGiaTruocThue = x.HangHoaHoaDon.DonGiaTruocThue,
                            DonGiaSauThue = x.HangHoaHoaDon.DonGiaSauThue,
                            TongGiaTruocThue = x.HangHoaHoaDon.TongGiaTruocThue,
                            TongGiaSauThue = x.HangHoaHoaDon.TongGiaSauThue,
                            NhomHangName = x.HangHoaHoaDon.NhomHang.Name
                        }
                )
                .ToListAsync();

            HoaDonXuat = new HoaDonXuatDetailViewModel
            {
                ID = hoaDon.Id,
                LyDoNhan = hoaDon.LyDoNhan,
                NgayNhan = hoaDon.NgayNhan,
                NguoiNhan = hoaDon.NguoiNhan.Username,
                ThanhTien = hoaDon.ThanhTien,
                KhoHang = hoaDon.KhoHang.Ten,
                HangHoas = hangHoas
            };

            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync(Guid id)
        {
            var hoaDon = await dBContext.HoaDonXuats
                .Include(h => h.NguoiNhan) // Bao gồm thông tin người nhận
                .Include(h => h.KhoHang) // Bao gồm thông tin kho hàng
                .FirstOrDefaultAsync(h => h.Id == id);
            if (hoaDon == null)
                return NotFound();

            var hangHoas = await dBContext.XuatKhos
                .Where(n => n.HoaDonXuatId == hoaDon.Id)
                .Include(n => n.HangHoaHoaDon)
                .ThenInclude(n => n.DonViTinh)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu Xuất Kho");

                // Định dạng chung
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 12;

                // Merge và định dạng tiêu đề
                worksheet.Cells["C2:H2"].Merge = true;
                worksheet.Cells["C2:H2"].Value = "PHIẾU XUẤT KHO";
                worksheet.Cells["C2:H2"].Style.Font.Bold = true;
                worksheet.Cells["C2:H2"].Style.Font.Size = 16;
                worksheet.Cells["C2:H2"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                worksheet.Cells["C3:H3"].Merge = true;
                worksheet.Cells["C3:H3"].Value =
                    $"Ngày {hoaDon.NgayNhan.Day} tháng {hoaDon.NgayNhan.Month} năm {hoaDon.NgayNhan.Year}";
                worksheet.Cells["C3:H3"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;

                // Định dạng phần thông tin
                worksheet.Cells["B5"].Value = "- Họ và tên người nhận hàng:";
                worksheet.Cells["B5:C5"].Merge = true;
                worksheet.Cells["D5:H5"].Merge = true;
                worksheet.Cells["D5"].Value = hoaDon.NguoiNhan.Username;

                worksheet.Cells["B6"].Value = "- Lý do nhận: ";
                worksheet.Cells["D6:H6"].Merge = true;
                worksheet.Cells["D6"].Value = hoaDon.LyDoNhan;

                worksheet.Cells["B7"].Value = "- Xuất tại kho (ngăn lô): ";
                worksheet.Cells["D7:H7"].Merge = true;
                worksheet.Cells["D7"].Value = hoaDon.KhoHang.Ten;

                // Tạo bảng header
                worksheet.Cells["B10"].Value = "Stt";
                worksheet.Cells["C10"].Value =
                    "Tên, nhãn hiệu, quy cách, phẩm chất vật tư, \ndụng cụ, sản phẩm, hàng hóa";
                worksheet.Cells["D10"].Value = "Mã số";
                worksheet.Cells["E10"].Value = "Đơn vị tính";
                worksheet.Cells["F10"].Value = "Số lượng yêu cầu";
                worksheet.Cells["G10"].Value = "Số lượng thực nhập";
                worksheet.Cells["H10"].Value = "Đơn giá";
                worksheet.Cells["I10"].Value = "Thành tiền";
                worksheet.Cells["J10"].Value = "Ghi chú";

                // Định dạng header
                worksheet.Cells["B10:J10"]
                    .Style
                    .Font
                    .Bold = true;
                worksheet.Cells["B10:J10"].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Center;
                worksheet.Cells["B10:J10"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B10:J10"].Style.WrapText = true; // Cho phép xuống dòng
                worksheet.Cells["B10:J10"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["B10:J10"].Style.Fill.BackgroundColor.SetColor(
                    System.Drawing.Color.LightGray
                );
                worksheet.Cells["B10:J10"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Đặt chiều cao hàng để hiển thị đủ nội dung
                worksheet.Row(10).Height = 30; // Tăng chiều cao để chứa 2 dòng

                // Điền dữ liệu hàng hóa
                int startRow = 11;
                for (int i = 0; i < hangHoas.Count; i++)
                {
                    var item = hangHoas[i];
                    worksheet.Cells[startRow + i, 2].Value = i + 1; // Stt
                    worksheet.Cells[startRow + i, 3].Value = item.HangHoaHoaDon.TenHangHoa;
                    worksheet.Cells[startRow + i, 5].Value = item.HangHoaHoaDon.DonViTinh.Name;
                    worksheet.Cells[startRow + i, 6].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 7].Value = item.HangHoaHoaDon.SoLuong;
                    worksheet.Cells[startRow + i, 8].Value = item.HangHoaHoaDon.DonGiaTruocThue;
                    worksheet.Cells[startRow + i, 9].Value = item.HangHoaHoaDon.TongGiaTruocThue;

                    // Định dạng border cho mỗi ô
                    worksheet.Cells[startRow + i, 2, startRow + i, 10].Style.Border.BorderAround(
                        ExcelBorderStyle.Thin
                    );
                }

                // Định dạng cột rộng
                worksheet.Column(2).Width = 5; // Stt
                worksheet.Column(3).Width = 40; // Tên hàng hóa
                worksheet.Column(4).Width = 15; // Mã số
                worksheet.Column(5).Width = 12; // Đơn vị tính
                worksheet.Column(6).Width = 15; // Số lượng yêu cầu
                worksheet.Column(7).Width = 15; // Số lượng thực nhập
                worksheet.Column(8).Width = 15; // Đơn giá
                worksheet.Column(9).Width = 15; // Thành tiền
                worksheet.Column(10).Width = 15; // Ghi chú

                // Tổng tiền
                worksheet.Cells[startRow + hangHoas.Count + 2, 8].Value =
                    "Tổng tiền (Chưa có VAT):";
                worksheet.Cells[startRow + hangHoas.Count + 2, 9].Value = hoaDon.ThanhTien;
                worksheet.Cells[startRow + hangHoas.Count + 2, 9].Style.Font.Bold = true;

                worksheet.Cells[startRow + hangHoas.Count + 3, 2].Value =
                    $"- Tổng số tiền (viết bằng chữ): {NumberToWords((long)hoaDon.ThanhTien)} đồng.";

                // Xuất file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Xuất kho_{hoaDon.NgayNhan}.xlsx";
                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    excelName
                );
            }
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
