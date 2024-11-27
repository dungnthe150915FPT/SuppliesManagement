using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Entities.ViewModels;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;

namespace SuppliesManagement.Pages
{
    public class DanhSachHoaDonXuatModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public DanhSachHoaDonXuatModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }
        public List<HoaDonXuatViewModel> HoaDonXuats { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = context.HoaDonXuats.Include(n => n.KhoHang).AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(h => h.NgayNhan >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(h => h.NgayNhan <= endDate.Value);
            }

            HoaDonXuats = await query
                .Select(h => new HoaDonXuatViewModel
                {
                    Id = h.Id,
                    LyDoNhan = h.LyDoNhan,
                    ThanhTien = h.ThanhTien,
                    NgayNhan = h.NgayNhan,
                    NguoiNhanUsername = h.NguoiNhan.Username,
                    KhoHang = h.KhoHang.Ten,
                    HangHoas = context.XuatKhos
                        .Where(n => n.HoaDonXuatId == h.Id)
                        .Include(n => n.HangHoaHoaDon).ThenInclude(n => n.KhoHang)
                        .Select(n => new HangHoaViewModel
                        {
                            TenHangHoa = n.HangHoaHoaDon.TenHangHoa,
                            SoLuong = n.HangHoaHoaDon.SoLuong,
                            DonGiaTruocThue = n.HangHoaHoaDon.DonGiaTruocThue,
                            DonGiaSauThue = n.HangHoaHoaDon.DonGiaSauThue,
                            TongGiaTruocThue = n.HangHoaHoaDon.TongGiaTruocThue,
                            TongGiaSauThue = n.HangHoaHoaDon.TongGiaSauThue
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
