using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using OfficeMgtAdmin.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemExportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly string _userJsonPath;
        private readonly ObservableCollection<ApplyRecordViewModel> _applyRecords;
        private readonly List<User> _users;
        private readonly List<ApplyRecordViewModel> _allRecords;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _hasNoRecords;

        public bool HasNoRecords
        {
            get => _hasNoRecords;
            set
            {
                if (_hasNoRecords != value)
                {
                    _hasNoRecords = value;
                    OnPropertyChanged(nameof(HasNoRecords));
                }
            }
        }

        public ICommand DetailCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand RejectCommand { get; }

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
            HasNoRecords = true; // 初始状态为无记录

            // 初始化命令
            DetailCommand = new Command<Item>(async (item) => await OnDetailClicked(item));
            ConfirmCommand = new Command<Item>(async (item) => await OnConfirmClicked(item));
            RejectCommand = new Command<Item>(async (item) => await OnRejectClicked(item));

            // 设置绑定上下文
            BindingContext = this;

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
                    _users.Clear();
                    _users.AddRange(JsonSerializer.Deserialize<List<User>>(userJson) ?? new List<User>());
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"加载用户数据失败: {ex.Message}", "确定");
            }
        }

        private User? FindUser(long userId)
        {
            return _users.FirstOrDefault(u => u.Id == userId);
        }

        private async void LoadApplyRecords()
        {
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    var records = await _context.ApplyRecords
                        .AsNoTracking()
                        .Include(r => r.Item)
                        .Where(r => !r.IsDelete)
                        .OrderByDescending(r => r.ApplyDate)
                        .ToListAsync();

                    _allRecords.Clear();
                    foreach (var record in records)
                    {
                        // 获取最新的入库记录以获取价格
                        var latestImport = await _context.ImportRecords
                            .Where(i => i.ItemId == record.ItemId && !i.IsDelete)
                            .OrderByDescending(i => i.ImportDate)
                            .FirstOrDefaultAsync();

                        var user = FindUser(record.UserId);
                        _allRecords.Add(new ApplyRecordViewModel(record, user, latestImport?.SinglePrice ?? 0));
                    }

                    FilterRecords();
                }
                finally
                {
                    _semaphore.Release();
                }
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

            // 更新无记录状态
            HasNoRecords = _applyRecords.Count == 0;
        }

        private void OnStatusFilterChanged(object sender, EventArgs e)
        {
            FilterRecords();
        }

        private async Task OnConfirmClicked(Item item)
        {
            if (item == null)
            {
                await DisplayAlert("错误", "找不到物品信息", "确定");
                return;
            }

            if (item.ItemNum < 0)
            {
                await DisplayAlert("提示", "库存数量不能为负数", "确定");
                item.ItemNum = 0;
                return;
            }

            if (item.ItemNum < 1)
            {
                await DisplayAlert("提示", "库存不足", "确定");
                return;
            }

            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    // 从数据库获取最新数据
                    var freshItem = await _context.Items.FindAsync(item.Id);
                    if (freshItem == null)
                    {
                        await DisplayAlert("错误", "找不到物品信息", "确定");
                        return;
                    }

                    // 获取并更新申请记录
                    var applyRecord = await _context.ApplyRecords
                        .FirstOrDefaultAsync(r => r.ItemId == item.Id && r.ApplyStatus == 0);
                    
                    if (applyRecord == null)
                    {
                        await DisplayAlert("错误", "找不到待处理的申请记录", "确定");
                        return;
                    }

                    // 更新申请记录状态为已通过
                    applyRecord.ApplyStatus = 1;
                    applyRecord.UpdateTime = DateTime.Now;

                    // 更新物品库存
                    freshItem.ItemNum -= applyRecord.ApplyNum;
                    freshItem.UpdateTime = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await DisplayAlert("提示", "已通过", "确定");
                }
                finally
                {
                    _semaphore.Release();
                }

                // 刷新列表（在释放信号量后进行）
                LoadApplyRecords();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"确认失败: {ex.Message}", "确定");
            }
        }

        private async Task OnRejectClicked(Item item)
        {
            if (item == null)
            {
                await DisplayAlert("错误", "找不到物品信息", "确定");
                return;
            }

            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    // 从数据库获取最新数据
                    var freshItem = await _context.Items.FindAsync(item.Id);
                    if (freshItem == null)
                    {
                        await DisplayAlert("错误", "找不到物品信息", "确定");
                        return;
                    }

                    // 获取并更新申请记录
                    var applyRecord = await _context.ApplyRecords
                        .FirstOrDefaultAsync(r => r.ItemId == item.Id && r.ApplyStatus == 0);
                    
                    if (applyRecord == null)
                    {
                        await DisplayAlert("错误", "找不到待处理的申请记录", "确定");
                        return;
                    }

                    // 更新申请记录状态为已驳回
                    applyRecord.ApplyStatus = 2;
                    applyRecord.UpdateTime = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await DisplayAlert("提示", "已驳回", "确定");
                }
                finally
                {
                    _semaphore.Release();
                }

                // 刷新列表（在释放信号量后进行）
                LoadApplyRecords();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"驳回失败: {ex.Message}", "确定");
            }
        }

        private async Task OnDetailClicked(Item item)
        {
            if (item == null)
            {
                await DisplayAlert("错误", "无法获取物品信息", "确定");
                return;
            }

            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    // 获取物品的新实例，使用 AsNoTracking 避免跟踪冲突
                    var freshItem = await _context.Items
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == item.Id);
                    
                    if (freshItem == null)
                    {
                        await DisplayAlert("错误", "找不到物品信息", "确定");
                        return;
                    }

                    var detailPage = new ItemDetailPage(_context, freshItem);
                    await Navigation.PushAsync(detailPage);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"导航到详情页面时发生错误：{ex.Message}", "确定");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadApplyRecords();
        }
    }
} 