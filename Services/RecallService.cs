// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using SuppliesManagement.Models;

// namespace SuppliesManagement.Services
// {
//     public class RecallService
//     {
//         private readonly SuppliesManagementProjectContext _context;

//         public RecallService(SuppliesManagementProjectContext context)
//         {
//             _context = context;
//         }

//         public async Task RecallHoaDonXuat(Guid hoaDonXuatId)
//         {
//             using var transaction = await _context.Database.BeginTransactionAsync();

//             try
//             {
//                 // Get the XuatKhos related to the HoaDonXuat
//                 var xuatKhos = await _context.XuatKhos
//                     .Include(x => x.HangHoaHoaDon)
//                     .Where(x => x.HoaDonXuatId == hoaDonXuatId)
//                     .ToListAsync();

//                 if (!xuatKhos.Any())
//                 {
//                     throw new Exception("No XuatKhos found for this HoaDonXuat");
//                 }

//                 // Update HangHoa quantities
//                 foreach (var xuatKho in xuatKhos)
//                 {
//                     var hangHoa = await _context.HangHoas.FindAsync(xuatKho.HangHoaHoaDon.Id);
//                     if (hangHoa != null)
//                     {
//                         hangHoa.SoLuongDaXuat -= xuatKho.HangHoaHoaDon.SoLuong;
//                         hangHoa.SoLuongConLai += xuatKho.HangHoaHoaDon.SoLuong;
//                         _context.HangHoas.Update(hangHoa);
//                     }
//                 }

//                 // Remove HangHoaHoaDons
//                 var hangHoaHoaDonIds = xuatKhos.Select(x => x.HangHoaHoaDonId).Distinct().ToList();
//                 var hangHoaHoaDons = await _context.HangHoaHoaDons
//                     .Where(h => hangHoaHoaDonIds.Contains(h.Id))
//                     .ToListAsync();
//                 _context.HangHoaHoaDons.RemoveRange(hangHoaHoaDons);

//                 // Remove XuatKhos
//                 _context.XuatKhos.RemoveRange(xuatKhos);

//                 // Remove HoaDonXuat
//                 var hoaDonXuat = await _context.HoaDonXuats.FindAsync(hoaDonXuatId);
//                 if (hoaDonXuat != null)
//                 {
//                     _context.HoaDonXuats.Remove(hoaDonXuat);
//                 }

//                 await _context.SaveChangesAsync();
//                 await transaction.CommitAsync();
//             }
//             catch (Exception)
//             {
//                 await transaction.RollbackAsync();
//                 throw;
//             }
//         }
//     }
// }
