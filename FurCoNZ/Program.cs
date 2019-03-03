using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FurCoNZ.DAL;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FurCoNZ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(false);
            var doMigrate = commandLineApplication.Option(
                "--ef-migrate",
                "Apply entity framework migrations and exit",
                CommandOptionType.NoValue);
            var verifyMigrate = commandLineApplication.Option(
                "--ef-migrate-check",
                "Check the status of entity framework migrations",
                CommandOptionType.NoValue);
            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                ExecuteApp(args, doMigrate, verifyMigrate);
                return 0;
            });
            commandLineApplication.Execute(args);
        }

        private static void ExecuteApp(string[] args, CommandOption doMigrate, CommandOption verifyMigrate)
        {
            var webHost = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

            if (verifyMigrate.HasValue() && doMigrate.HasValue())
            {
                Console.WriteLine("ef-migrate and ef-migrate-check are mutually exclusive, select one, and try again");
                Environment.Exit(2);
            }

            using (var scopeServices = webHost.Services.CreateScope())
            {
                using (var context = scopeServices.ServiceProvider.GetService<FurCoNZDbContext>())
                {
                    if (verifyMigrate.HasValue())
                    {
                        Console.WriteLine("Validating status of Entity Framework migrations");
                        var pendingMigrations = context.Database.GetPendingMigrations();
                        var migrations = pendingMigrations as IList<string> ?? pendingMigrations.ToList();
                        if (!migrations.Any())
                        {
                            Console.WriteLine("No pending migratons");
                            Environment.Exit(0);
                        }

                        Console.WriteLine("Pending migratons {0}", migrations.Count());
                        foreach (var migration in migrations)
                        {
                            Console.WriteLine($"\t{migration}");
                        }

                        Environment.Exit(3);
                    }
                    else if (doMigrate.HasValue())
                    {
                        Console.WriteLine("Applyting Entity Framework migrations");

                        context.Database.Migrate();
                        Console.WriteLine("All done, closing app");
                        Environment.Exit(0);
                    }
                }
            }

            // no flags provided, so just run the webhost
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
