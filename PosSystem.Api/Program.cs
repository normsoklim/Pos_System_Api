using Microsoft.EntityFrameworkCore;
using PosSystem.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using PosSystem.Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using PosSystem.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PosSystem.Api.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PosSystem.Api.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Api.Services;
var builder = WebApplication.CreateBuilder(args);

              // Add services to the container.
              builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:4200" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Default policy for general use
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Add rate limiting
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("fixedwindow", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 20;
    }));

// Add infrastructure services
builder.Services.AddInfrastructureServices();

// Add file service
builder.Services.AddScoped<IFileService, FileService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? "THIS_IS_A_SUPER_SECRET_KEY_123";
var issuer = jwtSettings["Issuer"] ?? "PosSystem";
var audience = jwtSettings["Audience"] ?? "PosSystemUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),
            ClockSkew = TimeSpan.Zero // Default: 5 min
        };
    });

// Add Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    // Define policies for POS system permissions
    options.AddPolicy("CanCreateProduct", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanUpdateProduct", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanDeleteProduct", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("CanCreateCustomer", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanUpdateCustomer", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanDeleteCustomer", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("CanSell", policy =>
        policy.RequireRole("Admin", "Cashier"));
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanManageStock", policy =>
        policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("CanViewSales", policy =>
        policy.RequireRole("Admin", "Manager", "Cashier"));
    options.AddPolicy("CanManageCategories", policy =>
        policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

             // Configure the HTTP request pipeline.
             if (app.Environment.IsDevelopment())
             {
                 // Swagger middleware removed for Postman usage
             }

             // Add request body buffering middleware first to ensure stream can be read multiple times
             app.UseRequestBodyBufferingMiddleware();

             // Add exception handling middleware to catch all exceptions
             app.UseExceptionMiddleware();

             app.UseHttpsRedirection();
             app.UseStaticFiles();

             // Use CORS (should be before authentication)
             app.UseCors();

             // Add security middleware in the correct order
             app.UseSecurityHeadersMiddleware(); // Add security headers
             app.UseAntiForgeryMiddleware(); // Anti-forgery protection
             app.UseRequestSizeLimitingMiddleware(10 * 1024 * 1024); // Request size limiting (10MB)
             app.UseMiddleware<PosSystem.Api.Security.RateLimitingMiddleware>(100, TimeSpan.FromMinutes(1)); // Rate limiting
             app.UseRequestValidationMiddleware(); // Request validation
             
             // Use API key authentication middleware before JWT authentication
             app.UseApiKeyAuthenticationMiddleware(builder.Configuration);
             // Use JWT authentication
             app.UseAuthentication();
             
             // Use authorization
             app.UseAuthorization();
             
             // Add security audit logging after authentication/authorization
             app.UseSecurityAuditMiddleware(); // Security audit logging

             // Map controllers
             app.MapControllers();
             
             // Add route to handle incorrect image paths (redirect /api/product/images/ to /images/)
             app.MapGet("/api/product/images/products/{*path}", (string path) =>
                 Results.Redirect($"/images/products/{path}")).AllowAnonymous();
             
             // Apply pending migrations
             using (var scope = app.Services.CreateScope())
             {
                 var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                 await context.Database.MigrateAsync();
                 
                 // Ensure product images directory exists
                 var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                 var imagesPath = Path.Combine(webHostEnvironment.WebRootPath, "images", "products");
                 if (!Directory.Exists(imagesPath))
                 {
                     Directory.CreateDirectory(imagesPath);
                 }
             }
             
             
             app.Run();
