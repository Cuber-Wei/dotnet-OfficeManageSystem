using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemExportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<ApplyRecord> _applyRecords;

        public ItemExportPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _applyRecords = new ObservableCollection<ApplyRecord>();
            ApplyRecordsCollection.ItemsSource = _applyRecords;
            LoadApplyRecords();
        }

        private async void LoadApplyRecords()
        {
            var records = await _context.ApplyRecords
                .Include(r => r.Item)
                .Where(r => !r.IsDelete)
                .OrderByDescending(r => r.ApplyDate)
                .ToListAsync();

            _applyRecords.Clear();
            foreach (var record in records)
            {
                _applyRecords.Add(record);
            }
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var recordId = (long)button.CommandParameter;
            var record = await _context.ApplyRecords
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record != null && record.ApplyStatus == 0)
            {
                if (record.Item.ItemNum < record.ApplyNum)
                {
                    await DisplayAlert("提示", "库存不足", "确定");
                    return;
                }

                record.ApplyStatus = 1; // 确认
                record.Item.ItemNum -= record.ApplyNum;
                record.Item.UpdateTime = DateTime.Now;
                record.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已确认", "确定");
                LoadApplyRecords();
            }
        }

        private async void OnRejectClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var recordId = (long)button.CommandParameter;
            var record = await _context.ApplyRecords.FindAsync(recordId);

            if (record != null && record.ApplyStatus == 0)
            {
                record.ApplyStatus = 2; // 驳回
                record.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已驳回", "确定");
                LoadApplyRecords();
            }
        }
    }
} 