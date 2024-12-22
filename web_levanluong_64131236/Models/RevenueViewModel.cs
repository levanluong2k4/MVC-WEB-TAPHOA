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

        // Doanh thu theo ngày
        public List<DailyRevenueViewModel> DailyRevenue { get; set; }

        // Doanh thu theo tháng
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; }

        // Doanh thu theo năm
        public List<YearlyRevenueViewModel> YearlyRevenue { get; set; }

        // Sản phẩm bán chạy
        public List<BestSellingProductViewModel> BestSellingProducts { get; set; }
    }
}