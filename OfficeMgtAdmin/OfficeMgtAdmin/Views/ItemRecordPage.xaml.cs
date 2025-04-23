using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using OfficeMgtAdmin.Converters;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemRecordPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly long _itemId;
        private ObservableCollection<ImportRecord> _importRecords;
        private ObservableCollection<ApplyRecord> _applyRecords;

        public ItemRecordPage(ApplicationDbContext context, long itemId)
        {
            InitializeComponent();
            _context = context;
            _itemId = itemId;
            _importRecords = new ObservableCollection<ImportRecord>();
            _applyRecords = new ObservableCollection<ApplyRecord>();
            ImportRecordsCollection.ItemsSource = _importRecords;
            ApplyRecordsCollection.ItemsSource = _applyRecords;
            LoadData();
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
                        _applyRecords.Add(record);
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