using Microsoft.EntityFrameworkCore;
using PosSystem.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Api.Services;
using Microsoft.Extensions.Configuration;
using PosSystem.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PosSystem.Api.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with annotations
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS System API", Version = "v1" });
    c.EnableAnnotations(); // Enable annotations support
    
    // Optional: Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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

// Add JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret not found in configuration");
var jwtExpiryInMinutes = int.Parse(builder.Configuration["Jwt:ExpiryInMinutes"] ?? "60");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PosSystem.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PosSystem.Api";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Recommended for production to avoid issues with token expiration
    };
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // Define policies for product and category management
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManageProducts", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManageCategories", policy => policy.RequireRole("Admin"));
    
    // Add additional security policies
    options.AddPolicy("RequireApiKey", policy =>
        policy.RequireClaim("api_key_authenticated", "true"));
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add exception handling middleware first to catch all exceptions
app.UseExceptionMiddleware();

// Add security middleware in the correct order
app.UseSecurityHeadersMiddleware(); // Add security headers
app.UseAntiForgeryMiddleware(); // Anti-forgery protection
app.UseRequestSizeLimitingMiddleware(10 * 1024 * 1024); // Request size limiting (10MB)
app.UseMiddleware<RateLimitingMiddleware>(100, TimeSpan.FromMinutes(1)); // Rate limiting
app.UseRequestValidationMiddleware(); // Request validation
app.UseApiKeyAuthenticationMiddleware(app.Configuration); // API key authentication
app.UseSecurityAuditMiddleware(); // Security audit logging

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use CORS (should be before authentication)
app.UseCors();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Seed default roles
await SeedService.SeedDefaultRoles(app.Services);

app.Run();
