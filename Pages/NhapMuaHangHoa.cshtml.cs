using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.DBContext;
using SuppliesManagement.Models;
using SuppliesManagement.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuppliesManagement.Pages
{
    public class NhapMuaHangHoaModel : PageModel
    {
        private readonly SuppliesManagementDBContext dBContext;

        public NhapMuaHangHoaModel(SuppliesManagementDBContext dBContext)
        {
            this.dBContext = dBContext;
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
        public async Task<IActionResult> OnPostAsync(Guid khoHangID, string NhaCungCap, DateTime NgayNhap,
            string SoSerial, string SoHoaDon, decimal ThanhTien, List<HangHoaInputModel> hangHoaModels)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var hoaDonNhap = new HoaDonNhap
            {
                NhaCungCap = NhaCungCap,
                KhoHangID = khoHangID,
                SoHoaDon = SoHoaDon,
                NgayNhap = NgayNhap,
                Serial = SoSerial,
                ThanhTien = ThanhTien,
            };
            dBContext.HoaDonNhaps.Add(hoaDonNhap);
            await dBContext.SaveChangesAsync();


            foreach (var item in hangHoaModels)
            {
                //var hangHoaExisted = await dBContext.HangHoas.FirstOrDefaultAsync(h => h.TenHangHoa == item.TenHangHoa);
                var hangHoaExisted = await dBContext.HangHoas
        .FirstOrDefaultAsync(h => EF.Functions.Like(h.TenHangHoa, item.TenHangHoa.Trim()));
                if (hangHoaExisted != null)
                {
                    var hangHoa = new HangHoa
                    {
                        TenHangHoa = hangHoaExisted.TenHangHoa,
                        NhomHangID = item.NhomHangID,
                        DonViTinhID = item.DonViTinhID,
                        SoLuong = item.SoLuong + hangHoaExisted.SoLuong,
                        VAT = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue + hangHoaExisted.TongGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)) + hangHoaExisted.TongGiaSauThue,
                        KhoHangID = khoHangID,
                        SoLuongConLai = hangHoaExisted.SoLuongConLai + item.SoLuong,
                        SoLuongDaXuat = hangHoaExisted.SoLuongDaXuat
                    };
                    dBContext.HangHoas.Update(hangHoa);
                    var nhapKho = new NhapKho
                    {
                        HoaDonNhapID = hoaDonNhap.ID,
                        HangHoaID = hangHoa.Id,
                    };
                    dBContext.NhapKhos.Add(nhapKho);
                }
                else
                {
                    var hangHoa = new HangHoa
                    {
                        TenHangHoa = item.TenHangHoa,
                        NhomHangID = item.NhomHangID,
                        DonViTinhID = item.DonViTinhID,
                        SoLuong = item.SoLuong,
                        VAT = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
                        KhoHangID = khoHangID,
                        SoLuongDaXuat = 0,
                        SoLuongConLai = item.SoLuong
                    };
                    dBContext.HangHoas.Add(hangHoa);

                    var nhapKho = new NhapKho
                    {
                        HoaDonNhapID = hoaDonNhap.ID,
                        HangHoaID = hangHoa.Id,
                    };
                    dBContext.NhapKhos.Add(nhapKho);
                }
            }
            await dBContext.SaveChangesAsync();
            /*ViewData["Success"] = "Thêm hóa đơn và hàng hóa thành công";*/
            return RedirectToPage("./NhapMuaHangHoa");
        }
    }
}