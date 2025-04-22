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

        private void OnQueryClicked(object sender, EventArgs e)
        {
            int? selectedType = TypePicker.SelectedIndex > 0 ? TypePicker.SelectedIndex - 1 : null;
            LoadItems(selectedType);
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;
            var item = await _context.Items.FindAsync(itemId);
            
            if (item != null)
            {
                var importRecords = await _context.ImportRecords
                    .Where(r => r.ItemId == itemId && !r.IsDelete)
                    .OrderByDescending(r => r.ImportDate)
                    .ToListAsync();

                var applyRecords = await _context.ApplyRecords
                    .Where(r => r.ItemId == itemId && !r.IsDelete)
                    .OrderByDescending(r => r.ApplyDate)
                    .ToListAsync();

                var message = $"物品: {item.ItemName}\n" +
                            $"编码: {item.Code}\n" +
                            $"库存: {item.ItemNum}\n\n" +
                            "入库记录:\n" +
                            string.Join("\n", importRecords.Select(r => 
                                $"- {r.ImportDate:yyyy-MM-dd} 数量: {r.ImportNum} 单价: {r.SinglePrice:C}")) +
                            "\n\n领用记录:\n" +
                            string.Join("\n", applyRecords.Select(r => 
                                $"- {r.ApplyDate:yyyy-MM-dd} 数量: {r.ApplyNum} 状态: {(r.ApplyStatus == 0 ? "申请中" : r.ApplyStatus == 1 ? "已确认" : "已驳回")}"));

                await DisplayAlert("物品明细", message, "确定");
            }
        }
    }
} 