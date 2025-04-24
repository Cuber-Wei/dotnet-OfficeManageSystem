using Microsoft.Extensions.Logging;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Views;

namespace OfficeMgtAdmin;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 注册数据库上下文
        builder.Services.AddDbContext<ApplicationDbContext>();

        // 注册页面
        builder.Services.AddTransient<ItemEditPage>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<ItemExportPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
} 