using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;


namespace OfficeMgtAdmin
{
    public static class MauiProgramExtensions
    {
        public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Configure EF Core with MySQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(
                    "Server=localhost;Database=dot_net_project_db;User=root;Password=123456;Port=3306",
                    ServerVersion.AutoDetect("Server=localhost;Database=dot_net_project_db;User=root;Password=123456;Port=3306"),
                    mySqlOptions => mySqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null)
                        .CommandTimeout(30)
                );
            }, ServiceLifetime.Scoped);
            return builder;
        }
    }
}
