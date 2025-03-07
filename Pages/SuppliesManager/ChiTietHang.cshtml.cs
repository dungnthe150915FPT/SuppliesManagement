using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class ChiTietHangModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public ChiTietHangModel(
            SuppliesManagementProjectContext dbContext,
            IWebHostEnvironment environment
        )
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        public HangHoa HangHoa { get; set; }
        public SelectList KhoHangList { get; set; }
        public SelectList NhomHangList { get; set; }
        public SelectList DonViTinhList { get; set; }

        [BindProperty]
        [Display(Name = "Image Upload")]
        public List<IFormFile> ImageUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            HangHoa = await _dbContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh)
                .Include(h => h.KhoHang)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (HangHoa == null)
            {
                return NotFound();
            }
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2 && role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            var vietnamCulture = new CultureInfo("vi-VN");
            HangHoa.DonGiaSauThue.ToString("N0", vietnamCulture);
            HangHoa.DonGiaTruocThue.ToString("N0", vietnamCulture);
            HangHoa.TongGiaSauThue.ToString("N0", vietnamCulture);
            HangHoa.TongGiaTruocThue.ToString("N0", vietnamCulture);

            await LoadSelectList();
            return Page();
        }

        private async Task LoadSelectList()
        {
            DonViTinhList = new SelectList(await _dbContext.DonViTinhs.ToListAsync(), "Id", "Name");
            KhoHangList = new SelectList(await _dbContext.KhoHangs.ToListAsync(), "Id", "Ten");
            NhomHangList = new SelectList(await _dbContext.NhomHangs.ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync(Guid id, HangHoa hangHoa)
        {
            /*            if (!ModelState.IsValid)
                        {
                            await PopulateDropdownLists();
                            return Page();
                        }*/
            if (id == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid HangHoa data.");
                await PopulateDropdownLists();
                return Page();
            }
            var hangHoaToUpdate = await _dbContext.HangHoas.FirstOrDefaultAsync(h => h.Id == id);
            if (hangHoaToUpdate == null)
            {
                ModelState.AddModelError(string.Empty, "HangHoa not found.");
                await PopulateDropdownLists();
                return Page();
            }
            if (hangHoaToUpdate == null)
            {
                return NotFound();
            }

            // Update the properties of hangHoaToUpdate
            hangHoaToUpdate.TenHangHoa = hangHoa.TenHangHoa;
            hangHoaToUpdate.DonViTinhId = hangHoa.DonViTinhId;
            hangHoaToUpdate.NhomHangId = hangHoa.NhomHangId;
            hangHoaToUpdate.DonGiaTruocThue = hangHoa.DonGiaTruocThue;
            hangHoaToUpdate.DonGiaSauThue = hangHoa.DonGiaSauThue;
            hangHoaToUpdate.Vat = hangHoa.Vat;
            hangHoaToUpdate.SoLuongDaXuat = hangHoa.SoLuongDaXuat;
            hangHoaToUpdate.SoLuong = hangHoa.SoLuong;
            hangHoaToUpdate.NgayNhap = hangHoa.NgayNhap;
            hangHoaToUpdate.KhoHangId = hangHoa.KhoHangId;

            // Recalculate totals
            hangHoaToUpdate.TongGiaTruocThue = hangHoa.TongGiaTruocThue;
            hangHoaToUpdate.TongGiaSauThue = hangHoa.TongGiaSauThue;
            hangHoaToUpdate.SoLuongConLai = hangHoaToUpdate.SoLuong - hangHoaToUpdate.SoLuongDaXuat;

            // Handle image uploads
            if (ImageUpload != null && ImageUpload.Count > 0)
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadPath);

                for (int i = 0; i < Math.Min(ImageUpload.Count, 3); i++)
                {
                    var file = ImageUpload[i];
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        byte[] imageData = await System.IO.File.ReadAllBytesAsync(filePath);
                        switch (i)
                        {
                            case 0:
                                hangHoaToUpdate.Image1 = imageData;
                                break;
                            case 1:
                                hangHoaToUpdate.Image2 = imageData;
                                break;
                            case 2:
                                hangHoaToUpdate.Image3 = imageData;
                                break;
                        }

                        System.IO.File.Delete(filePath);
                    }
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                TempData["SuccessChiTietHang"] = $"Chỉnh sửa thông tin hàng hóa thành công";
                return RedirectToPage("./ChiTietHang", new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbContext.HangHoas.AnyAsync(h => h.Id == hangHoa.Id))
                {
                    ModelState.AddModelError(string.Empty, "HangHoa no longer exists.");
                    await PopulateDropdownLists();
                    return Page();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task PopulateDropdownLists()
        {
            KhoHangList = new SelectList(await _dbContext.KhoHangs.ToListAsync(), "Id", "Name");
            NhomHangList = new SelectList(await _dbContext.NhomHangs.ToListAsync(), "Id", "Name");
            DonViTinhList = new SelectList(await _dbContext.DonViTinhs.ToListAsync(), "Id", "Name");
        }
    }
}
