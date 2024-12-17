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
        public List<HangHoa> HangHoas { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            NhomHangs = dBContext.NhomHangs.ToList();
            KhoHangs = dBContext.KhoHangs.Include(k => k.HangHoas).ToList();
            DonViTinhs = dBContext.DonViTinhs
                .Include(d => d.HangHoas)
                .OrderBy(d => d.Name)
                .ToList();
            HangHoas = dBContext.HangHoas.Include(h => h.DonViTinh).ToList();
            return Page();
        }

        public IActionResult OnPost(
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
            var SoHoaDonAndSerialExisted = dBContext.HoaDonNhaps.FirstOrDefault(
                h => h.SoHoaDon == SoHoaDon && h.Serial == SoSerial
            );
            if (SoHoaDonAndSerialExisted == null)
            {
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
                dBContext.SaveChanges();

                foreach (var item in hangHoaModels)
                {
                    var hangHoaExisted = dBContext.HangHoas.FirstOrDefault(
                        h =>
                            h.TenHangHoa == item.TenHangHoa
                            && h.NgayNhap == NgayNhap
                            && h.DonViTinhId == item.DonViTinhID
                            && h.DonGiaTruocThue == item.DonGiaTruocThue
                    );
                    if (hangHoaExisted != null)
                    {
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
                dBContext.SaveChanges();
                TempData["SuccessMessage"] =
                    "Nhập mới hóa đơn hàng hóa có số hóa đơn: "
                    + SoHoaDon
                    + " và số Serial: "
                    + SoSerial
                    + " thành công!";
                return RedirectToPage("./NhapMuaHangHoa");
            }
            else
            {
                TempData["Error"] =
                    "Hóa đơn có số hóa đơn: "
                    + SoHoaDon
                    + " và số Serial: "
                    + SoSerial
                    + " đã tồn tại";
                return Page();
            }
        }

        public IActionResult OnPostImportHoaDon(IFormFile fileXml)
        {
            if (fileXml != null && Path.GetExtension(fileXml.FileName).ToLower() == ".xml")
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        fileXml.CopyTo(stream);
                        stream.Position = 0;
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(stream);

                        var nhaCungCap = xmlDoc.SelectSingleNode("//NBan/Ten")?.InnerText;
                        var ngayNhap = xmlDoc.SelectSingleNode("//NLap")?.InnerText;
                        var khoHangID = xmlDoc.SelectSingleNode("//KhoHangID")?.InnerText;
                        var soSerial = xmlDoc.SelectSingleNode("//KHHDon")?.InnerText;
                        var soHoaDon = xmlDoc.SelectSingleNode("//SHDon")?.InnerText;
                        var tongTien = xmlDoc.SelectSingleNode("//TgTTTBSo")?.InnerText;
                        var thanhTien = xmlDoc.SelectSingleNode("//TgTCThue")?.InnerText;

                        TempData["NhaCungCap"] = nhaCungCap;
                        TempData["NgayNhap"] = ngayNhap;
                        TempData["KhoHangID"] = khoHangID;
                        TempData["SoSerial"] = soSerial;
                        TempData["SoHoaDon"] = soHoaDon;
                        TempData["TongTien"] = tongTien;
                        TempData["ThanhTien"] = thanhTien;

                        var hangHoasXml = xmlDoc.SelectNodes("/HDon/DLHDon/NDHDon/DSHHDVu/HHDVu");
                        var hangHoaList = new List<HangHoaInputModel>();

                        foreach (XmlNode hangHoaNode in hangHoasXml)
                        {
                            var hangHoa = new HangHoaInputModel
                            {
                                TenHangHoa = hangHoaNode.SelectSingleNode("THHDVu")?.InnerText,
                                // SoLuong = int.Parse(
                                //     hangHoaNode.SelectSingleNode("SLuong")?.InnerText ?? "0"
                                // ),
                                SoLuong = (int)
                                    Math.Floor(
                                        double.TryParse(
                                            hangHoaNode.SelectSingleNode("SLuong")?.InnerText,
                                            out double soLuongDouble
                                        )
                                            ? soLuongDouble
                                            : 0
                                    ),
                                DonGiaTruocThue = decimal.Parse(
                                    hangHoaNode.SelectSingleNode("DGia")?.InnerText ?? "0"
                                ),
                                /*                                VAT = int.Parse(hangHoaNode.SelectSingleNode("TSuat")?.InnerText ?? "0"),*/
                            };
                            if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "chiếc"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Chiếc"
                            )
                            {
                                hangHoa.DonViTinhID = 1;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cái"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Cái"
                            )
                            {
                                hangHoa.DonViTinhID = 2;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cặp"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Cặp"
                            )
                            {
                                hangHoa.DonViTinhID = 3;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "bộ"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Bộ"
                            )
                            {
                                hangHoa.DonViTinhID = 4;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "hệ thống"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Hệ thống"
                            )
                            {
                                hangHoa.DonViTinhID = 5;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "ram"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Ram"
                            )
                            {
                                hangHoa.DonViTinhID = 6;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "hộp"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Hộp"
                            )
                            {
                                hangHoa.DonViTinhID = 7;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "tập"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Tập"
                            )
                            {
                                hangHoa.DonViTinhID = 8;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "chuyến"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Chuyến"
                            )
                            {
                                hangHoa.DonViTinhID = 9;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "kg"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Kg"
                            )
                            {
                                hangHoa.DonViTinhID = 10;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "chai"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Chai"
                            )
                            {
                                hangHoa.DonViTinhID = 11;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "EA"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "ea"
                            )
                            {
                                hangHoa.DonViTinhID = 12;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Phần"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "phần"
                            )
                            {
                                hangHoa.DonViTinhID = 13;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Đôi"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "đôi"
                            )
                            {
                                hangHoa.DonViTinhID = 14;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Nồi"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "nồi"
                            )
                            {
                                hangHoa.DonViTinhID = 15;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Cuộn"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cuộn"
                            )
                            {
                                hangHoa.DonViTinhID = 15;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Nồi"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "nồi"
                            )
                            {
                                hangHoa.DonViTinhID = 16;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Túi"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "túi"
                            )
                            {
                                hangHoa.DonViTinhID = 17;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Suất"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "suất"
                            )
                            {
                                hangHoa.DonViTinhID = 18;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Người"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "người"
                            )
                            {
                                hangHoa.DonViTinhID = 19;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Chai"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "chai"
                            )
                            {
                                hangHoa.DonViTinhID = 20;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Lọ"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "lọ"
                            )
                            {
                                hangHoa.DonViTinhID = 21;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Cốc"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cốc"
                            )
                            {
                                hangHoa.DonViTinhID = 22;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Bàn"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "bàn"
                            )
                            {
                                hangHoa.DonViTinhID = 23;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Lốc"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "lốc"
                            )
                            {
                                hangHoa.DonViTinhID = 24;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Can"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "can"
                            )
                            {
                                hangHoa.DonViTinhID = 25;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Dây"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "dây"
                            )
                            {
                                hangHoa.DonViTinhID = 26;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Quả"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "quả"
                            )
                            {
                                hangHoa.DonViTinhID = 27;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Bình"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "bình"
                            )
                            {
                                hangHoa.DonViTinhID = 28;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Gói"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "gói"
                            )
                            {
                                hangHoa.DonViTinhID = 29;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Hũ"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "hũ"
                            )
                            {
                                hangHoa.DonViTinhID = 30;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Đĩa"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "đĩa"
                            )
                            {
                                hangHoa.DonViTinhID = 31;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Bát"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "bát"
                            )
                            {
                                hangHoa.DonViTinhID = 32;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Lon"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "lon"
                            )
                            {
                                hangHoa.DonViTinhID = 33;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Cuốn"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "cuốn"
                            )
                            {
                                hangHoa.DonViTinhID = 34;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "kWh"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "KWh"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "KWH"
                            )
                            {
                                hangHoa.DonViTinhID = 35;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Vận đơn"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "vận đơn"
                            )
                            {
                                hangHoa.DonViTinhID = 36;
                            }
                            else if (
                                hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "m"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "M"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "Mét"
                                || hangHoaNode.SelectSingleNode("DVTinh")?.InnerText == "mét"
                            )
                            {
                                hangHoa.DonViTinhID = 37;
                            }
                            else
                            {
                                hangHoa.DonViTinhID = 2;
                            }

                            if (
                                hangHoaNode.SelectSingleNode("TSuat")?.InnerText == "KCT"
                                || hangHoaNode.SelectSingleNode("TSuat")?.InnerText == "0%"
                                || hangHoaNode.SelectSingleNode("TSuat")?.InnerText == null
                            )
                            {
                                hangHoa.VAT = 0;
                            }
                            else if (hangHoaNode.SelectSingleNode("TSuat")?.InnerText == "5%")
                            {
                                hangHoa.VAT = 5;
                            }
                            else if (hangHoaNode.SelectSingleNode("TSuat")?.InnerText == "8%")
                            {
                                hangHoa.VAT = 8;
                            }
                            else if (hangHoaNode.SelectSingleNode("TSuat")?.InnerText == "10%")
                            {
                                hangHoa.VAT = 10;
                            }
                            hangHoaList.Add(hangHoa);
                        }
                        TempData["HangHoaList"] = System.Text.Json.JsonSerializer.Serialize(
                            hangHoaList
                        );
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

            return RedirectToPage("/SuppliesManager/NhapMuaHangHoa");
        }
    }
}
