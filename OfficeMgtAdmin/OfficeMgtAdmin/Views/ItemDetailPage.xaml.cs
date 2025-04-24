using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace OfficeMgtAdmin.Views;

public partial class ItemDetailPage : ContentPage
{
    private readonly ApplicationDbContext _context;
    private readonly Item _item;
    private readonly string _webRootPath = @"F:\code_repository\dotNetProjects\OfficeMgtAdmin\OfficeMgtAdmin.Web\wwwroot";

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
            try
            {
                // 检查是否是Web路径
                if (_item.ItemPic.StartsWith("/images/"))
                {
                    // 是Web路径，构建完整的文件系统路径
                    string fullPath = Path.Combine(_webRootPath, _item.ItemPic.TrimStart('/'));
                    
                    if (File.Exists(fullPath))
                    {
                        // 加载Web目录中的图片
                        ItemImage.Source = ImageSource.FromFile(fullPath);
                    }
                    else
                    {
                        // Web目录中的图片不存在，使用默认图片
                        ItemImage.Source = "default_item.jpg";
                    }
                }
                else if (File.Exists(_item.ItemPic))
                {
                    // 直接加载本地图片
                    ItemImage.Source = ImageSource.FromFile(_item.ItemPic);
                }
                else
                {
                    // 文件不存在，使用默认图片
                    ItemImage.Source = "default_item.jpg";
                }
            }
            catch (Exception)
            {
                // 加载失败时使用默认图片
                ItemImage.Source = "default_item.jpg";
            }
        }
        else
        {
            ItemImage.Source = "default_item.jpg";
        }

        // 设置文本标签
        CodeLabel.Text = _item.Code;
        NameLabel.Text = _item.ItemName;
        TypeLabel.Text = GetItemTypeName(_item.ItemType);
        OriginLabel.Text = _item.Origin ?? "--";
        SizeLabel.Text = _item.ItemSize ?? "--";
        VersionLabel.Text = _item.ItemVersion ?? "--";
        StockLabel.Text = _item.ItemNum.ToString();
        CreateTimeLabel.Text = _item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
        UpdateTimeLabel.Text = _item.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private string GetItemTypeName(int type)
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
} 