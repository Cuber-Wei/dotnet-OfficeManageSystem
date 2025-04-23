using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemEditPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private readonly long _itemId;
        private Item? _item;
        private string? _selectedImagePath;

        public ItemEditPage(ApplicationDbContext context, long itemId)
        {
            InitializeComponent();
            _context = context;
            _itemId = itemId;
            LoadItem();
        }

        private async void LoadItem()
        {
            try
            {
                _item = await _context.Items.FindAsync(_itemId);
                if (_item == null)
                {
                    await DisplayAlert("错误", "找不到指定的物品", "确定");
                    await Navigation.PopAsync();
                    return;
                }

                CodeEntry.Text = _item.Code;
                NameEntry.Text = _item.ItemName;
                TypePicker.SelectedIndex = _item.ItemType;
                OriginEntry.Text = _item.Origin;
                SizeEntry.Text = _item.ItemSize;
                VersionEntry.Text = _item.ItemVersion;
                _selectedImagePath = _item.ItemPic;
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载物品数据失败: {ex.Message}", "确定");
                await Navigation.PopAsync();
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
            if (_item == null)
            {
                await DisplayAlert("错误", "物品数据无效", "确定");
                return;
            }

            if (string.IsNullOrWhiteSpace(CodeEntry.Text) || string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("提示", "物品编码和名称不能为空", "确定");
                return;
            }

            _item.Code = CodeEntry.Text.Trim();
            _item.ItemName = NameEntry.Text.Trim();
            _item.ItemType = TypePicker.SelectedIndex;
            _item.Origin = OriginEntry.Text?.Trim();
            _item.ItemSize = SizeEntry.Text?.Trim();
            _item.ItemVersion = VersionEntry.Text?.Trim();
            _item.ItemPic = _selectedImagePath;
            _item.UpdateTime = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                await DisplayAlert("提示", "保存成功", "确定");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }
    }
} 