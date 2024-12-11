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
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu đầu vào không hợp lệ.");
                return Page();
            }

            // Thêm hóa đơn xuất
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
            _dbContext.SaveChanges();

            for (int i = 0; i < HangHoaIds.Count; i++)
            {
                var hangHoa = _dbContext.HangHoas.FirstOrDefault(h => h.Id == HangHoaIds[i]);

                if (hangHoa == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Hàng hóa với ID {HangHoaIds[i]} không tồn tại."
                    );
                    return Page();
                }

                /*if (hangHoa.SoLuongConLai < SoLuongs[i])
                {
                    ModelState.AddModelError(string.Empty, $"Số lượng tồn kho của hàng hóa '{hangHoa.TenHangHoa}' không đủ.");
                    return Page();
                }*/

                // Cập nhật số lượng còn lại
                hangHoa.SoLuongConLai -= SoLuongs[i];
                _dbContext.HangHoas.Update(hangHoa);

                // Tính toán chi tiết hóa đơn
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

            _dbContext.SaveChanges();
            return RedirectToPage("./XuatDungHangHoa");
        }
    }
}
