using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class XuatDungHangHoaModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public XuatDungHangHoaModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<KhoHang> KhoHangs { get; set; }
        public List<Account> Accounts { get; set; }
        public List<HangHoa> HangHoas { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            KhoHangs = _dbContext.KhoHangs?.ToList() ?? new List<KhoHang>();
            Accounts =
                _dbContext.Accounts?.Where(a => a.RoleId == 3).ToList() ?? new List<Account>();
            HangHoas =
                _dbContext.HangHoas
                    .Where(h => h.SoLuongConLai > 0)
                    ?.Include(h => h.NhomHang)
                    .Include(h => h.DonViTinh)
                    .OrderByDescending(h => h.TenHangHoa)
                    .ToList() ?? new List<HangHoa>();
            return Page();
        }

        public IActionResult OnPost(
            Guid KhoHangId,
            Guid NguoiNhanId,
            DateTime NgayNhan,
            string LyDoNhan,
            List<Guid> HangHoaIds,
            List<int> SoLuongs,
            decimal ThanhTien
        )
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu đầu vào không hợp lệ.");
                return Page();
            }

            KhoHangs = _dbContext.KhoHangs?.ToList() ?? new List<KhoHang>();
            Accounts =
                _dbContext.Accounts?.Where(a => a.RoleId == 3).ToList() ?? new List<Account>();
            HangHoas =
                _dbContext.HangHoas
                    .Where(h => h.SoLuongConLai > 0)
                    ?.Include(h => h.NhomHang)
                    .Include(h => h.DonViTinh)
                    .OrderByDescending(h => h.TenHangHoa)
                    .ToList() ?? new List<HangHoa>();

            // Kiểm tra danh sách dữ liệu từ form
            // if (HangHoaIds == null || SoLuongs == null || HangHoaIds.Count != SoLuongs.Count)
            // {
            //     ModelState.AddModelError(
            //         string.Empty,
            //         "Danh sách hàng hóa hoặc số lượng không hợp lệ."
            //     );
            //     return Page();
            // }

            if (HangHoaIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Không có hàng hóa nào để xuất.");
                return Page();
            }

            try
            {
                // Tạo hóa đơn xuất
                var hoaDonXuat = new HoaDonXuat
                {
                    Id = Guid.NewGuid(),
                    KhoHangId = KhoHangId,
                    NguoiNhanId = NguoiNhanId,
                    NgayNhan = NgayNhan,
                    LyDoNhan = LyDoNhan,
                    ThanhTien = ThanhTien
                };

                _dbContext.HoaDonXuats.Add(hoaDonXuat);

                // Xử lý từng hàng hóa
                for (int i = 0; i < HangHoaIds.Count; i++)
                {
                    var hangHoa = _dbContext.HangHoas.FirstOrDefault(h => h.Id == HangHoaIds[i]);

                    if (hangHoa == null)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            $"Hàng hóa với ID {HangHoaIds[i]} không tồn tại."
                        );
                        TempData["Error"] = $"Hàng hóa với ID {HangHoaIds[i]} không tồn tại.";
                        return Page();
                    }
                    else if (hangHoa.SoLuongConLai < SoLuongs[i])
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            $"Số lượng tồn kho của hàng hóa '{hangHoa.TenHangHoa}' không đủ."
                        );
                        TempData["Error"] =
                            $"Số lượng tồn kho của hàng hóa '{hangHoa.TenHangHoa}' không đủ.";
                        return Page();
                    }
                    else
                    {
                        // Cập nhật tồn kho
                        hangHoa.SoLuongConLai -= SoLuongs[i];
                        hangHoa.SoLuongDaXuat += SoLuongs[i];
                        _dbContext.HangHoas.Update(hangHoa);

                        // Tạo chi tiết hóa đơn
                        var tongGiaTruocThue = SoLuongs[i] * hangHoa.DonGiaTruocThue;
                        var tongGiaSauThue = tongGiaTruocThue * (1 + hangHoa.Vat / 100);

                        var hangHoaHoaDon = new HangHoaHoaDon
                        {
                            Id = Guid.NewGuid(),
                            TenHangHoa = hangHoa.TenHangHoa,
                            DonViTinhId = hangHoa.DonViTinhId,
                            SoLuong = SoLuongs[i],
                            DonGiaTruocThue = hangHoa.DonGiaTruocThue,
                            DonGiaSauThue = hangHoa.DonGiaTruocThue * (1 + hangHoa.Vat / 100),
                            Vat = hangHoa.Vat,
                            TongGiaTruocThue = tongGiaTruocThue,
                            TongGiaSauThue = tongGiaSauThue,
                            KhoHangId = KhoHangId,
                            NhomHangId = hangHoa.NhomHangId
                        };

                        _dbContext.HangHoaHoaDons.Add(hangHoaHoaDon);

                        var xuatKho = new XuatKho
                        {
                            XuatKhoId = Guid.NewGuid(),
                            HoaDonXuatId = hoaDonXuat.Id,
                            HangHoaHoaDonId = hangHoaHoaDon.Id
                        };
                        _dbContext.XuatKhos.Add(xuatKho);
                    }
                }
                // Lưu thay đổi
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Xuất dùng hàng hóa thành công!";
                return RedirectToPage("./XuatDungHangHoa");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                TempData["Error"] = "Đã xảy ra lỗi:" + ex.Message;
                return Page();
            }
        }
    }
}
