// Core
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

// Microsoft
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
#if DEBUG
using Microsoft.Data.Sqlite;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

// Third-party
using Npgsql;
using SendGrid;

// Local
using FurCoNZ.Web.Auth;
using FurCoNZ.Web.Configuration;
using FurCoNZ.Web.DAL;
using FurCoNZ.Web.Options;
using FurCoNZ.Web.Services;
using Microsoft.AspNetCore.Localization;

namespace FurCoNZ.Web
{
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, ILogger<Startup> logger, IHostingEnvironment env)
        {
            Configuration = configuration;
            _logger = logger;
            _env = env;
            // All the sames do this. I have no idea why.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.Name = "FurcoNZSessionCookie";
                options.Cookie.HttpOnly = true;
                //options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = _env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            services.AddHttpClient();

            // Add Entity Framework DB Context
            services.AddDbContext<FurCoNZDbContext>(options =>
            {
                switch (Configuration.GetValue("Data:Database:Provider", string.Empty).ToLowerInvariant())
                {
                    case "mysql":
                    case "mariadb":
                        throw new NotSupportedException(@"MySQL/MariaDB is not currently supported.");
                    case "oracle":
                        throw new NotSupportedException(@"Ha, no. Oracle can fuck right off. https://web.archive.org/web/20150811052336/https://blogs.oracle.com/maryanndavidson/entry/no_you_really_can_t");
                    case "sqlserver":
                        throw new NotSupportedException(@"Microsoft SQL Server is not currently supported. This may come in a future release because it's easy to implement, but it may be buggy and won't be officially maintained.");
                    case "sqlite":
#if DEBUG
                        var databasePath = new DirectoryInfo(Path.Combine("data", "database"));
                        if (!databasePath.Exists)
                            databasePath.Create();

                        var connectionStringBuilder = new SqliteConnectionStringBuilder
                        {
                            DataSource = Path.Combine(
                                Environment.CurrentDirectory,
                                Configuration.GetValue("Paths:Database", "data/database"),
                                Configuration.GetValue("Data:Database:Filename", "data.db")
                            ),
                        };
                        options.UseSqlite(connectionStringBuilder.ToString());
                        break;
#else
                        throw new NotSupportedException("SQLite has been disbled fro Release builds due to migration issues");
#endif
                    default:
                    case "postgres":
                    case "postgresql":
                        var NpgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(Configuration.GetValue("Data:Database:ConnectionString", string.Empty));

                        NpgsqlConnectionStringBuilder.Database = Configuration.GetValue<string>("Data:Database:Database", NpgsqlConnectionStringBuilder.Database);
                        NpgsqlConnectionStringBuilder.Host = Configuration.GetValue<string>("Data:Database:Host", NpgsqlConnectionStringBuilder.Host);
                        NpgsqlConnectionStringBuilder.Port = Configuration.GetValue("Data:Database:Port", NpgsqlConnectionStringBuilder.Port);
                        NpgsqlConnectionStringBuilder.Username = Configuration.GetValue("Data:Database:Username", NpgsqlConnectionStringBuilder.Username);
                        NpgsqlConnectionStringBuilder.Password = Configuration.GetValue("Data:Database:Password", NpgsqlConnectionStringBuilder.Password);

                        if(!string.IsNullOrEmpty(Configuration.GetValue<string>("Data:Database:RequireSSL")))
                            NpgsqlConnectionStringBuilder.SslMode = Configuration.GetValue("Data:Database:RequireSSL", false)
                                ? SslMode.Require
                                : SslMode.Prefer;

                        options.UseNpgsql(NpgsqlConnectionStringBuilder.ToString(), npgsqlOptions =>
                        {
                            npgsqlOptions.RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                            {
                                var caFingerprint = Configuration.GetValue("Data:Database:CAFingerprint", string.Empty);
                                // Perform default validation if a fingerprint was not provided.
                                if (string.IsNullOrEmpty(caFingerprint))
                                    return new X509Certificate2(certificate).Verify();

                                bool hasRecognisedCA = false;
                                var certsLog = new StringBuilder();
                                // Check to see if any CA matches a stored fingerprint
                                foreach (var chainElement in chain.ChainElements)
                                {
                                    certsLog.AppendLine($"- Certificate {chainElement.Certificate.SubjectName}: {chainElement.Certificate.GetCertHashString(HashAlgorithmName.SHA256)}");
                                    if (chainElement.Certificate.GetCertHashString(HashAlgorithmName.SHA256).Equals(caFingerprint, StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasRecognisedCA = true;
                                        break;
                                    }
                                }

                                if (!hasRecognisedCA)
                                {
                                    certsLog.Insert(0, $"Could not verify certificate. did not match. Expecting {caFingerprint}.\n");
                                    _logger.LogCritical(certsLog.ToString());
                                    return false;
                                }

                                if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                                {
                                    // Clear ssl policy error as we now trust the certificate chain.
                                    sslPolicyErrors = SslPolicyErrors.None;
                                }

                                if (sslPolicyErrors != SslPolicyErrors.None)
                                {
                                    _logger.LogCritical($"certificate failed validation: {sslPolicyErrors}");
                                    return false;
                                }

                                return true;
                            });
                        });
                    break;
                }
            });

            // Allow accessing the HTTPContext from services.
            services.AddHttpContextAccessor();

            // Configure Options
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<SendGridEmailServiceOptions>(options => Configuration.GetSection("SendGrid").Bind(options));
            services.Configure<ReminderServiceOptions>(options =>
            {
                options.RemindUserOfUnpaidOrderEveryXDays = Configuration.GetValue<int>("Orders:RemindUserOfUnpaidOrderEveryXDays");
            });
            services.Configure<BankSettings>(Configuration.GetSection("BankTransfer"));
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            // Add application services to DI
            services.AddTransient<ISendGridClient>(c => new SendGridClient(Configuration["SendGrid:ApiKey"]));
            services.AddTransient<IEmailProvider, SendGridEmailProvider>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IReminderService, ReminderService>();
            services.AddTransient<IViewRenderService, ViewRenderService>();
            services.AddTransient<IUserService, EntityFrameworkUserService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IPaymentProvider, Services.Payment.StripeService>();
            services.AddTransient<IPaymentProvider, Services.Payment.BankPaymentProvider>();
            services.AddTransient<Services.Payment.StripeService>();

            // Add background services
            services.AddHostedService<EmailRemindersHostedService>();
            services.AddHostedService<Services.Payment.Stripe.StripeHostedService>();

            // Authentication settings for OIDC integration
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(options => 
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = Configuration.GetValue<string>("Auth:Server");
                options.RequireHttpsMetadata = !Configuration.GetValue("Auth:AllowInsecureHttp", false);

                options.ClientId = Configuration.GetValue<string>("Auth:ClientId");
                options.ClientSecret = Configuration.GetValue<string>("Auth:ClientSecret");

                options.ResponseType = "code id_token";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };

                options.Events.OnUserInformationReceived = NzFursOpenIdConnectEvents.OnUserInformationReceived;
            });

            // Add authorisation policies
            services.AddAuthorization(options => 
            {
                options.AddPolicy("AdminOnly", policyBuilder =>
                {
                    policyBuilder
                        .RequireAuthenticatedUser()
                        .RequireClaim("admin", true.ToString());
                });
            });

            // Finally, configure MVC
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("Testing"))
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-NZ")
            });


            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };
            // Allow fowrwarded headers from IP address in Configuration["ReverseProxy:Networks"]
            foreach (var network in Configuration.GetSection("ReverseProxy:Networks").GetChildren())
            {
                var address = network.Value.Split("/"); // 0.0.0.0/0
                forwardedHeaderOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(address[0]), int.Parse(address[1])));
            }
            app.UseForwardedHeaders(forwardedHeaderOptions);

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());

            app.UseCsp(options =>
            { 
                options.BlockAllMixedContent()
                    .ConnectSources(c => c.None())
                    .FontSources(f => f.Self())
                    .FontSources(f => f.CustomSources("https://fonts.gstatic.com"))
                    .FrameAncestors(f => f.Self())
                    .ImageSources(i => i.Self())
                    .ImageSources(i => i.CustomSources("data:"))
                    .ManifestSources(m => m.None())
                    .MediaSources(m => m.Self())
                    .ObjectSources(o => o.None())
                    .ScriptSources(s => s.Self())
                    .ScriptSources(s => s.CustomSources("https://js.stripe.com"))
                    .StyleSources(s => s.Self())
                    .StyleSources(s => s.CustomSources("https://fonts.googleapis.com"))
                    .StyleSources(s => s.UnsafeInline()) // TODO: Temporary while Stripe payments are refactored, see #117
                    .WorkerSources(w => w.None());
            });

            app.UseSession();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas_without_action",
                    template: "{area:exists}/{controller=Home}/{id:int}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default_without_action",
                    template: "{controller=Home}/{id:int}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Aparently stripe uses a singleton pattern 😟
            Stripe.StripeConfiguration.ApiKey = Configuration.GetValue<string>("Stripe:SecretKey");
        }
    }
}
