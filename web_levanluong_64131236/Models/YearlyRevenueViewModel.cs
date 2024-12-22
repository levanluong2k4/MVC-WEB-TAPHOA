using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_levanluong_64131236.Models
{
    public class YearlyRevenueViewModel
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal GrowthRate { get; set; } // Tỷ lệ tăng trưởng so với năm trước
    }
}