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
    private readonly string _defaultImagePath = "default-item.jpg";

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
        try
        {
            if (string.IsNullOrEmpty(_item.ItemPic))
            {
                // 如果图片路径为空，使用默认图片
                ItemImage.Source = Path.Combine(_webRootPath,"images",_defaultImagePath);
            }
            else if (_item.ItemPic.StartsWith("/images/"))
            {
                // 处理Web路径
                string fullPath = Path.Combine(_webRootPath, _item.ItemPic.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    ItemImage.Source = ImageSource.FromFile(fullPath);
                }
                else
                {
                    ItemImage.Source = _defaultImagePath;
                }
            }
            else if (File.Exists(_item.ItemPic))
            {
                // 处理本地文件路径
                ItemImage.Source = ImageSource.FromFile(_item.ItemPic);
            }
            else
            {
                // 文件不存在，使用默认图片
                ItemImage.Source = _defaultImagePath;
            }
        }
        catch (Exception ex)
        {
            // 记录异常信息
            System.Diagnostics.Debug.WriteLine($"加载图片出错: {ex.Message}");
            // 加载失败时使用默认图片
            ItemImage.Source = _defaultImagePath;
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