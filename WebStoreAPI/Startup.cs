using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System;
using WebStoreAPI.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using WebStoreAPI.Middlewares;
using WebStoreAPI.Services;

namespace WebStoreAPI
{
    public class Startup
    {
        private readonly IConfiguration _appConfiguration;

        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("adminsettings.json")
                .AddJsonFile("companysettings.json")
                .AddConfiguration(configuration);

            _appConfiguration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(_appConfiguration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationContext>(provider => provider.GetService<ApplicationContext>());

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IConfiguration>(provider => _appConfiguration);

            services.AddHttpContextAccessor();

            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddMemoryCache();

            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<EmailService>();
            
            services.AddTransient<ProductsService>();
            services.AddTransient<ProductsCartService>();
            services.AddTransient<AccountService>();
            services.AddTransient<CategoriesService>();
            services.AddTransient<ImageService>();
            services.AddTransient<OpenStoreRequestService>();
            services.AddTransient<RolesService>();
            services.AddTransient<StoreService>();
            services.AddTransient<TagsService>();
            services.AddTransient<UsersService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
        }
    }
}