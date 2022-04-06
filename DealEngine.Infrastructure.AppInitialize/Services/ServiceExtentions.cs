using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using DealEngine.Services.Impl;
using DealEngine.Services.Impl.UnderwritingModuleServices;
using DealEngine.Services.Interfaces;

namespace DealEngine.Infrastructure.AppInitialize.Services
{
    public static class RespositoriesExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            var repositoryAssembly = typeof(EmailService).Assembly;
            var registrations =
                from type in repositoryAssembly.GetExportedTypes()
                where type.Namespace == "DealEngine.Services.Impl"
                where type.GetInterfaces().Any()
                select new { Service = type.GetInterfaces().Single(), Implementation = type };

            foreach (var reg in registrations)
            {
                services.AddTransient(reg.Service, reg.Implementation);
            }

            services.AddTransient<IUnderwritingModule, EmptyUWModule>();
            services.AddTransient<IUnderwritingModule, ICIBARCCOUWModule>();
            services.AddTransient<IUnderwritingModule, ICIBHIANZUWModule>();
            services.AddTransient<IUnderwritingModule, MarshCoastGuardUWModule>();
            return services;
        }
    }

    
}
