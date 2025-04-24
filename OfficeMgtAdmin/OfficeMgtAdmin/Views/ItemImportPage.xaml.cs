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
            ImportNumEntry.TextChanged += OnImportNumTextChanged;
            SinglePriceEntry.TextChanged += OnSinglePriceTextChanged;
            
            LoadItems();
        }

        // 验证入库数量不为负数
        private void OnImportNumTextChanged(object? sender, TextChangedEventArgs e)
        {
            // 如果为空则跳过验证
            if (string.IsNullOrEmpty(e.NewTextValue))
                return;
            
            // 尝试解析为数字
            if (int.TryParse(e.NewTextValue, out int value))
            {
                // 如果是负数，将其改为0并提示
                if (value < 0)
                {
                    ImportNumEntry.Text = "0";
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("提示", "入库数量不能为负数，已自动更正为0", "确定");
                    });
                }
            }
            // 如果输入的不是有效数字，恢复之前的值或设为0
            else if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                ImportNumEntry.Text = string.IsNullOrEmpty(e.OldTextValue) ? "0" : e.OldTextValue;
            }
        }

        // 验证单价不为负数
        private void OnSinglePriceTextChanged(object? sender, TextChangedEventArgs e)
        {
            // 如果为空则跳过验证
            if (string.IsNullOrEmpty(e.NewTextValue))
                return;
            
            // 允许小数点输入
            if (e.NewTextValue == ".")
            {
                SinglePriceEntry.Text = "0.";
                return;
            }
            
            // 尝试解析为小数
            if (decimal.TryParse(e.NewTextValue, out decimal value))
            {
                // 如果是负数，将其改为0并提示
                if (value < 0)
                {
                    SinglePriceEntry.Text = "0";
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("提示", "单价不能为负数，已自动更正为0", "确定");
                    });
                }
            }
            // 如果输入的不是有效小数，恢复之前的值或设为0
            else if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                SinglePriceEntry.Text = string.IsNullOrEmpty(e.OldTextValue) ? "0" : e.OldTextValue;
            }
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
                CalculateTotalPrice();
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
                if (int.TryParse(ImportNumEntry.Text, out int importNum) && 
                    decimal.TryParse(SinglePriceEntry.Text, out decimal singlePrice))
                {
                    decimal totalPrice = importNum * singlePrice;
                    
                    TotalPriceLabel.Text = totalPrice.ToString("0.00");
                    
                    TotalPriceLabel.TextColor = totalPrice > 0 ? Colors.Green : Colors.Black;
                }
                else
                {
                    TotalPriceLabel.Text = "0.00";
                    TotalPriceLabel.TextColor = Colors.Black;
                }
            }
            catch
            {
                TotalPriceLabel.Text = "0.00";
                TotalPriceLabel.TextColor = Colors.Black;
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
                TotalPriceLabel.TextColor = Colors.Black;

                LoadItems();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }
    }
} 