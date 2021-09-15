using CreatureDisplayCreator.Pages;
using Stylet;
using StyletIoC;
using Microsoft.Extensions.DependencyInjection;
using CreatureDisplayCreator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using CreatureDisplayCreator.Infrastructure.Models;

namespace CreatureDisplayCreator
{
    public class Bootstrapper : MicrosoftDependencyInjectionBootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddTransient<ShellView>();
            services.AddTransient<ShellViewModel>();

            services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(
                configuration.GetConnectionString("db"),
                new MySqlServerVersion(new Version(8, 0, 25))
            ));          
        }
    }
}
