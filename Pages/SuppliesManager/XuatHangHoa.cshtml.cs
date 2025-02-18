using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class XuatHangHoaModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public XuatHangHoaModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<KhoHang> KhoHangs { get; set; }
        public List<Account> Accounts { get; set; }
        public List<HangHoa> HangHoas { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; }

        public IActionResult OnGet(string sortOrder)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            KhoHangs = _dbContext.KhoHangs?.ToList() ?? new List<KhoHang>();
            Accounts =
                _dbContext.Accounts?.Where(a => a.RoleId == 3).ToList() ?? new List<Account>();
            IQueryable<HangHoa> query = _dbContext.HangHoas
                .Where(h => h.SoLuongConLai > 0)
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh);

            SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "date_desc":
                    query = query.OrderByDescending(h => h.NgayNhap);
                    break;
                case "date_asc":
                    query = query.OrderBy(h => h.NgayNhap);
                    break;
                case "dongia_asc":
                    query = query.OrderBy(h => h.DonGiaTruocThue);
                    break;
                case "dongia_desc":
                    query = query.OrderByDescending(h => h.DonGiaTruocThue);
                    break;
                case "slcl_desc":
                    query = query.OrderByDescending(h => h.SoLuongConLai);
                    break;
                case "slcl_asc":
                    query = query.OrderBy(h => h.SoLuongConLai);
                    break;
                default:
                    query = query.OrderByDescending(h => h.NgayNhap);
                    break;
            }

            HangHoas = query.ToList();

            return Page();
        }

        public IActionResult OnPost(
            Guid KhoHangId,
            Guid NguoiNhanId,
            DateTime NgayNhan,
            string LyDoNhan,
            Dictionary<Guid, int> SoLuongs // Đọc danh sách số lượng xuất từ form
        )
        {
            // Kiểm tra ModelState hợp lệ
            /*            if (!ModelState.IsValid)
                        {
                            ModelState.AddModelError(string.Empty, "Dữ liệu đầu vào không hợp lệ.");
                            return Page();
                        }*/

            // Kiểm tra danh sách số lượng
            if (SoLuongs == null || SoLuongs.Count == 0 || !SoLuongs.Any(s => s.Value > 0))
            {
                ModelState.AddModelError(string.Empty, "Không có hàng hóa nào được chọn để xuất.");
                TempData["Error"] = "Không có hàng hóa nào được chọn để xuất.";
                OnGet(null);
                return Page();
            }

            try
            {
                // Tạo hóa đơn xuất
                var hoaDonXuat = new HoaDonXuat
                {
                    Id = Guid.NewGuid(),
                    KhoHangId = KhoHangId,
                    NguoiNhanId = NguoiNhanId,
                    NgayNhan = NgayNhan,
                    LyDoNhan = LyDoNhan,
                    ThanhTien = 0 // Sẽ được tính lại sau
                };
                _dbContext.HoaDonXuats.Add(hoaDonXuat);

                decimal tongThanhTien = 0;

                // Lặp qua danh sách số lượng xuất
                foreach (var item in SoLuongs)
                {
                    var hangHoaId = item.Key;
                    var soLuongXuat = item.Value;

                    // Bỏ qua nếu số lượng xuất bằng 0
                    if (soLuongXuat <= 0)
                    {
                        continue;
                    }

                    // Lấy hàng hóa từ cơ sở dữ liệu
                    var hangHoa = _dbContext.HangHoas.FirstOrDefault(h => h.Id == hangHoaId);
                    if (hangHoa == null)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            $"Hàng hóa với ID {hangHoaId} không tồn tại."
                        );
                        TempData["Error"] = $"Hàng hóa với ID {hangHoaId} không tồn tại.";
                        return Page();
                    }

                    // Kiểm tra số lượng tồn kho
                    if (hangHoa.SoLuongConLai < soLuongXuat)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            $"Số lượng tồn kho của hàng hóa '{hangHoa.TenHangHoa}' không đủ."
                        );
                        TempData["Error"] =
                            $"Số lượng tồn kho của hàng hóa '{hangHoa.TenHangHoa}' không đủ.";
                        return Page();
                    }

                    // Cập nhật tồn kho
                    hangHoa.SoLuongConLai -= soLuongXuat;
                    hangHoa.SoLuongDaXuat += soLuongXuat;
                    _dbContext.HangHoas.Update(hangHoa);

                    // Tính tổng giá trị hàng hóa xuất
                    var tienHangTruocThue = soLuongXuat * hangHoa.DonGiaTruocThue;
                    tongThanhTien += tienHangTruocThue;

                    // Tạo chi tiết hóa đơn
                    var hangHoaHoaDon = new HangHoaHoaDon
                    {
                        Id = Guid.NewGuid(),
                        TenHangHoa = hangHoa.TenHangHoa,
                        DonViTinhId = hangHoa.DonViTinhId,
                        SoLuong = soLuongXuat,
                        DonGiaTruocThue = hangHoa.DonGiaTruocThue,
                        TongGiaTruocThue = tienHangTruocThue,
                        KhoHangId = KhoHangId,
                        NhomHangId = hangHoa.NhomHangId
                    };
                    _dbContext.HangHoaHoaDons.Add(hangHoaHoaDon);

                    // Tạo bản ghi xuất kho
                    var xuatKho = new XuatKho
                    {
                        XuatKhoId = Guid.NewGuid(),
                        HoaDonXuatId = hoaDonXuat.Id,
                        HangHoaHoaDonId = hangHoaHoaDon.Id
                    };
                    _dbContext.XuatKhos.Add(xuatKho);
                }

                // Cập nhật tổng thành tiền vào hóa đơn
                hoaDonXuat.ThanhTien = tongThanhTien;

                // Lưu thay đổi vào cơ sở dữ liệu
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Xuất dùng hàng hóa thành công!";
                return RedirectToPage("./XuatHangHoa");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return Page();
            }
        }
    }
}
