using DotNet.Web.Api.Template.Configurations;
using DotNet.Web.Api.Template.Data;
using DotNet.Web.Api.Template.Helpers;
using DotNet.Web.Api.Template.Hubs;
using DotNet.Web.Api.Template.Models;
using DotNet.Web.Api.Template.Models.Auth;
using DotNet.Web.Api.Template.Models.User;
using DotNet.Web.Api.Template.Repositories;
using DotNet.Web.Api.Template.Repositories.Interfaces;
using DotNet.Web.Api.Template.Services;
using DotNet.Web.Api.Template.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://slcedt.netlify.app",
    "https://edt.ecubemedia.lk",
    "https://slcedt.ecubemedia.lk"
};

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Update the DbContext configuration to include the required ServerVersion parameter for MySQL.  
builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
));

// Configure Options for FileStorageSettings
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorageSettings"));

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>("Authenticator");

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // 2FA settings
    options.Tokens.AuthenticatorTokenProvider = "Authenticator";
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 20;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// Add Authentication with JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// Configure Token Lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24); // Set expiration to 24 hours
});

// Register the background service
builder.Services.AddHostedService<DeadlineCheckService>();

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFileUploadRepository, FileUploadRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFileStorageService, LocalStorageService>();
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Configure Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Add client app URL configuration
builder.Services.Configure<ClientAppSettings>(builder.Configuration.GetSection("ClientApp"));

builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DotNet.Web.Api.Template", Version = "v1" });

    // Add JWT Authentication support in Swagger
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
            new string[] {}
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// **Configure Static Files for the custom uploads path**
// Get the FileStorageSettings directly from the service provider here
var fileStorageSettings = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;
string uploadsRootPath = fileStorageSettings.UploadsRootPath;

// Ensure the root uploads directory exists based on appsettings.json
if (!Directory.Exists(uploadsRootPath))
{
    Directory.CreateDirectory(uploadsRootPath);
}

// Ensure the specific folder for meeting minutes exists
string meetingMinutesFullPath = Path.Combine(uploadsRootPath, fileStorageSettings.MeetingMinutesFolderName);
if (!Directory.Exists(meetingMinutesFullPath))
{
    Directory.CreateDirectory(meetingMinutesFullPath);
}

// Serve files from the custom uploads path
// You need a specific RequestPath to access these files via URL.
// For example, if UploadsRootPath is C:\MyAppUploads\SLC-EDT
// and a file is at C:\MyAppUploads\SLC-EDT\meeting-minutes\abc.pdf,
// and RequestPath is "/meeting-files", then it would be accessed at /meeting-files/meeting-minutes/abc.pdf
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRootPath),
    RequestPath = "/uploaded-files" // <-- Choose a URL prefix for direct access
});

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        context.Database.Migrate(); // Apply pending migrations

        // --- Explicit Department Seeding ---
        // Ensure departments exist before seeding users who depend on them.
        List<Department> seededDepartments = new List<Department>(); // Declare a list to store seeded departments
        if (!context.Departments.Any())
        {
            seededDepartments = new List<Department> // Assign to the list
            {
                new Department { Id = Guid.NewGuid(), Name = "Human Resources", ShortName = "HR" },
                new Department { Id = Guid.NewGuid(), Name = "Information Technology", ShortName = "IT" },
                new Department { Id = Guid.NewGuid(), Name = "Finance", ShortName = "FIN" },
                new Department { Id = Guid.NewGuid(), Name = "Operations", ShortName = "OPS" }
            };
            context.Departments.AddRange(seededDepartments); //
            await context.SaveChangesAsync(); // <-- CRUCIAL: Save departments to DB now
            logger.LogInformation("Departments seeded successfully.");
        }
        else
        {
            logger.LogInformation("Departments already exist. Skipping seeding.");
            // If departments already exist, retrieve them to use their IDs
            seededDepartments = await context.Departments.ToListAsync();
        }
        // --- End of Explicit Department Seeding ---

        // Seed roles (if not already handled by HasData in OnModelCreating)
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                logger.LogInformation("Role '{RoleName}' created.", roleName);
            }
        }

        // --- IMPORTANT: Get a Department ID for the Admin user ---
        // Choose one of the seeded departments, e.g., the first one, or a specific one by name
        // Ensure seededDepartments is not empty before trying to access elements.
        Guid adminDepartmentId = Guid.Empty;
        if (seededDepartments.Any())
        {
            adminDepartmentId = seededDepartments.First().Id; // Or pick a specific one like .FirstOrDefault(d => d.ShortName == "IT")?.Id ?? Guid.Empty;
        }

        // Seed Admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            DepartmentId = adminDepartmentId // <--- Set the DepartmentId here!
        };

        var user = await userManager.FindByEmailAsync(adminUser.Email);
        if (user == null)
        {
            // Only attempt to create if a valid DepartmentId was found
            if (adminDepartmentId != Guid.Empty)
            {
                var createAdmin = await userManager.CreateAsync(adminUser, "AdminP@ssw0rd");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Admin user created and assigned 'Admin' role.");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", createAdmin.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Cannot create admin user: No valid Department ID found for seeding.");
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists. Skipping creation.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.MapHealthChecks("/health");

app.Run();
