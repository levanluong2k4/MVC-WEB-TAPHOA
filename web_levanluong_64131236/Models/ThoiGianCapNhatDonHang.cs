//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace web_levanluong_64131236.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ThoiGianCapNhatDonHang
    {
        public int MaCapNhat { get; set; }
        public int MaDH { get; set; }
        public string TrangThai { get; set; }
        public string TrangThaiGiaoHang { get; set; }
        public System.DateTime ThoiGianCapNhat { get; set; }
    
        public virtual DonHang DonHang { get; set; }
    }
}
