using System.Net;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.Mapping;
using InventoryManagement.Core.Services;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Services;
using InventoryManagement.Infrastructure.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Codeblaze.SemanticKernel.Connectors.Ollama;
using InventoryManagement.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure settings configuration

/* localde bu kullanılacak
var infrastructureSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "InventoryManagement.Infrastructure", "appsettings.json");

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Path.GetDirectoryName(infrastructureSettingsPath))
    .AddJsonFile(Path.GetFileName(infrastructureSettingsPath), optional: false, reloadOnChange: true)
    .Build();
  
*/

// azureda bu kullanılacak
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

/*var semanticKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("deepseek-r1:latest", "http://localhost:11434");
builder.Services.AddSingleton(semanticKernel.Build());*/

var configuration = builder.Configuration;

// Database Configuration
var connectionString =configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
     //options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    

// Identity Configuration
builder.Services.AddIdentityCore<User>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
})
.AddRoles<Role>()
.AddRoleManager<RoleManager<Role>>()
.AddSignInManager<SignInManager<User>>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Authorization Service
builder.Services.AddApplicationAuthorizationPolicies();
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanView", policy =>
        policy.RequireClaim("Permission", "CanView"));
    options.AddPolicy("CanCreate", policy =>
        policy.RequireClaim("Permission", "CanCreate"));
    options.AddPolicy("CanEdit", policy =>
        policy.RequireClaim("Permission", "CanEdit"));
    options.AddPolicy("CanDelete", policy =>
        policy.RequireClaim("Permission", "CanDelete"));
});
*/

// Authentication & JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["TokenKey"] ?? throw new InvalidOperationException("Token Key is not configured"))),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Swagger Configuration
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory Management API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                     "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                     "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins(
                "https://www.hizmeyonetim.me",
                "https://hizmeyonetim.me",
                "http://www.hizmeyonetim.me",
                "http://hizmeyonetim.me",
                "http://localhost:5173",                      
                "http://192.168.1.90:5173"                    
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});




// Service Registrations
builder.Services.AddHttpClient();
builder.Services.AddKernel()
    .AddOllamaChatCompletion("deepseek-r1:latest", "http://localhost:11434");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidator<CreateInventoryDto>, CreateInventoryDtoValidator>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

builder.Services.AddHttpClient<OpenRouterService>();
builder.Services.AddHttpClient<DeepSeekService>();

builder.Services.AddScoped<DynamicQueryService>();


var app = builder.Build();  

// Seed Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    //await SeedRolesAsync(serviceProvider);
    //await SeedDepartmentsAsync(serviceProvider);
}

// Configure Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed metodları 
static async Task SeedDepartmentsAsync(IServiceProvider serviceProvider)
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Departments.Any())
    {
        context.Departments.Add(new Department { Name = "İK" });
        context.Departments.Add(new Department { Name = "BT" });
        context.Departments.Add(new Department { Name = "Muhasebe" });

        
        await context.SaveChangesAsync();
    }
    else
    {
        Console.WriteLine("Departmanlar zaten mevcut, ekleme yapılmadı.");
    }

    if (!context.Groups.Any())
    {
        context.Groups.Add(new Group { Name = "Backend",CreatedDate = DateTime.Now,IsActive = true,DepartmentId = 2 });
        await context.SaveChangesAsync();
    }
    else
    {
        Console.WriteLine("Gruplar zaten mevcut");
    }
}

static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    string[] roleNames = { "Admin", "User", "InventoryManager" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var user = await userManager.FindByEmailAsync("arda@arda");
    if (user != null)
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}


app.Run();