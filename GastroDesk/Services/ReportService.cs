using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GastroDesk.Data;
using GastroDesk.Models;
using GastroDesk.Models.Enums;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.Services
{
    public class ReportService : IReportService
    {
        private readonly DbContextFactory _dbContextFactory;

        public ReportService()
        {
            _dbContextFactory = DbContextFactory.Instance;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<DailyRevenueReport> GetDailyRevenueAsync(DateTime date)
        {
            using var context = _dbContextFactory.CreateContext();

            var orders = await context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                .Where(o => o.OrderDateTime.Date == date.Date)
                .ToListAsync();

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

            var report = new DailyRevenueReport
            {
                Date = date,
                TotalOrders = orders.Count,
                CompletedOrders = completedOrders.Count,
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = completedOrders.Sum(o => o.Items.Sum(i => i.Price * i.Quantity)),
                Orders = orders.Select(o => new OrderSummary
                {
                    OrderId = o.Id,
                    TableNumber = o.TableNumber,
                    OrderDateTime = o.OrderDateTime,
                    Total = o.Items.Sum(i => i.Price * i.Quantity),
                    Status = o.Status.ToString(),
                    WaiterName = o.User?.FullName ?? "Unknown"
                }).ToList()
            };

            report.TopDishes = await GetTopDishesAsync(context, date, date);
            return report;
        }

        public async Task<WeeklyRevenueReport> GetWeeklyRevenueAsync(DateTime startDate)
        {
            var endDate = startDate.AddDays(6);

            var report = new WeeklyRevenueReport
            {
                StartDate = startDate,
                EndDate = endDate
            };

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var dailyReport = await GetDailyRevenueAsync(day);
                report.DailyReports.Add(dailyReport);
            }

            report.TotalOrders = report.DailyReports.Sum(r => r.CompletedOrders);
            report.TotalRevenue = report.DailyReports.Sum(r => r.TotalRevenue);

            return report;
        }

        private async Task<List<DishSalesSummary>> GetTopDishesAsync(AppDbContext context, DateTime startDate, DateTime endDate)
        {
            var orderItems = await context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Dish)
                .Where(oi => oi.Order!.OrderDateTime.Date >= startDate.Date &&
                             oi.Order.OrderDateTime.Date <= endDate.Date &&
                             oi.Order.Status == OrderStatus.Completed)
                .ToListAsync();

            return orderItems
                .GroupBy(oi => new { oi.DishId, DishName = oi.Dish?.Name ?? "Unknown" })
                .Select(g => new DishSalesSummary
                {
                    DishName = g.Key.DishName,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(d => d.QuantitySold)
                .Take(10)
                .ToList();
        }

        public async Task<byte[]> GenerateDailyReportPdfAsync(DateTime date)
        {
            var report = await GetDailyRevenueAsync(date);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Daily Report - {date:yyyy-MM-dd}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(10);

                            // Summary section
                            col.Item().Text("Summary").SemiBold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Total Orders:");
                                table.Cell().Text(report.TotalOrders.ToString());

                                table.Cell().Text("Completed Orders:");
                                table.Cell().Text(report.CompletedOrders.ToString());

                                table.Cell().Text("Cancelled Orders:");
                                table.Cell().Text(report.CancelledOrders.ToString());

                                table.Cell().Text("Total Revenue:");
                                table.Cell().Text($"${report.TotalRevenue:F2}");
                            });

                            // Orders table
                            col.Item().PaddingTop(20).Text("Orders").SemiBold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(50);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(70);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Table").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Waiter").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Status").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total").SemiBold();
                                });

                                foreach (var order in report.Orders)
                                {
                                    table.Cell().Padding(5).Text(order.OrderId.ToString());
                                    table.Cell().Padding(5).Text(order.TableNumber.ToString());
                                    table.Cell().Padding(5).Text(order.WaiterName);
                                    table.Cell().Padding(5).Text(order.Status);
                                    table.Cell().Padding(5).Text($"${order.Total:F2}");
                                }
                            });

                            // Top dishes
                            if (report.TopDishes.Any())
                            {
                                col.Item().PaddingTop(20).Text("Top Selling Dishes").SemiBold().FontSize(14);
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(80);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Dish").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qty Sold").SemiBold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Revenue").SemiBold();
                                    });

                                    foreach (var dish in report.TopDishes)
                                    {
                                        table.Cell().Padding(5).Text(dish.DishName);
                                        table.Cell().Padding(5).Text(dish.QuantitySold.ToString());
                                        table.Cell().Padding(5).Text($"${dish.TotalRevenue:F2}");
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            x.Span(" - GastroDesk");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateWeeklyReportPdfAsync(DateTime startDate)
        {
            var report = await GetWeeklyRevenueAsync(startDate);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Weekly Report - {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(10);

                            // Summary
                            col.Item().Text("Weekly Summary").SemiBold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Total Orders:");
                                table.Cell().Text(report.TotalOrders.ToString());

                                table.Cell().Text("Total Revenue:");
                                table.Cell().Text($"${report.TotalRevenue:F2}");

                                table.Cell().Text("Average Daily Revenue:");
                                table.Cell().Text($"${(report.TotalRevenue / 7):F2}");
                            });

                            // Daily breakdown
                            col.Item().PaddingTop(20).Text("Daily Breakdown").SemiBold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Orders").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Completed").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Revenue").SemiBold();
                                });

                                foreach (var daily in report.DailyReports)
                                {
                                    table.Cell().Padding(5).Text(daily.Date.ToString("ddd, MMM dd"));
                                    table.Cell().Padding(5).Text(daily.TotalOrders.ToString());
                                    table.Cell().Padding(5).Text(daily.CompletedOrders.ToString());
                                    table.Cell().Padding(5).Text($"${daily.TotalRevenue:F2}");
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            x.Span(" - GastroDesk");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<string> ExportMenuToJsonAsync()
        {
            using var context = _dbContextFactory.CreateContext();

            var menu = new MenuExport
            {
                ExportDate = DateTime.Now,
                Categories = await context.Categories
                    .Include(c => c.Dishes.Where(d => d.IsActive))
                    .ToListAsync()
            };

            return JsonSerializer.Serialize(menu, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public async Task<string> ExportMenuToXmlAsync()
        {
            using var context = _dbContextFactory.CreateContext();

            var menu = new MenuExport
            {
                ExportDate = DateTime.Now,
                Categories = await context.Categories
                    .Include(c => c.Dishes.Where(d => d.IsActive))
                    .ToListAsync()
            };

            var serializer = new XmlSerializer(typeof(MenuExport));
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, menu);
            return stringWriter.ToString();
        }

        public async Task ImportMenuFromJsonAsync(string json)
        {
            var menu = JsonSerializer.Deserialize<MenuExport>(json);
            if (menu?.Categories != null)
            {
                await ImportMenuAsync(menu.Categories);
            }
        }

        public async Task ImportMenuFromXmlAsync(string xml)
        {
            var serializer = new XmlSerializer(typeof(MenuExport));
            using var stringReader = new StringReader(xml);
            var menu = (MenuExport?)serializer.Deserialize(stringReader);
            if (menu?.Categories != null)
            {
                await ImportMenuAsync(menu.Categories);
            }
        }

        private async Task ImportMenuAsync(List<Category> categories)
        {
            using var context = _dbContextFactory.CreateContext();

            foreach (var category in categories)
            {
                var existingCategory = await context.Categories
                    .FirstOrDefaultAsync(c => c.Name == category.Name);

                if (existingCategory == null)
                {
                    existingCategory = new Category
                    {
                        Name = category.Name,
                        Description = category.Description,
                        CreatedAt = DateTime.Now
                    };
                    context.Categories.Add(existingCategory);
                    await context.SaveChangesAsync();
                }

                foreach (var dish in category.Dishes)
                {
                    var existingDish = await context.Dishes
                        .FirstOrDefaultAsync(d => d.Name == dish.Name && d.CategoryId == existingCategory.Id);

                    if (existingDish == null)
                    {
                        context.Dishes.Add(new Dish
                        {
                            Name = dish.Name,
                            Description = dish.Description,
                            Price = dish.Price,
                            CategoryId = existingCategory.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<string> ExportDishesToJsonAsync(int categoryId)
        {
            using var context = _dbContextFactory.CreateContext();

            var category = await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found");

            var export = new DishesExport
            {
                ExportDate = DateTime.Now,
                CategoryName = category.Name,
                Dishes = category.Dishes.Select(d => new DishExportItem
                {
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    IsActive = d.IsActive
                }).ToList()
            };

            return JsonSerializer.Serialize(export, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public async Task<string> ExportDishesToXmlAsync(int categoryId)
        {
            using var context = _dbContextFactory.CreateContext();

            var category = await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found");

            var export = new DishesExport
            {
                ExportDate = DateTime.Now,
                CategoryName = category.Name,
                Dishes = category.Dishes.Select(d => new DishExportItem
                {
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    IsActive = d.IsActive
                }).ToList()
            };

            var serializer = new XmlSerializer(typeof(DishesExport));
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, export);
            return stringWriter.ToString();
        }

        public async Task ImportDishesFromJsonAsync(string json, int categoryId)
        {
            var export = JsonSerializer.Deserialize<DishesExport>(json);
            if (export?.Dishes != null)
            {
                await ImportDishesAsync(export.Dishes, categoryId);
            }
        }

        public async Task ImportDishesFromXmlAsync(string xml, int categoryId)
        {
            var serializer = new XmlSerializer(typeof(DishesExport));
            using var stringReader = new StringReader(xml);
            var export = (DishesExport?)serializer.Deserialize(stringReader);
            if (export?.Dishes != null)
            {
                await ImportDishesAsync(export.Dishes, categoryId);
            }
        }

        private async Task ImportDishesAsync(List<DishExportItem> dishes, int categoryId)
        {
            using var context = _dbContextFactory.CreateContext();

            var category = await context.Categories.FindAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            foreach (var dish in dishes)
            {
                var existingDish = await context.Dishes
                    .FirstOrDefaultAsync(d => d.Name == dish.Name && d.CategoryId == categoryId);

                if (existingDish == null)
                {
                    context.Dishes.Add(new Dish
                    {
                        Name = dish.Name,
                        Description = dish.Description,
                        Price = dish.Price,
                        CategoryId = categoryId,
                        IsActive = dish.IsActive,
                        CreatedAt = DateTime.Now
                    });
                }
                else
                {
                    existingDish.Description = dish.Description;
                    existingDish.Price = dish.Price;
                    existingDish.IsActive = dish.IsActive;
                    existingDish.UpdatedAt = DateTime.Now;
                }
            }

            await context.SaveChangesAsync();
        }
    }

    public class MenuExport
    {
        public DateTime ExportDate { get; set; }
        public List<Category> Categories { get; set; } = new();
    }

    public class DishesExport
    {
        public DateTime ExportDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<DishExportItem> Dishes { get; set; } = new();
    }

    public class DishExportItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
