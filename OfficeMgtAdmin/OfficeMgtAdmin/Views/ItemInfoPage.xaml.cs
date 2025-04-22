using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemInfoPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<Item> _items;
        private string _selectedImagePath;

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
            var items = await _context.Items.Where(i => !i.IsDelete).ToListAsync();
            _items.Clear();
            foreach (var item in items)
            {
                _items.Add(item);
            }
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
            await _context.SaveChangesAsync();
            await DisplayAlert("提示", "保存成功", "确定");
            LoadItems();
            ClearEntries();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var itemId = (long)button.CommandParameter;
            var item = await _context.Items.FindAsync(itemId);
            if (item != null)
            {
                item.IsDelete = true;
                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "删除成功", "确定");
                LoadItems();
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
    }
} 