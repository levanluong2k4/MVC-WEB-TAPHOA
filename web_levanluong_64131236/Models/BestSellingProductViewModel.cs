using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_levanluong_64131236.Models
{
    public class BestSellingProductViewModel
    {
        public string MaHH { get; set; }
        public string TenHH { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public string AnhHH { get; set; }

        // New analytics properties
        public decimal AverageOrderQuantity { get; set; }
        public decimal AverageUnitPrice { get; set; }
        public int TotalOrders { get; set; }
        public DateTime FirstSaleDate { get; set; }
        public DateTime LastSaleDate { get; set; }
        public decimal DailyAverageSales
        {
            get
            {
                if (FirstSaleDate == default(DateTime) || LastSaleDate == default(DateTime))
                    return 0;

                var daysDifference = (LastSaleDate - FirstSaleDate).Days + 1;
                return daysDifference > 0 ? (decimal)TotalQuantity / daysDifference : 0;
            }
        }

        // Growth metrics
        public decimal RevenueGrowthRate { get; set; }
        public decimal QuantityGrowthRate { get; set; }

        // Sales distribution
        public Dictionary<string, int> SalesByMonth { get; set; }
        public Dictionary<DayOfWeek, int> SalesByDayOfWeek { get; set; }

        public BestSellingProductViewModel()
        {
            SalesByMonth = new Dictionary<string, int>();
            SalesByDayOfWeek = new Dictionary<DayOfWeek, int>();
        }

        // Helper method to calculate revenue per day
        public decimal GetRevenuePerDay()
        {
            if (FirstSaleDate == default(DateTime) || LastSaleDate == default(DateTime))
                return 0;

            var daysDifference = (LastSaleDate - FirstSaleDate).Days + 1;
            return daysDifference > 0 ? TotalRevenue / daysDifference : 0;
        }

        // Helper method to get sales trend
        public string GetSalesTrend()
        {
            if (QuantityGrowthRate > 10)
                return "Tăng mạnh";
            else if (QuantityGrowthRate > 0)
                return "Tăng nhẹ";
            else if (QuantityGrowthRate == 0)
                return "Ổn định";
            else if (QuantityGrowthRate > -10)
                return "Giảm nhẹ";
            else
                return "Giảm mạnh";
        }

        // Helper method to get peak sales day
        public DayOfWeek GetPeakSalesDay()
        {
            return SalesByDayOfWeek.OrderByDescending(x => x.Value)
                                  .First()
                                  .Key;
        }

        // Helper method to get best performing month
        public string GetBestPerformingMonth()
        {
            return SalesByMonth.OrderByDescending(x => x.Value)
                              .First()
                              .Key;
        }
    }
}