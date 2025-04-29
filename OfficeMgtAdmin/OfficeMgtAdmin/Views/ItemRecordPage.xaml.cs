using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using OfficeMgtAdmin.Converters;
using OfficeMgtAdmin.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemRecordPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly long _itemId;
        private readonly string _userJsonPath;
        private readonly List<User> _users;
        private ObservableCollection<ImportRecord> _importRecords;
        private ObservableCollection<ApplyRecordViewModel> _applyRecords;

        public ItemRecordPage(ApplicationDbContext context, long itemId)
        {
            InitializeComponent();
            _context = context;
            _itemId = itemId;
            _userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
            _users = new List<User>();
            _importRecords = new ObservableCollection<ImportRecord>();
            _applyRecords = new ObservableCollection<ApplyRecordViewModel>();
            ImportRecordsCollection.ItemsSource = _importRecords;
            ApplyRecordsCollection.ItemsSource = _applyRecords;
            LoadUsers();
            LoadData();
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

        private async void LoadData()
        {
            try
            {
                var item = await _context.Items.FindAsync(_itemId);
                if (item != null)
                {
                    ItemInfoLabel.Text = $"{item.Code} - {item.ItemName}";
                    string itemType = GetItemTypeName(item.ItemType);
                    InventoryLabel.Text = $"物品类别: {itemType}    当前库存: {item.ItemNum}";

                    var importRecords = await _context.ImportRecords
                        .Where(r => r.ItemId == _itemId && !r.IsDelete)
                        .OrderByDescending(r => r.ImportDate)
                        .ToListAsync();

                    var applyRecords = await _context.ApplyRecords
                        .Where(r => r.ItemId == _itemId && !r.IsDelete)
                        .OrderByDescending(r => r.ApplyDate)
                        .ToListAsync();

                    _importRecords.Clear();
                    foreach (var record in importRecords)
                    {
                        _importRecords.Add(record);
                    }

                    _applyRecords.Clear();
                    foreach (var record in applyRecords)
                    {
                        // 获取最新的入库记录以获取价格
                        var latestImport = await _context.ImportRecords
                            .Where(i => i.ItemId == record.ItemId && !i.IsDelete)
                            .OrderByDescending(i => i.ImportDate)
                            .FirstOrDefaultAsync();

                        var user = FindUser(record.UserId);
                        _applyRecords.Add(new ApplyRecordViewModel(record, user, latestImport?.SinglePrice ?? 0));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载记录失败: {ex.Message}", "确定");
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
    }
} 