using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuppliesManagement.Pages
{
    public class DanhSachHoaDonNhapModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public DanhSachHoaDonNhapModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<HoaDonNhapViewModel> HoaDonNhaps { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.HoaDonNhaps.Include(n => n.KhoHang).AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap <= endDate.Value);
            }

            HoaDonNhaps = await query
                .Select(h => new HoaDonNhapViewModel
                {
                    ID = h.Id,
                    NhaCungCap = h.NhaCungCap,
                    NgayNhap = h.NgayNhap,
                    SoHoaDon = h.SoHoaDon,
                    ThanhTien = h.ThanhTien,
                    Serial = h.Serial,
                    KhoNhap = h.KhoHang.Ten,
                    HangHoas = _dbContext.NhapKhos
                        .Where(n => n.HoaDonNhapId == h.Id)
                        .Include(n => n.HangHoaHoaDon)
                        .Select(n => new HangHoaViewModel
                        {
                            TenKhoHang = n.HangHoaHoaDon.KhoHang.Ten,
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