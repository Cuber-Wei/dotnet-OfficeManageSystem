using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class InventoryQueryPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<Item> _items;

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
            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                var message = $"物品编码: {item.Code}\n" +
                             $"物品名称: {item.ItemName}\n" +
                             $"物品类别: {GetItemTypeName(item.ItemType)}\n" +
                             $"产地: {item.Origin ?? "未设置"}\n" +
                             $"规格: {item.ItemSize ?? "未设置"}\n" +
                             $"型号: {item.ItemVersion ?? "未设置"}\n" +
                             $"当前库存: {item.ItemNum}\n" +
                             $"创建时间: {item.CreateTime:yyyy-MM-dd HH:mm:ss}\n" +
                             $"更新时间: {item.UpdateTime:yyyy-MM-dd HH:mm:ss}";

                await DisplayAlert($"物品详情 - {item.ItemName}", message, "确定");
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