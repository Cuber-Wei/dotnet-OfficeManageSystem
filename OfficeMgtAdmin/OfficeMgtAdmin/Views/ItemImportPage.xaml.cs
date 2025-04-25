using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;
using System.Threading;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemImportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly ObservableCollection<Item> _items;
        private Item? _selectedItem;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ItemImportPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _items = new ObservableCollection<Item>();
            ItemsCollection.ItemsSource = _items;
            ImportDatePicker.Date = DateTime.Now;
            
            // 设置输入事件处理
            ImportNumEntry.TextChanged += OnNumOrPriceChanged;
            SinglePriceEntry.TextChanged += OnNumOrPriceChanged;
            
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

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _selectedItem = e.SelectedItem as Item;
            if (_selectedItem != null)
            {
                ItemLabel.Text = $"已选择: {_selectedItem.ItemName}";
                
                // 获取最新的入库记录以获取单价
                var latestImport = await _context.ImportRecords
                    .Where(i => i.ItemId == _selectedItem.Id && !i.IsDelete)
                    .OrderByDescending(i => i.ImportDate)
                    .FirstOrDefaultAsync();

                if (latestImport != null)
                {
                    SinglePriceEntry.Text = latestImport.SinglePrice.ToString("0.00");
                }
                else
                {
                    SinglePriceEntry.Text = "0.00";
                }

                // 清空数量输入并初始化总价显示
                ImportNumEntry.Text = string.Empty;
                TotalPriceLabel.Text = "0.00";
                TotalPriceLabel.TextColor = Colors.Green;
            }
        }

        // 当数量或单价变化时计算总价
        private void OnNumOrPriceChanged(object? sender, TextChangedEventArgs e)
        {
            CalculateTotalPrice();
        }

        // 计算总价并更新显示
        private void CalculateTotalPrice()
        {
            try
            {
                int importNum = 0;
                decimal singlePrice = 0;

                // 检查数量和单价是否都有效
                bool hasValidNum = !string.IsNullOrWhiteSpace(ImportNumEntry.Text) && 
                                 int.TryParse(ImportNumEntry.Text, out importNum) && 
                                 importNum >= 0;
                
                bool hasValidPrice = !string.IsNullOrWhiteSpace(SinglePriceEntry.Text) && 
                                   decimal.TryParse(SinglePriceEntry.Text, out singlePrice) && 
                                   singlePrice >= 0;

                // 只有当数量和单价都有效时才计算总价
                if (hasValidNum && hasValidPrice)
                {
                    decimal totalPrice = importNum * singlePrice;
                    TotalPriceLabel.Text = totalPrice.ToString("0.00");
                    TotalPriceLabel.TextColor = Colors.Green;
                }
                else
                {
                    TotalPriceLabel.Text = "0.00";
                    TotalPriceLabel.TextColor = Colors.Green;
                }
            }
            catch
            {
                TotalPriceLabel.Text = "0.00";
                TotalPriceLabel.TextColor = Colors.Green;
            }
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

        private string GetItemTypeName(int itemType)
        {
            return itemType switch
            {
                0 => "办公用品",
                1 => "电子设备",
                2 => "家具",
                3 => "其他",
                _ => "未知"
            };
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                await DisplayAlert("提示", "请选择物品", "确定");
                return;
            }

            if (!int.TryParse(ImportNumEntry.Text, out int importNum))
            {
                await DisplayAlert("提示", "请输入有效的入库数量", "确定");
                return;
            }

            if (importNum < 0)
            {
                await DisplayAlert("提示", "入库数量不能为负数", "确定");
                ImportNumEntry.Text = "0";
                return;
            }

            if (!decimal.TryParse(SinglePriceEntry.Text, out decimal singlePrice))
            {
                await DisplayAlert("提示", "请输入有效的单价", "确定");
                return;
            }

            if (singlePrice < 0)
            {
                await DisplayAlert("提示", "单价不能为负数", "确定");
                SinglePriceEntry.Text = "0";
                return;
            }

            try
            {
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

                ImportNumEntry.Text = string.Empty;
                SinglePriceEntry.Text = string.Empty;
                ImportDatePicker.Date = DateTime.Now;
                ItemsCollection.SelectedItem = null;
                _selectedItem = null;
                ItemLabel.Text = "请选择物品";
                TotalPriceLabel.Text = "0.00";
                TotalPriceLabel.TextColor = Colors.Green;

                LoadItems();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }
    }
} 