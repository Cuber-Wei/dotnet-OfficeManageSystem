using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemInfoPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<Item> _items;
        private string? _selectedImagePath;
        private Item? _editingItem;

        public ItemInfoPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _items = new ObservableCollection<Item>();
            ItemsCollection.ItemsSource = _items;
            LoadItems();
        }

        private async void LoadItems()
        {
            try
            {
                var items = await _context.Items.Where(i => !i.IsDelete).ToListAsync();
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
            LoadItems();
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "选择物品图片"
                });

                if (result != null)
                {
                    _selectedImagePath = result.FullPath;
                    await DisplayAlert("提示", "图片选择成功", "确定");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"选择图片时出错: {ex.Message}", "确定");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEntry.Text) || string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("提示", "物品编码和名称不能为空", "确定");
                return;
            }

            try
            {
                if (_editingItem == null)
                {
                    // 创建新物品
                    var item = new Item
                    {
                        Code = CodeEntry.Text.Trim(),
                        ItemName = NameEntry.Text.Trim(),
                        ItemType = TypePicker.SelectedIndex,
                        Origin = OriginEntry.Text?.Trim(),
                        ItemSize = SizeEntry.Text?.Trim(),
                        ItemVersion = VersionEntry.Text?.Trim(),
                        ItemPic = _selectedImagePath,
                        ItemNum = 0,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        IsDelete = false
                    };
                    _context.Items.Add(item);
                }
                else
                {
                    // 更新现有物品
                    _editingItem.Code = CodeEntry.Text.Trim();
                    _editingItem.ItemName = NameEntry.Text.Trim();
                    _editingItem.ItemType = TypePicker.SelectedIndex;
                    _editingItem.Origin = OriginEntry.Text?.Trim();
                    _editingItem.ItemSize = SizeEntry.Text?.Trim();
                    _editingItem.ItemVersion = VersionEntry.Text?.Trim();
                    if (!string.IsNullOrEmpty(_selectedImagePath))
                    {
                        _editingItem.ItemPic = _selectedImagePath;
                    }
                    _editingItem.UpdateTime = DateTime.Now;
                }

                await SaveItemsAsync();
                await DisplayAlert("提示", _editingItem == null ? "添加成功" : "修改成功", "确定");
                LoadItems();
                ClearEntries();
                _editingItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }

        private void ClearEntries()
        {
            CodeEntry.Text = string.Empty;
            NameEntry.Text = string.Empty;
            TypePicker.SelectedIndex = -1;
            OriginEntry.Text = string.Empty;
            SizeEntry.Text = string.Empty;
            VersionEntry.Text = string.Empty;
            _selectedImagePath = null;
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;

            var confirm = await DisplayAlert("确认", "确定要删除该物品吗？", "确定", "取消");
            if (confirm)
            {
                var item = _items.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    item.IsDelete = true;
                    item.UpdateTime = DateTime.Now;
                    await SaveItemsAsync();
                    LoadItems();
                }
            }
        }

        private async Task SaveItemsAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存数据失败: {ex.Message}", "确定");
            }
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
                             $"物品类别: {item.ItemType}\n" +
                             $"产地: {item.Origin ?? "未设置"}\n" +
                             $"规格: {item.ItemSize ?? "未设置"}\n" +
                             $"型号: {item.ItemVersion ?? "未设置"}\n" +
                             $"创建时间: {item.CreateTime:yyyy-MM-dd HH:mm:ss}\n" +
                             $"更新时间: {item.UpdateTime:yyyy-MM-dd HH:mm:ss}";

                await DisplayAlert($"物品详情 - {item.ItemName}", message, "确定");
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;
            var item = _items.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                _editingItem = item;
                CodeEntry.Text = item.Code;
                NameEntry.Text = item.ItemName;
                TypePicker.SelectedIndex = item.ItemType;
                OriginEntry.Text = item.Origin;
                SizeEntry.Text = item.ItemSize;
                VersionEntry.Text = item.ItemVersion;
            }
        }
    }
} 