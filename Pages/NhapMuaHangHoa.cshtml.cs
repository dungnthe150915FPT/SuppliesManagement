using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.Request;
using System.Xml;
using System.Xml.Linq;

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
            NhomHangs = await dBContext.NhomHangs
                .Include(h => h.HangHoas)
                .Include(h => h.HangHoaHoaDons)
                .ToListAsync();
            KhoHangs = await dBContext.KhoHangs.Include(k => k.HangHoas).ToListAsync();
            DonViTinhs = await dBContext.DonViTinhs.Include(d => d.HangHoas).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(
            Guid khoHangID,
            string NhaCungCap,
            DateTime NgayNhap,
            string SoSerial,
            string SoHoaDon,
            decimal ThanhTien,
            List<HangHoaInputModel> hangHoaModels
        )
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
                var hangHoaExisted = await dBContext.HangHoas.FirstOrDefaultAsync(
                    h =>
                        h.TenHangHoa == item.TenHangHoa
                        && h.NgayNhap == NgayNhap
                        && h.DonViTinhId == item.DonViTinhID
                        && h.DonGiaTruocThue == item.DonGiaTruocThue
                );
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
                    hangHoaExisted.DonGiaSauThue =
                        hangHoaExisted.DonGiaTruocThue * (1 + hangHoaExisted.Vat / 100);
                    hangHoaExisted.TongGiaTruocThue =
                        hangHoaExisted.SoLuong * hangHoaExisted.DonGiaTruocThue;
                    hangHoaExisted.TongGiaSauThue =
                        hangHoaExisted.SoLuong * hangHoaExisted.DonGiaSauThue;
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
                        TongGiaSauThue =
                            item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
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
                        TongGiaSauThue =
                            item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
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
                        TongGiaSauThue =
                            item.SoLuong * (item.DonGiaTruocThue * (1 + item.VAT / 100)),
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

        public async Task<IActionResult> OnPostImportHoaDon(IFormFile fileXml)
        {
            if (fileXml != null && Path.GetExtension(fileXml.FileName).ToLower() == ".xml")
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await fileXml.CopyToAsync(stream);
                        stream.Position = 0;
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(stream);

                        var nhaCungCap = xmlDoc.SelectSingleNode("//NBan/Ten")?.InnerText;
                        var ngayNhap = xmlDoc.SelectSingleNode("//NLap")?.InnerText;
                        var khoHangID = xmlDoc.SelectSingleNode("//KhoHangID")?.InnerText;
                        var soSerial = xmlDoc.SelectSingleNode("//KHHDon")?.InnerText;
                        var soHoaDon = xmlDoc.SelectSingleNode("//SHDon")?.InnerText;
                        var thanhTien = xmlDoc.SelectSingleNode("//TgTTTBSo")?.InnerText;

                        TempData["NhaCungCap"] = nhaCungCap;
                        TempData["NgayNhap"] = ngayNhap;
                        TempData["KhoHangID"] = khoHangID;
                        TempData["SoSerial"] = soSerial;
                        TempData["SoHoaDon"] = soHoaDon;
                        TempData["ThanhTien"] = thanhTien;

                        var hangHoasXml = xmlDoc.SelectNodes("//DSHHDVu/HHDVu");
                        var hangHoaList = new List<HangHoaInputModel>();

                        foreach (XmlNode hangHoaNode in hangHoasXml)
                        {
                            var hangHoa = new HangHoaInputModel
                            {
                                TenHangHoa = hangHoaNode.SelectSingleNode("THHDVu")?.InnerText,
                                SoLuong = int.Parse(hangHoaNode.SelectSingleNode("SLuong")?.InnerText ?? "0"),
                                DonGiaTruocThue = decimal.Parse(hangHoaNode.SelectSingleNode("DGia")?.InnerText ?? "0"),
                                VAT = int.Parse(hangHoaNode.SelectSingleNode("TSuat")?.InnerText ?? "0"),
                            };
                            if(hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "chiếc")
                            {
                                hangHoa.DonViTinhID = 1;
                            }else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cái")
                            {
                                hangHoa.DonViTinhID = 2;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cặp")
                            {
                                hangHoa.DonViTinhID = 3;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "bộ")
                            {
                                hangHoa.DonViTinhID = 4;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "hệ thống")
                            {
                                hangHoa.DonViTinhID = 5;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "ram")
                            {
                                hangHoa.DonViTinhID = 6;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "hộp")
                            {
                                hangHoa.DonViTinhID = 7;
                            }
                            else if (hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "tập")
                            {
                                hangHoa.DonViTinhID = 8;
                            }
                            hangHoaList.Add(hangHoa);
                        }
                        TempData["HangHoaList"] = System.Text.Json.JsonSerializer.Serialize(hangHoaList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi trong quá trình xử lý file XML: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Định dạng tệp không đúng. Chỉ cho phép tệp .xml.";
            }

            return RedirectToAction("./NhapMuaHangHoa");
        }
    }
}
