using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.Views;

public partial class ItemAddPage : ContentPage
{
    private readonly ApplicationDbContext _context;
    private string? _selectedImagePath;

    public ItemAddPage(ApplicationDbContext context)
    {
        InitializeComponent();
        _context = context;
        TypePicker.SelectedIndex = 0;
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "请选择图片",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                ImagePathLabel.Text = Path.GetFileName(result.FullPath);
                PreviewImage.Source = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", "选择图片时发生错误：" + ex.Message, "确定");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text) ||
            !int.TryParse(NumEntry.Text, out int itemNum))
        {
            await DisplayAlert("错误", "请填写必要信息", "确定");
            return;
        }

        var item = new Item
        {
            Code = CodeEntry.Text,
            ItemName = NameEntry.Text,
            ItemType = TypePicker.SelectedIndex,
            Origin = OriginEntry.Text,
            ItemSize = SizeEntry.Text,
            ItemVersion = VersionEntry.Text,
            ItemNum = itemNum,
            ItemPic = _selectedImagePath,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            IsDelete = false
        };

        try
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            await DisplayAlert("成功", "物品入库成功", "确定");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", "保存失败：" + ex.Message, "确定");
        }
    }
} 