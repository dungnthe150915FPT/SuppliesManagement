using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.DBContext;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuppliesManagement.Pages
{
    public class DanhSachHoaDonNhapModel : PageModel
    {
        private readonly SuppliesManagementDBContext _dbContext;

        public DanhSachHoaDonNhapModel(SuppliesManagementDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<HoaDonNhapViewModel> HoaDonNhaps { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.HoaDonNhaps.Include(n => n.KhoHang).AsQueryable();

            // Lọc theo khoảng ngày nếu có
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
                    ID = h.ID,
                    NhaCungCap = h.NhaCungCap,
                    NgayNhap = h.NgayNhap,
                    SoHoaDon = h.SoHoaDon,
                    ThanhTien = h.ThanhTien,
                    Serial = h.Serial,
                    KhoNhap = h.KhoHang.TenKho,
                    HangHoas = _dbContext.NhapKhos
                        .Where(n => n.HoaDonNhapID == h.ID)
                        .Include(n => n.HangHoa)
                        .Select(n => new HangHoaViewModel
                        {
                            TenKhoHang = n.HangHoa.KhoHang.TenKho,
                            TenHangHoa = n.HangHoa.TenHangHoa,
                            SoLuong = n.HangHoa.SoLuong,
                            DonGiaTruocThue = n.HangHoa.DonGiaTruocThue,
                            DonGiaSauThue = n.HangHoa.DonGiaSauThue,
                            TongGiaTruocThue = n.HangHoa.TongGiaTruocThue,
                            TongGiaSauThue = n.HangHoa.TongGiaSauThue
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}