using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.Request;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SuppliesManagement.Pages
{
    public class ImportHoaDonNhapModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        /*public ImportHoaDonNhapModel(SuppliesManagementProjectContext dBContext)
        {
            dBContext = dBContext;
        }*/

        public ImportHoaDonNhapModel(SuppliesManagementProjectContext dBContext)
        {
            this.dBContext = dBContext ?? throw new ArgumentNullException(nameof(dBContext));
        }

        public List<NhomHang> NhomHangs { get; set; }
        public List<KhoHang> KhoHangs { get; set; }
        public List<DonViTinh> DonViTinhs { get; set; }
        public async Task OnGet()
        {
            NhomHangs = await dBContext.NhomHangs.ToListAsync();
            KhoHangs = await dBContext.KhoHangs.ToListAsync();
            DonViTinhs = await dBContext.DonViTinhs.ToListAsync();
        }

        // Thuộc tính để hiển thị thông tin hóa đơn
        public string NhaCungCap { get; set; }
        public string SoHoaDon { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public List<HangHoaInputModel> HangHoaModels { get; set; } = new List<HangHoaInputModel>();

        public async Task<IActionResult> OnPostImportPdfAsync(IFormFile pdfFile)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    Console.WriteLine($"Key: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            if (pdfFile == null || pdfFile.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file PDF.");
                return Page();
            }

            try
            {
                using (var stream = pdfFile.OpenReadStream())
                {
                    using (var pdf = PdfDocument.Open(stream))
                    {
                        foreach (var page in pdf.GetPages()) // Chỉnh sửa ở đây
                        {
                            var text = page.Text;
                            if (string.IsNullOrWhiteSpace(NhaCungCap))
                            {
                                NhaCungCap = ExtractNhaCungCap(text);
                            }

                            if (string.IsNullOrWhiteSpace(SoHoaDon))
                            {
                                SoHoaDon = ExtractSoHoaDon(text);
                            }

                            if (HangHoaModels.Count == 0)
                            {
                                HangHoaModels.AddRange(ExtractHangHoaData(text));
                            }
                        }
                    }
                }

                if (HangHoaModels.Count == 0)
                {
                    ModelState.AddModelError("", "Không tìm thấy dữ liệu hàng hóa trong file PDF.");
                    return Page();
                }

                return Page(); // Hiển thị dữ liệu đã đọc trên giao diện
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi khi đọc file PDF: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSaveInvoiceAsync(Guid khoHangID)
        {
            using var transaction = await dBContext.Database.BeginTransactionAsync();
            try
            {
                // Tạo và lưu hóa đơn nhập
                var hoaDonNhap = new HoaDonNhap
                {
                    Id = Guid.NewGuid(),
                    NhaCungCap = NhaCungCap,
                    SoHoaDon = SoHoaDon,
                    NgayNhap = NgayNhap,
                    KhoHangId = khoHangID,
                    ThanhTien = HangHoaModels.Sum(h => h.DonGiaTruocThue * h.SoLuong),
                };
                dBContext.HoaDonNhaps.Add(hoaDonNhap);
                await dBContext.SaveChangesAsync();

                // Xử lý từng hàng hóa
                foreach (var hangHoa in HangHoaModels)
                {
                    var existingHangHoa = await dBContext.HangHoas
                        .FirstOrDefaultAsync(h => h.TenHangHoa == hangHoa.TenHangHoa);

                    if (existingHangHoa == null)
                    {
                        // Thêm mới hàng hóa
                        var newHangHoa = new HangHoa
                        {
                            Id = Guid.NewGuid(),
                            TenHangHoa = hangHoa.TenHangHoa,
                            NhomHangId = hangHoa.NhomHangID,
                            DonViTinhId = hangHoa.DonViTinhID,
                            SoLuong = hangHoa.SoLuong,
                            DonGiaTruocThue = hangHoa.DonGiaTruocThue,
                            DonGiaSauThue = hangHoa.DonGiaTruocThue * (1 + hangHoa.VAT / 100),
                            TongGiaTruocThue = hangHoa.DonGiaTruocThue * hangHoa.SoLuong,
                            TongGiaSauThue = hangHoa.DonGiaTruocThue * hangHoa.SoLuong * (1 + hangHoa.VAT / 100),
                            KhoHangId = khoHangID,
                        };
                        dBContext.HangHoas.Add(newHangHoa);
                    }
                    else
                    {
                        // Cập nhật hàng hóa hiện có
                        existingHangHoa.SoLuong += hangHoa.SoLuong;
                        existingHangHoa.TongGiaTruocThue += hangHoa.DonGiaTruocThue * hangHoa.SoLuong;
                        existingHangHoa.TongGiaSauThue += hangHoa.DonGiaTruocThue * hangHoa.SoLuong * (1 + hangHoa.VAT / 100);
                        dBContext.HangHoas.Update(existingHangHoa);
                    }

                    // Lưu thông tin hóa đơn hàng hóa
                    var hangHoaHoaDon = new HangHoaHoaDon
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = hangHoa.TenHangHoa,
                        NhomHangId = hangHoa.NhomHangID,
                        DonViTinhId = hangHoa.DonViTinhID,
                        SoLuong = hangHoa.SoLuong,
                        DonGiaTruocThue = hangHoa.DonGiaTruocThue,
                        DonGiaSauThue = hangHoa.DonGiaTruocThue * (1 + hangHoa.VAT / 100),
                        TongGiaTruocThue = hangHoa.DonGiaTruocThue * hangHoa.SoLuong,
                        TongGiaSauThue = hangHoa.DonGiaTruocThue * hangHoa.SoLuong * (1 + hangHoa.VAT / 100),
                        KhoHangId = khoHangID
                    };
                    dBContext.HangHoaHoaDons.Add(hangHoaHoaDon);
                }

                // Lưu tất cả thay đổi
                await dBContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToPage("./NhapMuaHangHoa");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Đã xảy ra lỗi khi lưu dữ liệu: {ex.Message}");
                return Page();
            }
        }

        private string ExtractNhaCungCap(string text)
        {
            var lines = text.Split('\n');
            var nhaCungCap = lines.FirstOrDefault(l => l.Contains("Nhà cung cấp:"))?.Replace("Nhà cung cấp:", "").Trim();
            Console.WriteLine($"NhaCungCap: {nhaCungCap}");
            return nhaCungCap ?? string.Empty;
        }

        private List<HangHoaInputModel> ExtractHangHoaData(string text)
        {
            var hangHoaList = new List<HangHoaInputModel>();
            var lines = text.Split('\n').SkipWhile(line => !line.Contains("Hàng hóa:")).Skip(1);

            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                if (parts.Length >= 6)
                {
                    hangHoaList.Add(new HangHoaInputModel
                    {
                        TenHangHoa = parts[0].Trim(),
                        NhomHangID = int.Parse(parts[1]),
                        SoLuong = int.Parse(parts[2]),
                        DonViTinhID = int.Parse(parts[3]),
                        DonGiaTruocThue = decimal.Parse(parts[4]),
                        VAT = int.Parse(parts[5])
                    });
                }
            }
            return hangHoaList;
        }


        private string ExtractSoHoaDon(string text)
        {
            var lines = text.Split('\n');
            var soHoaDon = lines.FirstOrDefault(l => l.Contains("Số hóa đơn:"))?.Replace("Số hóa đơn:", "").Trim();
            Console.WriteLine($"SoHoaDon: {soHoaDon}");
            return soHoaDon ?? string.Empty;
        }
    }
}
