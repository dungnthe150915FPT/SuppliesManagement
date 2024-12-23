namespace SuppliesManagement.Models.Request
{
    public class HangHoaInputModel
    {
        public Guid Id { get; set; }
        public string TenHangHoa { get; set; }
        public int NhomHangID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaTruocThue { get; set; }
        public int DonViTinhID { get; set; }
        public int VAT { get; set; }
        public byte[]? Image { get; set; }
        public IFormFile ImageFile { get; set; }
        /*        public string TenNhomHang { get; set; }
                public string TenDonViTinh { get; set; }*/
    }
}
