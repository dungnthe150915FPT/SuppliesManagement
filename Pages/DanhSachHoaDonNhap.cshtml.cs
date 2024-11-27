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
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int ItemsPerPage = 10;

        /*public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
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
        }*/

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, int page = 1)
        {
            CurrentPage = page;

            var query = _dbContext.HoaDonNhaps.Include(n => n.KhoHang).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(h => h.NgayNhap <= endDate.Value);
            }

            // Tính tổng số hóa đơn và tổng số trang
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)ItemsPerPage);

            // Lấy hóa đơn cho trang hiện tại
            HoaDonNhaps = await query
                .OrderByDescending(h => h.NgayNhap)
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .Select(h => new HoaDonNhapViewModel
                {
                    ID = h.Id,
                    NhaCungCap = h.NhaCungCap,
                    NgayNhap = h.NgayNhap,
                    SoHoaDon = h.SoHoaDon,
                    ThanhTien = h.ThanhTien,
                    Serial = h.Serial,
                    KhoNhap = h.KhoHang.Ten,
                })
                .ToListAsync();
        }
    }
}