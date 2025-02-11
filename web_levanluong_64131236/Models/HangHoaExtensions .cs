using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_levanluong_64131236.Models
{
    public static class HangHoaExtensions
    {
        public static int GetSoldQuantity(this HangHoa hangHoa, webbanhang64131236Entities1 db)
        {
            return db.ChiTietDonHangs
                .Where(ct => ct.MaHH == hangHoa.MaHH && ct.DonHang.TrangThai == "Đã xác nhận")
                .Sum(ct => ct.SoLuong);
        }
    }
}