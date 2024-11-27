using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.Request;

namespace SuppliesManagement.Pages
{
    public class NhapMuaHangHoaModel : PageModel
    {
        private readonly SuppliesManagementProjectContext dBContext;

        public NhapMuaHangHoaModel(SuppliesManagementProjectContext dBContext)
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
                Id = Guid.NewGuid(),
                NhaCungCap = NhaCungCap,
                KhoHangId = khoHangID,
                SoHoaDon = SoHoaDon,
                NgayNhap = NgayNhap,
                Serial = SoSerial,
                ThanhTien = ThanhTien,
            };
            dBContext.HoaDonNhaps.Add(hoaDonNhap);
            await dBContext.SaveChangesAsync();


            foreach (var item in hangHoaModels)
            {
                var hangHoaExisted = await dBContext.HangHoas.FirstOrDefaultAsync(h => h.TenHangHoa == item.TenHangHoa && 
                h.NgayNhap == NgayNhap && h.DonViTinhId == item.DonViTinhID && h.DonGiaTruocThue == item.DonGiaTruocThue);
                if (hangHoaExisted != null)
                {
                    /*var hangHoa = new HangHoa
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = hangHoaExisted.TenHangHoa,
                        NhomHangId = item.NhomHangID,
                        DonViTinhId = item.DonViTinhID,
                        SoLuong = item.SoLuong + hangHoaExisted.SoLuong,
                        Vat = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue + hangHoaExisted.TongGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)) + hangHoaExisted.TongGiaSauThue,
                        KhoHangId = khoHangID,
                        SoLuongConLai = hangHoaExisted.SoLuongConLai + item.SoLuong,
                        SoLuongDaXuat = hangHoaExisted.SoLuongDaXuat
                    };
                    dBContext.HangHoas.Update(hangHoa);*/
                    hangHoaExisted.NgayNhap = NgayNhap;
                    hangHoaExisted.SoLuong += item.SoLuong;
                    hangHoaExisted.DonGiaTruocThue = hangHoaExisted.DonGiaTruocThue;
                    hangHoaExisted.Vat = hangHoaExisted.Vat;
                    hangHoaExisted.DonGiaSauThue = hangHoaExisted.DonGiaTruocThue * (1 + hangHoaExisted.Vat / 100);
                    hangHoaExisted.TongGiaTruocThue = hangHoaExisted.SoLuong * hangHoaExisted.DonGiaTruocThue;
                    hangHoaExisted.TongGiaSauThue = hangHoaExisted.SoLuong * hangHoaExisted.DonGiaSauThue;
                    hangHoaExisted.KhoHangId = khoHangID;
                    hangHoaExisted.SoLuongConLai += item.SoLuong;
                    dBContext.HangHoas.Update(hangHoaExisted);
                    var hangHoaHoaDon = new HangHoaHoaDon
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = item.TenHangHoa,
                        NhomHangId = item.NhomHangID,
                        DonViTinhId = item.DonViTinhID,
                        SoLuong = item.SoLuong,
                        Vat = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
                        KhoHangId = khoHangID,
                    };
                    dBContext.HangHoaHoaDons.Add(hangHoaHoaDon);

                    var nhapKho = new NhapKho
                    {
                        NhapKhoId = Guid.NewGuid(),
                        HoaDonNhapId = hoaDonNhap.Id,
                        HangHoaHoaDonId = hangHoaHoaDon.Id,
                    };
                    dBContext.NhapKhos.Add(nhapKho);
                }
                else
                {
                    var hangHoa = new HangHoa
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = item.TenHangHoa,
                        NhomHangId = item.NhomHangID,
                        DonViTinhId = item.DonViTinhID,
                        SoLuong = item.SoLuong,
                        Vat = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
                        KhoHangId = khoHangID,
                        SoLuongDaXuat = 0,
                        SoLuongConLai = item.SoLuong,
                        NgayNhap = NgayNhap
                    };
                    dBContext.HangHoas.Add(hangHoa);
                    var hangHoaHoaDon = new HangHoaHoaDon
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = item.TenHangHoa,
                        NhomHangId = item.NhomHangID,
                        DonViTinhId = item.DonViTinhID,
                        SoLuong = item.SoLuong,
                        Vat = item.VAT,
                        DonGiaTruocThue = item.DonGiaTruocThue,
                        DonGiaSauThue = item.DonGiaTruocThue * (1 + item.VAT / 100),
                        TongGiaTruocThue = item.SoLuong * item.DonGiaTruocThue,
                        TongGiaSauThue = item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
                        KhoHangId = khoHangID,
                    };
                    dBContext.HangHoaHoaDons.Add(hangHoaHoaDon);
                    var nhapKho = new NhapKho
                    {
                        NhapKhoId = Guid.NewGuid(),
                        HoaDonNhapId = hoaDonNhap.Id,
                        HangHoaHoaDonId = hangHoaHoaDon.Id,
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