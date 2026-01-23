using GastroDesk.Models;

namespace GastroDesk.Services.Interfaces
{
    public interface IReportService
    {
        Task<DailyRevenueReport> GetDailyRevenueAsync(DateTime date);
        Task<WeeklyRevenueReport> GetWeeklyRevenueAsync(DateTime startDate);
        Task<byte[]> GenerateDailyReportPdfAsync(DateTime date);
        Task<byte[]> GenerateWeeklyReportPdfAsync(DateTime startDate);
        Task<string> ExportMenuToJsonAsync();
        Task<string> ExportMenuToXmlAsync();
        Task ImportMenuFromJsonAsync(string json);
        Task ImportMenuFromXmlAsync(string xml);
        Task<string> ExportDishesToJsonAsync(int categoryId);
        Task<string> ExportDishesToXmlAsync(int categoryId);
        Task ImportDishesFromJsonAsync(string json, int categoryId);
        Task ImportDishesFromXmlAsync(string xml, int categoryId);
    }

    public class DailyRevenueReport
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OrderSummary> Orders { get; set; } = new();
        public List<DishSalesSummary> TopDishes { get; set; } = new();
    }

    public class WeeklyRevenueReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DailyRevenueReport> DailyReports { get; set; } = new();
    }

    public class OrderSummary
    {
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderDateTime { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public string WaiterName { get; set; } = string.Empty;
    }

    public class DishSalesSummary
    {
        public string DishName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
