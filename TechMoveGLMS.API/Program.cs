using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TechMoveGLMS.API.Services;
using TechMoveGLMS_Web.Data;

namespace TechMoveGLMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            // Configure Swagger with JWT support
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TechMove GLMS API",
                    Version = "v1",
                    Description = "Global Logistics Management System API",
                    Contact = new OpenApiContact
                    {
                        Name = "TechMove Support",
                        Email = "support@techmove.com"
                    }
                });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your token"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Configure Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register Services
            builder.Services.AddScoped<IContractRepository, ContractRepository>();
            builder.Services.AddScoped<ServiceRequestFactory>();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<ICurrencyConverter, ExchangeRateApiConverter>();

            // HttpClient for API calls
            builder.Services.AddHttpClient<ICurrencyConverter, ExchangeRateApiConverter>();

            // Configure CORS [allow MVC app to call API]
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebApp", policy =>
                {
                    policy.WithOrigins(
                        "https://localhost:7035",     // MVC HTTPS 
                        "http://localhost:5213",      // MVC HTTP
                        "http://glms-web:8080"        // Docker Compose web service
                    )
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Configure JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Jwt:Key is not configured. Set it via user-secrets:\n" +
                    "  dotnet user-secrets set \"Jwt:Key\" \"<your-32+-char-secret>\"");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Jwt:Issuer is not configured. Set it via user-secrets:\n" +
                    "  dotnet user-secrets set \"Jwt:Issuer\" \"TechMoveGLMS.API\"");

            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Jwt:Audience is not configured. Set it via user-secrets:\n" +
                    "  dotnet user-secrets set \"Jwt:Audience\" \"TechMoveGLMS.Web\"");

            // Validate key length is legit. HMAC-SHA256 requires a key >= 256 bits/32 bytes
            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be at least 32 characters (256 bits) for HMAC-SHA256.");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)  // Default is 5 min; tighter is better
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddAuthorization();
   

            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMove GLMS API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowWebApp");
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
