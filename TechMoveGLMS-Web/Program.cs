using Microsoft.AspNetCore.Authentication.Cookies;
using TechMoveGLMS_Web.Services;

namespace TechMoveGLMS_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add HTTP Context Accessor
            builder.Services.AddHttpContextAccessor();

            // Add Session support
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configure Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            // Register API Services
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7089");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Add Dependencies for API services
            builder.Services.AddScoped<IContractApiService, ContractApiService>();
            builder.Services.AddScoped<IClientApiService, ClientApiService>();
            builder.Services.AddScoped<IServiceRequestApiService, ServiceRequestApiService>();

            // Add Dependency for Auth service Api
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}")
                .WithStaticAssets();

            
            app.Run();
        }
    }
}
