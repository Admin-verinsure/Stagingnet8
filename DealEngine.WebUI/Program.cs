using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Infrastructure;   // 👈 ADD THIS
namespace DealEngine.WebUI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // ✅ SET LICENSE HERE (only once)
            QuestPDF.Settings.License = LicenseType.Community;

            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}