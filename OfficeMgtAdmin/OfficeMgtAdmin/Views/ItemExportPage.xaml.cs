using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using OfficeMgtAdmin.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemExportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly string _userJsonPath;
        private ObservableCollection<ApplyRecordViewModel> _applyRecords;
        private List<User> _users;
        private List<ApplyRecordViewModel> _allRecords;

        public ItemExportPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
            _applyRecords = new ObservableCollection<ApplyRecordViewModel>();
            _allRecords = new List<ApplyRecordViewModel>();
            _users = new List<User>();
            ApplyRecordsCollection.ItemsSource = _applyRecords;
            StatusPicker.SelectedIndex = 0; // 默认选择"全部"
            LoadUsers();
            LoadApplyRecords();
        }

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_userJsonPath))
                {
                    var userJson = File.ReadAllText(_userJsonPath);
                    _users = JsonSerializer.Deserialize<List<User>>(userJson) ?? new List<User>();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"加载用户数据失败: {ex.Message}", "确定");
            }
        }

        private User? FindUser(long userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            return user;
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

                _allRecords.Clear();
                foreach (var record in records)
                {
                    var user = FindUser(record.UserId);
                    _allRecords.Add(new ApplyRecordViewModel(record, user));
                }

                FilterRecords();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载申请记录失败: {ex.Message}", "确定");
            }
        }

        private void FilterRecords()
        {
            if (_allRecords == null) return;

            var filteredRecords = StatusPicker.SelectedIndex switch
            {
                1 => _allRecords.Where(r => r.ApplyStatus == 0), // 待审批
                2 => _allRecords.Where(r => r.ApplyStatus == 1), // 已通过
                3 => _allRecords.Where(r => r.ApplyStatus == 2), // 已驳回
                _ => _allRecords // 全部
            };

            _applyRecords.Clear();
            foreach (var record in filteredRecords)
            {
                _applyRecords.Add(record);
            }
        }

        private void OnStatusFilterChanged(object sender, EventArgs e)
        {
            FilterRecords();
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            try
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

                // 检查用户是否存在
                var user = FindUser(record.UserId);
                if (user == null)
                {
                    await DisplayAlert("错误", "找不到申请用户信息", "确定");
                    return;
                }

                record.ApplyStatus = 1; // 确认
                record.Item.ItemNum -= record.ApplyNum;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已确认", "确定");
                LoadApplyRecords(); // 重新加载申请记录
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"确认失败: {ex.Message}", "确定");
            }
        }

        private async void OnRejectClicked(object sender, EventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var recordId = (long)button.CommandParameter;

                var record = await _context.ApplyRecords
                    .Include(r => r.Item)
                    .FirstOrDefaultAsync(r => r.Id == recordId);

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

                // 检查用户是否存在
                var user = FindUser(record.UserId);
                if (user == null)
                {
                    await DisplayAlert("错误", "找不到申请用户信息", "确定");
                    return;
                }

                record.ApplyStatus = 2; // 驳回

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "已驳回", "确定");
                LoadApplyRecords(); // 重新加载申请记录
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"驳回失败: {ex.Message}", "确定");
            }
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var applyId = (long)button.CommandParameter;
            var applyRecordVM = _applyRecords.FirstOrDefault(r => r.Id == applyId);

            if (applyRecordVM?.Item != null)
            {
                var message = $"物品编码: {applyRecordVM.Item.Code}\n" +
                             $"物品名称: {applyRecordVM.Item.ItemName}\n" +
                             $"物品类别: {GetItemTypeName(applyRecordVM.Item.ItemType)}\n" +
                             $"申请数量: {applyRecordVM.ApplyNum}\n" +
                             $"申请状态: {GetStatusName(applyRecordVM.ApplyStatus)}\n" +
                             $"申请时间: {applyRecordVM.ApplyDate:yyyy-MM-dd HH:mm:ss}\n" +
                             $"申请人: {applyRecordVM.UserName}\n" +
                             $"更新时间: {applyRecordVM.UpdateTime:yyyy-MM-dd HH:mm:ss}";

                await DisplayAlert($"申请详情", message, "确定");
            }
        }

        private string GetItemTypeName(int itemType)
        {
            return itemType switch
            {
                0 => "纸张",
                1 => "文具",
                2 => "刀具",
                3 => "单据",
                4 => "礼品",
                5 => "其它",
                _ => "未知"
            };
        }

        private string GetStatusName(int status)
        {
            return status switch
            {
                0 => "待审批",
                1 => "已通过",
                2 => "已驳回",
                _ => "未知"
            };
        }
    }
} 