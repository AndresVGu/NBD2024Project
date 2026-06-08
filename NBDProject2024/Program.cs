using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBD2024.Data;
using NBDProject2024.Data;
using NBDProject2024.Utilities;
using NBDProject2024.ViewModels;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Could not connect with connection string.");

// In Azure App Service, keep SQLite under /home so the DB survives restarts/deploys.
var nbdConnectionString = rawConnectionString;
var identityConnectionString = rawConnectionString;
if (!builder.Environment.IsDevelopment())
{
    var nbdBuilder = new SqliteConnectionStringBuilder(rawConnectionString);
    if (!string.IsNullOrWhiteSpace(nbdBuilder.DataSource) && !Path.IsPathRooted(nbdBuilder.DataSource))
    {
        var persistentDataPath = "/home/data";
        Directory.CreateDirectory(persistentDataPath);
        var nbdFileName = Path.GetFileName(nbdBuilder.DataSource);
        var identityFileName = Path.GetFileNameWithoutExtension(nbdFileName) + "_identity" + Path.GetExtension(nbdFileName);

        nbdBuilder.DataSource = Path.Combine(persistentDataPath, nbdFileName);
        nbdConnectionString = nbdBuilder.ToString();

        var identityBuilder = new SqliteConnectionStringBuilder(rawConnectionString)
        {
            DataSource = Path.Combine(persistentDataPath, identityFileName)
        };
        identityConnectionString = identityBuilder.ToString();
    }
}
else
{
    var nbdBuilder = new SqliteConnectionStringBuilder(rawConnectionString);
    var nbdFileName = Path.GetFileNameWithoutExtension(nbdBuilder.DataSource);
    var nbdFileExt = Path.GetExtension(nbdBuilder.DataSource);
    var identityBuilder = new SqliteConnectionStringBuilder(rawConnectionString)
    {
        DataSource = nbdFileName + "_identity" + nbdFileExt
    };
    identityConnectionString = identityBuilder.ToString();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(identityConnectionString));

//Context:
builder.Services.AddDbContext<NBDContext>(options =>
    options.UseSqlite(nbdConnectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.LoginPath = "/Identity/Account/Login/";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});


builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddResponseCaching();
builder.Services.AddOutputCache();


builder.Services.AddControllersWithViews();

//For email Service configuration
builder.Services.AddSingleton<IEmailConfiguration>(builder.Configuration
    .GetSection("EmailConfiguration").Get<EmailConfiguration>());

//For the Identity System
builder.Services.AddTransient<IEmailSender, EmailSender>();

//Email With addes methods for production use
builder.Services.AddTransient<IMyEmailSender, MyEmailSender>();




//Commented ou to avoid issues with the Identity System
//builder.Services.AddAuthorization(options =>
//{
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //.RequireAuthenticatedUser()
    //.Build();
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 7;
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={durationInSeconds}";
    }
});

app.UseRouting();

app.UseResponseCaching();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Keep schema up to date and baseline legacy databases that predate EF migration history.
await using (var scope = app.Services.CreateAsyncScope())
{
    var nbdContext = scope.ServiceProvider.GetRequiredService<NBDContext>();
    await nbdContext.Database.OpenConnectionAsync();

    const string initialMigrationId = "20240403023052_Initial";
    const string productVersion = "8.0.17";

    bool hasEmployeesTable;
    await using (var checkCommand = nbdContext.Database.GetDbConnection().CreateCommand())
    {
        checkCommand.CommandText = "SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Employees');";
        var result = await checkCommand.ExecuteScalarAsync();
        hasEmployeesTable = Convert.ToInt32(result) == 1;
    }

    if (hasEmployeesTable)
    {
        await nbdContext.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL);");

        await nbdContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({initialMigrationId}, {productVersion});");
    }

    await nbdContext.Database.CloseConnectionAsync();

    var nbdPendingMigrations = await nbdContext.Database.GetPendingMigrationsAsync();
    if (nbdPendingMigrations.Any())
    {
        await nbdContext.Database.MigrateAsync();
    }
}

// Heavy domain seed only in local development to avoid slow production cold starts.
if (app.Environment.IsDevelopment())
{
    NBDInitializer.Seed(app);
}

// Always ensure essential location lookups exist for client/project forms.
await LookupDataInitializer.SeedAsync(app);

// Lightweight identity seed in all environments (roles/users) so login always works.
await ApplicationDbInitializer.SeedAsync(app);

app.Run();
