using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemImportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly ObservableCollection<Item> _items;
        private Item? _selectedItem;

        public ItemImportPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _items = new ObservableCollection<Item>();
            ItemsCollection.ItemsSource = _items;
            ImportDatePicker.Date = DateTime.Now;
            LoadItems();
        }

        private async void LoadItems()
        {
            try
            {
                var items = await _context.Items
                    .AsNoTracking()
                    .Where(i => !i.IsDelete)
                    .OrderBy(i => i.Code)
                    .ToListAsync();

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

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _selectedItem = e.SelectedItem as Item;
            if (_selectedItem != null)
            {
                ItemLabel.Text = $"已选择: {_selectedItem.ItemName}";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                await DisplayAlert("提示", "请选择物品", "确定");
                return;
            }

            if (!int.TryParse(ImportNumEntry.Text, out int importNum) || importNum <= 0)
            {
                await DisplayAlert("提示", "请输入有效的入库数量", "确定");
                return;
            }

            if (!decimal.TryParse(SinglePriceEntry.Text, out decimal singlePrice) || singlePrice <= 0)
            {
                await DisplayAlert("提示", "请输入有效的单价", "确定");
                return;
            }

            try
            {
                // 重新从数据库获取最新的物品数据
                var item = await _context.Items.FindAsync(_selectedItem.Id);
                if (item == null)
                {
                    await DisplayAlert("错误", "找不到选中的物品", "确定");
                    return;
                }

                var importRecord = new ImportRecord
                {
                    ItemId = item.Id,
                    ImportNum = importNum,
                    SinglePrice = singlePrice,
                    ImportDate = ImportDatePicker.Date,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDelete = false
                };

                _context.ImportRecords.Add(importRecord);

                item.ItemNum += importNum;
                item.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "入库成功", "确定");

                // 清空输入
                ImportNumEntry.Text = string.Empty;
                SinglePriceEntry.Text = string.Empty;
                ImportDatePicker.Date = DateTime.Now;
                ItemsCollection.SelectedItem = null;
                _selectedItem = null;
                ItemLabel.Text = "请选择物品";

                // 重新加载物品列表
                LoadItems();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }
    }
} 