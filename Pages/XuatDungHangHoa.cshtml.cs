using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.Request;

namespace SuppliesManagement.Pages
{
    public class XuatDungHangHoaModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public XuatDungHangHoaModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }
        public List<HangHoa> HangHoas { get; set; }
        public List<KhoHang> KhoHangs { get; set; }
        public List<Account> Accounts { get; set; }
        public async Task OnGetAsync()
        {
            HangHoas = await context.HangHoas
                .Include(h => h.NhomHang) 
                .Include(h => h.DonViTinh) 
                .Where(h => h.SoLuongConLai > 0) 
                .ToListAsync();
            KhoHangs = await context.KhoHangs.ToListAsync();
            Accounts = await context.Accounts.Include(a => a.Role).ToListAsync();
        }


        public async Task<IActionResult> OnPostAsync(
    Guid khoHangID,
    Guid NguoiNhanId,
    DateTime NgayNhan,
    string LyDoNhan,
    List<HangHoaInputModel> hangHoaModels)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var hoaDonXuat = new HoaDonXuat
            {
                Id = Guid.NewGuid(),
                KhoHangId = khoHangID,
                NguoiNhanId = NguoiNhanId,
                NgayNhan = NgayNhan,
                LyDoNhan = LyDoNhan,
                ThanhTien = 0 // Sẽ được cập nhật sau
            };

            // Lưu hóa đơn xuất
            context.HoaDonXuats.Add(hoaDonXuat);
            await context.SaveChangesAsync();

            decimal totalAmount = 0; // Tổng tiền hoá đơn

            foreach (var item in hangHoaModels)
            {
                var hangHoa = await context.HangHoas.FirstOrDefaultAsync(h => h.Id == item.Id);
                if (hangHoa == null || hangHoa.SoLuongConLai < item.SoLuong)
                {
                    ModelState.AddModelError(string.Empty, $"Số lượng hàng hóa {item.TenHangHoa} không đủ.");
                    return Page();
                }

                // Cập nhật số lượng hàng hóa
                hangHoa.SoLuongConLai -= item.SoLuong;
                context.HangHoas.Update(hangHoa);

                var hangHoaHoaDon = new HangHoaHoaDon
                {
                    Id = Guid.NewGuid(),
                    TenHangHoa = hangHoa.TenHangHoa,
                    DonViTinhId = hangHoa.DonViTinhId,
                    DonGiaTruocThue = hangHoa.DonGiaTruocThue,
                    DonGiaSauThue = hangHoa.DonGiaTruocThue * (1 + hangHoa.Vat / 100),
                    Vat = hangHoa.Vat,
                    TongGiaTruocThue = item.SoLuong * hangHoa.DonGiaTruocThue,
                    TongGiaSauThue = item.SoLuong * (hangHoa.DonGiaTruocThue * (1 + hangHoa.Vat / 100)),
                    SoLuong = item.SoLuong,
                    KhoHangId = khoHangID,
                    NhomHangId = hangHoa.NhomHangId
                };

                context.HangHoaHoaDons.Add(hangHoaHoaDon);

                totalAmount += hangHoaHoaDon.TongGiaTruocThue; // Cộng dồn tổng tiền
            }

            hoaDonXuat.ThanhTien = totalAmount; // Cập nhật tổng tiền sau khi tính toán
            context.HoaDonXuats.Update(hoaDonXuat);
            await context.SaveChangesAsync();

            return RedirectToPage("./XuatDungHangHoa");
        }
    }
}
