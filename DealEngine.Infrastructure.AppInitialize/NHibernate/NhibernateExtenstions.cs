using DealEngine.Infrastructure.FluentNHibernate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.AspNetCore.Identity;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Extensions.Logging;
using NHibernate.Extensions.NpgSql;
using NHibernate.Mapping.ByCode;
using System;
using System.Reflection;

namespace DealEngine.Infrastructure.AppInitialize.Nhibernate
{
    public static class NHibernateExtensions
    {
        public static IServiceCollection AddNHibernate(this IServiceCollection services)
        {
            // 1️⃣ Get .NET built-in logger factory from DI
            var connectionStringName = "DealEngineConnection";
            var sessionFactory = SessionFactoryBuilder.BuildSessionFactory(connectionStringName);

            services.AddSingleton(sessionFactory);
            services.AddScoped(factory => sessionFactory.OpenSession());
            services.AddTransient(typeof(IMapperSession<>), typeof(NHibernateMapperSession<>));
            services.AddTransient<IUnitOfWork, NHibernateUnitOfWork>();

            return services;
        }
    }

    
}
