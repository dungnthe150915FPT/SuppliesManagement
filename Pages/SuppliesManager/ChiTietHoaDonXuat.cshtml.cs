using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
    }
}
