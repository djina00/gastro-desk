using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using GastroDesk.Commands;
using GastroDesk.Services.Interfaces;

namespace GastroDesk.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                _ = LoadDailyReportAsync();
            }
        }

        private DateTime _weekStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
        public DateTime WeekStartDate
        {
            get => _weekStartDate;
            set
            {
                SetProperty(ref _weekStartDate, value);
                _ = LoadWeeklyReportAsync();
            }
        }

        private DailyRevenueReport? _dailyReport;
        public DailyRevenueReport? DailyReport
        {
            get => _dailyReport;
            set => SetProperty(ref _dailyReport, value);
        }

        private WeeklyRevenueReport? _weeklyReport;
        public WeeklyRevenueReport? WeeklyReport
        {
            get => _weeklyReport;
            set => SetProperty(ref _weeklyReport, value);
        }

        private bool _showDailyReport = true;
        public bool ShowDailyReport
        {
            get => _showDailyReport;
            set
            {
                SetProperty(ref _showDailyReport, value);
                if (value) _ = LoadDailyReportAsync();
            }
        }

        private bool _showWeeklyReport;
        public bool ShowWeeklyReport
        {
            get => _showWeeklyReport;
            set
            {
                SetProperty(ref _showWeeklyReport, value);
                if (value) _ = LoadWeeklyReportAsync();
            }
        }

        public ICommand LoadDailyReportCommand { get; }
        public ICommand LoadWeeklyReportCommand { get; }
        public ICommand ExportDailyPdfCommand { get; }
        public ICommand ExportWeeklyPdfCommand { get; }
        public ICommand SwitchToDailyCommand { get; }
        public ICommand SwitchToWeeklyCommand { get; }

        public ReportViewModel(IReportService reportService)
        {
            _reportService = reportService;

            LoadDailyReportCommand = new AsyncRelayCommand(LoadDailyReportAsync);
            LoadWeeklyReportCommand = new AsyncRelayCommand(LoadWeeklyReportAsync);
            ExportDailyPdfCommand = new AsyncRelayCommand(ExportDailyPdfAsync);
            ExportWeeklyPdfCommand = new AsyncRelayCommand(ExportWeeklyPdfAsync);
            SwitchToDailyCommand = new RelayCommand(() => { ShowDailyReport = true; ShowWeeklyReport = false; });
            SwitchToWeeklyCommand = new RelayCommand(() => { ShowDailyReport = false; ShowWeeklyReport = true; });

            _ = LoadDailyReportAsync();
        }

        private async Task LoadDailyReportAsync()
        {
            IsLoading = true;
            ClearError();

            try
            {
                DailyReport = await _reportService.GetDailyRevenueAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                SetError($"Error loading daily report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWeeklyReportAsync()
        {
            IsLoading = true;
            ClearError();

            try
            {
                WeeklyReport = await _reportService.GetWeeklyRevenueAsync(WeekStartDate);
            }
            catch (Exception ex)
            {
                SetError($"Error loading weekly report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportDailyPdfAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"daily_report_{SelectedDate:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    var pdf = await _reportService.GenerateDailyReportPdfAsync(SelectedDate);
                    await File.WriteAllBytesAsync(dialog.FileName, pdf);

                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                SetError($"Error exporting PDF: {ex.Message}");
            }
        }

        private async Task ExportWeeklyPdfAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"weekly_report_{WeekStartDate:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    var pdf = await _reportService.GenerateWeeklyReportPdfAsync(WeekStartDate);
                    await File.WriteAllBytesAsync(dialog.FileName, pdf);

                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                SetError($"Error exporting PDF: {ex.Message}");
            }
        }
    }
}
