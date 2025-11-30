using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate.MappingConventions;
using DealEngine.Infrastructure.FluentNHibernate.MappingOverrides;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.AspNetCore.Identity;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Extensions.Logging;
using NHibernate.Extensions.NpgSql;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

using NHibernate.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace DealEngine.Infrastructure.AppInitialize.Nhibernate
{
    public class SessionFactoryBuilder
    {

        private static string NpgsqlConnectionString;
        public static ISessionFactory BuildSessionFactory(string connectionStringName)
        {
            // 1. Create a Microsoft logger factory
           
            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                        .AddJsonFile("appsettings.json")
                                        .Build();

            NpgsqlConnectionString = configuration.GetConnectionString("DealEngineConnection");

            var session = Fluently.Configure()
                .Database(PostgreSQLConfiguration.Standard.ConnectionString(NpgsqlConnectionString)
                    .Dialect<PostgreSQL82Dialect>()
                    .AdoNetBatchSize(10)
                    .Driver<NpgSqlDriver>()
                    .ShowSql()
                    .FormatSql()
                    .MaxFetchDepth(5)
                 )
                .CurrentSessionContext("web")                               
                .ExposeConfiguration(cfg => BuildSchema(cfg, NpgsqlConnectionString))
                .Mappings(m => m.AutoMappings.Add(AutoMap.AssemblyOf<Organisation>(new DefaultAutomappingConfiguration())
                    .Conventions.Add<CascadeConvention>()
                    .AddMappingsFromAssembly(Assembly.GetExecutingAssembly())
                .UseOverridesFromAssemblyOf<UserMappingOverride>())
                ).BuildConfiguration()
                .AddIdentityMappingsForPostgres()
                .BuildSessionFactory();

            return session;
        }
            /// <summary>  
            /// Build the schema of the database.  
            /// </summary>  
            /// <param name="config">Configuration.</param>  
        private static void BuildSchema(Configuration config, string connectionStringName)
        {

            using (var connection = new NpgsqlConnection(connectionStringName))
            {
                try
                {
                    connection.Open();
                }
                catch (NpgsqlException)
                {
                    CreateDatabase(connectionStringName);
                    CreateLoggingTable(connectionStringName);
                    CreateIdentityTable(connectionStringName);
                }
                finally
                {
                    try
                    {
                        var schemaUpdate = new SchemaUpdate(config);
                        schemaUpdate.Execute(false, true);
                        if (schemaUpdate.Exceptions.Count != 0)
                        {
                            foreach(var ex in schemaUpdate.Exceptions)
                            {
                                 throw ex;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    
                }
            }          
        }
       
        private static void CreateDatabase(string connectionStringName)
        {
            var masterConnectionString = Regex.Replace(connectionStringName, "(Database|Initial Catalog)=[^;]+", "Database=postgres", RegexOptions.IgnoreCase);
            using (var connection = new NpgsqlConnection(masterConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "CREATE DATABASE " + "\"" + GetDatabaseName(connectionStringName) + "\"";
                command.ExecuteNonQuery();
            }
        }

        private static void CreateLoggingTable(string connectionStringName)
        {
            using (var connection = new NpgsqlConnection(connectionStringName))
            {
                connection.Open();

                string strSql = @" CREATE SEQUENCE ELMAH_Error_SEQUENCE;

                                        CREATE TABLE ELMAH_Error
                                        (
                                            ErrorId     UUID NOT NULL,
                                            Application VARCHAR(60) NOT NULL,
                                            Host        VARCHAR(50) NOT NULL,
                                            Type        VARCHAR(100) NOT NULL,
                                            Source      VARCHAR(60)  NOT NULL,
                                            Message     VARCHAR(500) NOT NULL,
                                            ""User""      VARCHAR(50)  NOT NULL,
                                            StatusCode  INT NOT NULL,
                                            TimeUtc     TIMESTAMP NOT NULL,
                                            Sequence    INT NOT NULL DEFAULT NEXTVAL('ELMAH_Error_SEQUENCE'),
                                            AllXml      TEXT NOT NULL
                                        );

                                        ALTER TABLE ELMAH_Error ADD CONSTRAINT PK_ELMAH_Error PRIMARY KEY (ErrorId);

                                        CREATE INDEX IX_ELMAH_Error_App_Time_Seq ON ELMAH_Error USING BTREE
                                        (
                                            Application   ASC,
                                            TimeUtc       DESC,
                                            Sequence      DESC
                                        );";

                using (var cmd = new NpgsqlCommand(strSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void CreateIdentityTable(string connectionStringName)
        {
            using (var connection = new NpgsqlConnection(connectionStringName))
            {
                connection.Open();

                string strSql = @" 
-- sequence: public.snow_flake_id_seq
-- drop sequence public.snow_flake_id_seq;
create sequence public.snow_flake_id_seq;
alter sequence public.snow_flake_id_seq
    owner to postgres;

-- function: public.snow_flake_id()
-- drop function public.snow_flake_id();

create or replace function public.snow_flake_id()
    returns bigint
    language 'sql'
    cost 100
    volatile
as $body$

select (extract(epoch from current_timestamp) * 1000)::bigint * 1000000
  + 2 * 10000
  + nextval('public.snow_flake_id_seq') % 1000
  as snow_flake_id

$body$;

alter function public.snow_flake_id()
    owner to postgres;

comment on function public.snow_flake_id()
    is 'snow flake id ';

-- table: public.aspnet_roles
-- drop table public.aspnet_roles;

create table public.aspnet_roles
(
    id character varying(32) collate pg_catalog.""default"" not null default (snow_flake_id())::character varying,
    name character varying(64) collate pg_catalog.""default"" not null,
    normalized_name character varying(64) collate pg_catalog.""default"" not null,
    concurrency_stamp character varying(36) collate pg_catalog.""default"",
    constraint pk_aspnet_roles primary key(id),
    constraint u_aspnet_roles_name unique(name),
    constraint u_aspnet_roles_normalized_name unique(normalized_name)
)
with(
    oids = false
)
tablespace pg_default;

                alter table public.aspnet_roles
                    owner to postgres;

-- index: ix_aspnet_roles_name
-- drop index public.ix_aspnet_roles_name;

create index ix_aspnet_roles_name
    on public.aspnet_roles using btree
    (normalized_name collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_role_claims
-- drop table public.aspnet_role_claims;
create table public.aspnet_role_claims
(
    id integer not null default nextval('snow_flake_id_seq'::regclass),
    role_id character varying(32) collate pg_catalog.""default"" not null,
    claim_type character varying(1024) collate pg_catalog.""default"" not null,
    claim_value character varying(1024) collate pg_catalog.""default"" not null,
    constraint pk_aspnet_role_claims primary key (id),
    constraint fk_aspnet_roles_id foreign key (role_id)
        references public.aspnet_roles(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_role_claims
        owner to postgres;
    comment on table public.aspnet_role_claims
    is 'aspnet role claims table';

-- index: ix_aspnet_role_claims_role_id

-- drop index public.ix_aspnet_role_claims_role_id;

create index ix_aspnet_role_claims_role_id
    on public.aspnet_role_claims using btree
    (role_id collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_users

-- drop table public.aspnet_users;

create table public.aspnet_users
(
    id character varying(32) collate pg_catalog.""default"" not null default (snow_flake_id())::character varying,
    user_name character varying(64) collate pg_catalog.""default"" not null,
    normalized_user_name character varying(64) collate pg_catalog.""default"" not null,
    email character varying(256) collate pg_catalog.""default"" not null,
    normalized_email character varying(256) collate pg_catalog.""default"" not null,
    email_confirmed boolean not null,
    phone_number character varying(32) collate pg_catalog.""default"",
    phone_number_confirmed boolean not null,
    lockout_enabled boolean not null,
    lockout_end_unix_time_milliseconds bigint,
    password_hash character varying(256) collate pg_catalog.""default"",
    access_failed_count integer not null,
    security_stamp character varying(256) collate pg_catalog.""default"",
    two_factor_enabled boolean not null,
    concurrency_stamp character varying(36) collate pg_catalog.""default"",
    constraint pk_aspnet_users primary key (id),
    constraint u_aspnet_users_normalized_user_name unique (normalized_user_name),
    constraint u_aspnet_users_username unique (user_name)
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_users
        owner to postgres;
    comment on table public.aspnet_users
    is 'aspnet users table.';

-- index: ix_aspnet_users_email

-- drop index public.ix_aspnet_users_email;

create index ix_aspnet_users_email
    on public.aspnet_users using btree
    (normalized_email collate pg_catalog.""default"")
    tablespace pg_default;

-- index: ix_aspnet_users_user_name

-- drop index public.ix_aspnet_users_user_name;

create unique index ix_aspnet_users_user_name
    on public.aspnet_users using btree
    (normalized_user_name collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_user_claims

-- drop table public.aspnet_user_claims;

create table public.aspnet_user_claims
(
    id integer not null default nextval('snow_flake_id_seq'::regclass),
    user_id character varying(32) collate pg_catalog.""default"" not null,
    claim_type character varying(1024) collate pg_catalog.""default"" not null,
    claim_value character varying(1024) collate pg_catalog.""default"" not null,
    constraint pk_aspnet_user_claims primary key (id),
    constraint fk_aspnet_users_id foreign key (user_id)
        references public.aspnet_users(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_user_claims
        owner to postgres;
    comment on table public.aspnet_user_claims
    is 'aspnet user claims table';

-- index: ix_aspnet_user_claims_user_id

-- drop index public.ix_aspnet_user_claims_user_id;

create index ix_aspnet_user_claims_user_id
    on public.aspnet_user_claims using btree
    (user_id collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_user_logins

-- drop table public.aspnet_user_logins;

create table public.aspnet_user_logins
(
    login_provider character varying(32) collate pg_catalog.""default"" not null,
    provider_key character varying(1024) collate pg_catalog.""default"" not null,
    provider_display_name character varying(32) collate pg_catalog.""default"" not null,
    user_id character varying(32) collate pg_catalog.""default"" not null,
    constraint pk_aspnet_user_logins primary key (login_provider, provider_key),
    constraint fk_aspnet_user_logins_user_id foreign key (user_id)
        references public.aspnet_users(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_user_logins
        owner to postgres;
    comment on table public.aspnet_user_logins
    is 'aspnet user logins table.';

-- index: ix_aspnet_user_logins_user_id

-- drop index public.ix_aspnet_user_logins_user_id;

create index ix_aspnet_user_logins_user_id
    on public.aspnet_user_logins using btree
    (user_id collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_user_tokens

-- drop table public.aspnet_user_tokens;

create table public.aspnet_user_tokens
(
    user_id character varying(32) collate pg_catalog.""default"" not null,
    login_provider character varying(32) collate pg_catalog.""default"" not null,
    name character varying(32) collate pg_catalog.""default"" not null,
    value character varying(256) collate pg_catalog.""default"",
    constraint pk_aspnet_user_tokens primary key (user_id, login_provider, name),
    constraint fk_aspnet_users_id foreign key (user_id)
        references public.aspnet_users(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_user_tokens
        owner to postgres;
    comment on table public.aspnet_user_tokens
    is 'aspnet user tokens table.';

-- index: ix_aspnet_user_tokens_user_id

-- drop index public.ix_aspnet_user_tokens_user_id;

create index ix_aspnet_user_tokens_user_id
    on public.aspnet_user_tokens using btree
    (user_id collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.aspnet_user_roles

-- drop table public.aspnet_user_roles;

create table public.aspnet_user_roles
(
    user_id character varying(32) collate pg_catalog.""default"" not null,
    role_id character varying(32) collate pg_catalog.""default"" not null,
    constraint pk_aspnet_user_roles primary key (user_id, role_id),
    constraint fk_aspnet_roles_id foreign key (role_id)
        references public.aspnet_roles(id) match simple
        on update cascade
        on delete cascade,
    constraint fk_aspnet_users_id foreign key(user_id)
        references public.aspnet_users(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

    alter table public.aspnet_user_roles
        owner to postgres;
    comment on table public.aspnet_user_roles
    is 'aspnet user roles relation table.';

-- index: ix_aspnet_user_roles_role_id

-- drop index public.ix_aspnet_user_roles_role_id;

create index ix_aspnet_user_roles_role_id
    on public.aspnet_user_roles using btree
    (role_id collate pg_catalog.""default"")
    tablespace pg_default;

-- index: ix_aspnet_user_roles_user_id

-- drop index public.ix_aspnet_user_roles_user_id;

create index ix_aspnet_user_roles_user_id
    on public.aspnet_user_roles using btree
    (user_id collate pg_catalog.""default"")
    tablespace pg_default;

-- table: public.app_users

-- drop table public.app_users;

create table public.app_users
(
    id character varying(32) collate pg_catalog.""default"" not null,
    create_time timestamp without time zone not null default now(),
    last_login timestamp without time zone,
    login_count integer default 0,
    constraint pk_app_users primary key(id),
    constraint fk_aspnet_users_id foreign key(id)
        references public.aspnet_users(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

        alter table public.app_users
            owner to postgres;
        comment on table public.app_users
    is 'application users table.';
-- table: public.app_roles

-- drop table public.app_roles;

create table public.app_roles
(
    id character varying(32) collate pg_catalog.""default"" not null,
    description character varying(256) collate pg_catalog.""default"",
    constraint pk_app_roles primary key(id),
    constraint fk_aspnet_roles_id foreign key(id)
        references public.aspnet_roles(id) match simple
        on update cascade
        on delete cascade
)
with(
    oids = false
)
tablespace pg_default;

        alter table public.app_roles
            owner to postgres;
        comment on table public.app_roles
    is 'application roles table.';

comment on column public.app_roles.description
    is 'roles description';";

                using (var cmd = new NpgsqlCommand(strSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string GetDatabaseName(string connectionStringName)
        {
            using (var connection = new NpgsqlConnection(connectionStringName))
            {
                return connection.Database;
            }
        }
    }
}
