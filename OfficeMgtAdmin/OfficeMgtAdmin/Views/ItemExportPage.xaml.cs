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
            try
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
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载申请记录失败: {ex.Message}", "确定");
            }
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var recordId = (long)button.CommandParameter;
            var record = await _context.ApplyRecords
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record?.Item == null)
            {
                await DisplayAlert("错误", "找不到申请记录或物品信息", "确定");
                return;
            }

            if (record.ApplyStatus != 0)
            {
                await DisplayAlert("提示", "该申请已处理", "确定");
                return;
            }

            if (record.Item.ItemNum < record.ApplyNum)
            {
                await DisplayAlert("提示", "库存不足", "确定");
                return;
            }

            try
            {
                record.ApplyStatus = 1; // 确认
                record.Item.ItemNum -= record.ApplyNum;
                record.Item.UpdateTime = DateTime.Now;
                record.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已确认", "确定");
                LoadApplyRecords();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"确认失败: {ex.Message}", "确定");
            }
        }

        private async void OnRejectClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var recordId = (long)button.CommandParameter;
            var record = await _context.ApplyRecords.FindAsync(recordId);

            if (record == null)
            {
                await DisplayAlert("错误", "找不到申请记录", "确定");
                return;
            }

            if (record.ApplyStatus != 0)
            {
                await DisplayAlert("提示", "该申请已处理", "确定");
                return;
            }

            try
            {
                record.ApplyStatus = 2; // 驳回
                record.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已驳回", "确定");
                LoadApplyRecords();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"驳回失败: {ex.Message}", "确定");
            }
        }
    }
} 