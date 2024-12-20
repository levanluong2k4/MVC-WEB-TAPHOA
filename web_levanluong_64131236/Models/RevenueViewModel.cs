using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_levanluong_64131236.Models
{
    public class RevenueViewModel
    {

        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }
        public List<BestSellingProductViewModel> BestSellingProducts { get; set; }
        public dynamic MonthlyRevenueData { get; set; }
    }
}