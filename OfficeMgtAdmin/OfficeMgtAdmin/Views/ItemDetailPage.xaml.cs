using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace OfficeMgtAdmin.Views;

public partial class ItemDetailPage : ContentPage
{
    private readonly ApplicationDbContext _context;
    private readonly Item _item;

    public ItemDetailPage(ApplicationDbContext context, Item item)
    {
        InitializeComponent();
        _context = context;

        // 检查项目是否被跟踪，如果是，则分离它
        if (_context.Entry(item).State != EntityState.Detached)
        {
            _context.Entry(item).State = EntityState.Detached;
        }

        // 创建新的 Item 对象，避免共享引用
        _item = new Item
        {
            Id = item.Id,
            Code = item.Code,
            ItemName = item.ItemName,
            ItemType = item.ItemType,
            Origin = item.Origin,
            ItemSize = item.ItemSize,
            ItemVersion = item.ItemVersion,
            ItemNum = item.ItemNum,
            ItemPic = item.ItemPic,
            CreateTime = item.CreateTime,
            UpdateTime = item.UpdateTime,
            IsDelete = item.IsDelete
        };
        
        LoadItemDetails();
    }

    private void LoadItemDetails()
    {
        // 设置图片
        if (!string.IsNullOrEmpty(_item.ItemPic))
        {
            ItemImage.Source = ImageSource.FromFile(_item.ItemPic);
        }
        else
        {
            ItemImage.Source = "default_item.png";
        }

        // 设置基本信息
        CodeLabel.Text = _item.Code;
        NameLabel.Text = _item.ItemName;
        TypeLabel.Text = GetItemTypeText(_item.ItemType);
        OriginLabel.Text = _item.Origin;
        SizeLabel.Text = _item.ItemSize;
        VersionLabel.Text = _item.ItemVersion;
        StockLabel.Text = _item.ItemNum.ToString();
        CreateTimeLabel.Text = _item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
        UpdateTimeLabel.Text = _item.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private string GetItemTypeText(int type)
    {
        return type switch
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

    private async void OnApplyClicked(object sender, EventArgs e)
    {
        // TODO: 实现申请领用功能
        await DisplayAlert("提示", "申请领用功能开发中...", "确定");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
} 