using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace DealEngine.WebUI
{
    public static class Program
    {
       
        public static void Main(string[] args) =>CreateWebHostBuilder(args).Build().Run();

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
         .UseStartup<Startup>()
         .UseKestrel(options =>
         {
             options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10); 
                                                                         
             options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10); 


         });
    }
}
