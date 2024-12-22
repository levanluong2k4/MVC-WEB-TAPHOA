using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_levanluong_64131236.Models
{
    public class MonthlyRevenueViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal GrowthRate { get; set; } // Tỷ lệ tăng trưởng so với tháng trước
    }
}