using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

using FurCoNZ.DAL;

namespace FurCoNZ
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FurCoNZDbContext>(options =>
            {
                switch (Configuration.GetValue("Data:Database:Provider", string.Empty).ToLowerInvariant())
                {
                    case "mysql":
                    case "mariadb":
                        throw new NotSupportedException(@"MySQL/MariaDB is not currently supported.");
                    case "postgres":
                    case "postgresql":
                        options.UseNpgsql(new NpgsqlConnectionStringBuilder
                        {
                            Database = Configuration.GetValue<string>("Data:Database:Database"),
                            Host = Configuration.GetValue<string>("Data:Database:Host"),
                            Port = Configuration.GetValue("Data:Database:Port", 5432),
                            Username = Configuration.GetValue("Data:Database:Username", string.Empty),
                            Password = Configuration.GetValue("Data:Database:Password", string.Empty),
                        }.ToString());
                        break;
                    case "oracle":
                        throw new NotSupportedException(@"Ha, no. Oracle can fuck right off. https://web.archive.org/web/20150811052336/https://blogs.oracle.com/maryanndavidson/entry/no_you_really_can_t");
                    case "sqlserver":
                        throw new NotSupportedException(@"Microsoft SQL Server is not currently supported. This may come in a future release because it's easy to implement, but it may be buggy and won't be officially maintained.");
                    case "sqlite":
                    default:
                        var databasePath = new DirectoryInfo(Path.Combine("data", "database"));
                        if (!databasePath.Exists)
                            databasePath.Create();

                        var connectionStringBuilder = new SqliteConnectionStringBuilder
                        {
                            DataSource = Path.Combine(
                                Environment.CurrentDirectory,
                                Configuration.GetValue("Paths:Database", "data/database"),
                                Configuration.GetValue("Data:Database:Filename", "data.db"))
                        };
                        options.UseSqlite(connectionStringBuilder.ToString());
                        break;
                }
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
