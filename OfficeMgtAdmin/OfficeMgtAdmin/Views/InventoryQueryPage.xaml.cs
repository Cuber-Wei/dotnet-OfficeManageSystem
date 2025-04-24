using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;
using System.Threading;

namespace OfficeMgtAdmin.Views
{
    public partial class InventoryQueryPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<Item> _items;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public InventoryQueryPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _items = new ObservableCollection<Item>();
            ItemsCollection.ItemsSource = _items;
            LoadItems();
        }

        private async void LoadItems(int? itemType = null)
        {
            try
            {
                var query = _context.Items.Where(i => !i.IsDelete);
                if (itemType.HasValue)
                {
                    query = query.Where(i => i.ItemType == itemType.Value);
                }

                var items = await query.ToListAsync();
                _items.Clear();
                foreach (var item in items)
                {
                    _items.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载物品数据失败: {ex.Message}", "确定");
            }
        }

        private void OnQueryClicked(object sender, EventArgs e)
        {
            int? selectedType = TypePicker.SelectedIndex > 0 ? TypePicker.SelectedIndex - 1 : null;
            LoadItems(selectedType);
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;
            
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    // 获取物品的新实例，使用 AsNoTracking 避免跟踪冲突
                    var freshItem = await _context.Items
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == itemId);
                    
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

        private async void OnViewRecordsClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;
            await Navigation.PushAsync(new ItemRecordPage(_context, itemId));
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