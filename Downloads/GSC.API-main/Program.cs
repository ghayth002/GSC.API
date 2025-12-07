using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "GsC-Super-Secret-Key-For-Development-Only-Change-In-Production";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "GsC.API",
        ValidAudience = jwtSettings["Audience"] ?? "GsC.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(googleOptions =>
{
    var googleAuthSettings = builder.Configuration.GetSection("Authentication:Google");
    googleOptions.ClientId = googleAuthSettings["ClientId"];
    googleOptions.ClientSecret = googleAuthSettings["ClientSecret"];
    googleOptions.CallbackPath = "/api/auth/google-callback";
    googleOptions.SaveTokens = true;
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure email settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGscBusinessService, GscBusinessService>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Add Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GsC API - User Management", 
        Version = "v1",
        Description = "Gestion de la Sous-traitance du Catering - User Management API"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GsC API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Keep the original WeatherForecast endpoint for testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    
    // Ensure database is created
    // context.Database.EnsureCreated(); // Commented out - database already exists
    
    // Seed initial data
    await SeedInitialData(userManager, roleManager, context);
}

app.Run();

async Task SeedInitialData(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
{
    // Create default roles - Only Administrator and Fournisseur
    string[] roles = ["Administrator", "Fournisseur"];
    
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role 
            { 
                Name = roleName, 
                Description = $"Default {roleName} role" 
            });
        }
    }

    // Create default admin user
    var adminEmail = "admin@gsc.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            PictureUrl = null
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }

    // Créer un fournisseur par défaut
    await CreateDefaultFournisseur(userManager, roleManager, context);

    // Create default permissions
    if (!context.Permissions.Any())
    {
        var permissions = new[]
        {
            new Permission { Name = "Users.Create", Description = "Create users", Module = "UserManagement" },
            new Permission { Name = "Users.Read", Description = "View users", Module = "UserManagement" },
            new Permission { Name = "Users.Update", Description = "Update users", Module = "UserManagement" },
            new Permission { Name = "Users.Delete", Description = "Delete users", Module = "UserManagement" },
            new Permission { Name = "Roles.Manage", Description = "Manage roles", Module = "UserManagement" }
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // Assign all permissions to Administrator role
        var adminRole = await roleManager.FindByNameAsync("Administrator");
        if (adminRole != null)
        {
            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    GrantedAt = DateTime.UtcNow
                };
                context.RolePermissions.Add(rolePermission);
            }
            await context.SaveChangesAsync();
        }
    }
}

async Task CreateDefaultFournisseur(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
{
    var fournisseurEmail = "demo.fournisseur@gsc.com";
    var fournisseurPassword = "Fournisseur123!";
    
    var existingFournisseurUser = await userManager.FindByEmailAsync(fournisseurEmail);
    
    if (existingFournisseurUser == null)
    {
        // Créer l'utilisateur fournisseur
        var fournisseurUser = new User
        {
            UserName = fournisseurEmail,
            Email = fournisseurEmail,
            FirstName = "Demo",
            LastName = "Fournisseur",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(fournisseurUser, fournisseurPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(fournisseurUser, "Fournisseur");

            // Créer le profil fournisseur
            var fournisseur = new Fournisseur
            {
                UserId = fournisseurUser.Id,
                CompanyName = "Demo Catering Solutions",
                Address = "123 Avenue de la Restauration, 75001 Paris, France",
                Phone = "+33 1 23 45 67 89",
                ContactEmail = fournisseurEmail,
                ContactPerson = "Demo Fournisseur",
                Siret = "12345678901234",
                NumeroTVA = "FR12345678901",
                Specialites = "Restauration aérienne, menus gastronomiques, plateaux repas",
                IsActive = true,
                IsVerified = true, // Pré-vérifié pour les tests
                CreatedAt = DateTime.UtcNow
            };

            context.Fournisseurs.Add(fournisseur);
            await context.SaveChangesAsync();

            Console.WriteLine("🎉 FOURNISSEUR PAR DÉFAUT CRÉÉ !");
            Console.WriteLine($"📧 Email: {fournisseurEmail}");
            Console.WriteLine($"🔐 Mot de passe: {fournisseurPassword}");
            Console.WriteLine($"🏢 Entreprise: {fournisseur.CompanyName}");
            Console.WriteLine($"📍 Adresse: {fournisseur.Address}");
            Console.WriteLine("✅ Statut: Actif et Vérifié");
        }
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
