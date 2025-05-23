using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Web.Data;
using OfficeMgtAdmin.Web.Models;
using System.Text.Json;
using System.IO;
using OfficeMgtAdmin.Web.Attributes;
using Microsoft.AspNetCore.Http;

namespace OfficeMgtAdmin.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string userId, string password)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
        {
            return Json(new { success = false, message = "用户名和密码不能为空" });
        }

        // 从 user.json 读取用户信息
        var userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
        var userJson = System.IO.File.ReadAllText(userJsonPath);
        var users = JsonSerializer.Deserialize<List<User>>(userJson);

        var user = users?.FirstOrDefault(u => u.UserId == userId && u.Password == password);
        if (user == null)
        {
            return Json(new { success = false, message = "用户名或密码不正确" });
        }

        // 存储用户信息到 Session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.UserName);

        return Json(new { success = true });
    }

    [Authorize]
    public async Task<IActionResult> Inventory()
    {
        var items = await _context.Items
            .Where(i => !i.IsDelete)
            .OrderBy(i => i.Code)
            .ToListAsync();

        // 获取每个物品的最新入库价格
        var latestImports = await _context.ImportRecords
            .Where(i => !i.IsDelete)
            .GroupBy(i => i.ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                SinglePrice = g.OrderByDescending(i => i.ImportDate)
                             .Select(i => i.SinglePrice)
                             .FirstOrDefault()
            })
            .Where(x => x.SinglePrice > 0)
            .ToDictionaryAsync(x => x.ItemId, x => x.SinglePrice);

        ViewBag.LatestImports = latestImports;

        return View(items);
    }

    [Authorize]
    [HttpPost]
    public IActionResult QueryByType(int? itemType)
    {
        var query = _context.Items.Where(i => !i.IsDelete);
        if (itemType.HasValue)
        {
            query = query.Where(i => i.ItemType == itemType.Value);
        }

        var items = query.ToList();
        return PartialView("_InventoryList", items);
    }

    [Authorize]
    public IActionResult Apply()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction("Index");
        }

        var items = _context.Items
            .Where(i => !i.IsDelete)
            .ToList();

        return View(items);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitApply(long itemId, int applyNum)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return Json(new { success = false, message = "请先登录" });
        }

        var userId = long.Parse(HttpContext.Session.GetString("UserId") ?? "0");
        var item = await _context.Items.FindAsync(itemId);

        if (item == null)
        {
            return Json(new { success = false, message = "物品不存在" });
        }

        if (applyNum <= 0)
        {
            return Json(new { success = false, message = "申请数量必须大于0" });
        }

        if (item.ItemNum < applyNum)
        {
            return Json(new { success = false, message = "库存不足" });
        }

        var applyRecord = new ApplyRecord
        {
            UserId = userId,
            ItemId = itemId,
            ApplyNum = applyNum,
            ApplyStatus = 0,
            ApplyDate = DateTime.Now,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            IsDelete = false
        };

        _context.ApplyRecords.Add(applyRecord);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [Authorize]
    public IActionResult MyApplications()
    {
        var userId = long.Parse(HttpContext.Session.GetString("UserId") ?? "0");
        var applications = _context.ApplyRecords
            .Include(a => a.Item)
            .Where(a => a.UserId == userId && !a.IsDelete)
            .OrderByDescending(a => a.ApplyDate)
            .ToList();

        return View(applications);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}