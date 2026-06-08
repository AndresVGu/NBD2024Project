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

var resetAndSeedOnceEnabled = builder.Configuration.GetValue<bool>("Bootstrap:ResetAndSeedOnce");
var resetIdentityToo = builder.Configuration.GetValue<bool>("Bootstrap:ResetIdentityToo");
var resetAndSeedOverride = builder.Configuration.GetValue<bool>("Bootstrap:ResetAndSeedOverride");
var forceDomainResetOnStartup = builder.Configuration.GetValue<bool>("Bootstrap:ForceDomainResetOnStartup");
var recreateDomainDatabase = builder.Configuration.GetValue<bool>("Bootstrap:RecreateDomainDatabase");
var recreateDomainDatabaseOverride = builder.Configuration.GetValue<bool>("Bootstrap:RecreateDomainDatabaseOverride");
const string domainSeedVersion = "2026-06-08.2";
var resetMarkerPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "App_Data", ".nbd-reset-seed.once")
    : "/home/data/.nbd-reset-seed.once";
var resetAlreadyExecuted = File.Exists(resetMarkerPath);
var shouldRunResetAndSeed = (resetAndSeedOnceEnabled && !resetAlreadyExecuted) || resetAndSeedOverride;
var legacyResetRequested = forceDomainResetOnStartup || shouldRunResetAndSeed;
var recreateMarkerPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "App_Data", ".nbd-recreate-domain.once")
    : "/home/data/.nbd-recreate-domain.once";
var recreateAlreadyExecuted = File.Exists(recreateMarkerPath);

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

var shouldWriteRecreateMarker = false;

// Keep schema up to date and ensure deterministic domain data state for each seed version.
await using (var scope = app.Services.CreateAsyncScope())
{
    var nbdContext = scope.ServiceProvider.GetRequiredService<NBDContext>();
    var currentDomainSeedVersion = await TryGetSeedControlValueAsync(nbdContext, "DomainSeedVersion");
    var recreateBySeedVersion = !string.Equals(currentDomainSeedVersion, domainSeedVersion, StringComparison.Ordinal);
    var shouldRecreateDomainDatabase = legacyResetRequested
        || (recreateDomainDatabase && !recreateAlreadyExecuted)
        || recreateDomainDatabaseOverride
        || recreateBySeedVersion;

    if (shouldRecreateDomainDatabase)
    {
        await nbdContext.Database.EnsureDeletedAsync();
        shouldWriteRecreateMarker = true;
    }

    await nbdContext.Database.MigrateAsync();
}

// Always ensure essential location lookups exist for client/project forms.
await LookupDataInitializer.SeedAsync(app);

// Compact domain seed is idempotent, safe to run in all environments.
NBDInitializer.Seed(app);

await using (var seedScope = app.Services.CreateAsyncScope())
{
    var nbdContext = seedScope.ServiceProvider.GetRequiredService<NBDContext>();
    await EnsureSeedControlTableAsync(nbdContext);
    await SetSeedControlValueAsync(nbdContext, "DomainSeedVersion", domainSeedVersion);
}

// Lightweight identity seed in all environments (roles/users) so login always works.
await ApplicationDbInitializer.SeedAsync(app);

if (shouldRunResetAndSeed)
{
    var markerDir = Path.GetDirectoryName(resetMarkerPath);
    if (!string.IsNullOrWhiteSpace(markerDir))
    {
        Directory.CreateDirectory(markerDir);
    }

    await File.WriteAllTextAsync(
        resetMarkerPath,
        $"One-time reset completed at {DateTime.UtcNow:O}");
}

if (shouldWriteRecreateMarker)
{
    var markerDir = Path.GetDirectoryName(recreateMarkerPath);
    if (!string.IsNullOrWhiteSpace(markerDir))
    {
        Directory.CreateDirectory(markerDir);
    }

    await File.WriteAllTextAsync(
        recreateMarkerPath,
        $"One-time domain recreate completed at {DateTime.UtcNow:O}");
}

app.Run();

static async Task<string> TryGetSeedControlValueAsync(NBDContext context, string key)
{
    try
    {
        await context.Database.OpenConnectionAsync();

        await using var existsCommand = context.Database.GetDbConnection().CreateCommand();
        existsCommand.CommandText = "SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '__SeedControl');";
        var existsResult = await existsCommand.ExecuteScalarAsync();
        var hasSeedControlTable = Convert.ToInt32(existsResult) == 1;
        if (!hasSeedControlTable)
        {
            return null;
        }

        await using var valueCommand = context.Database.GetDbConnection().CreateCommand();
        valueCommand.CommandText = "SELECT \"Value\" FROM \"__SeedControl\" WHERE \"Key\" = $key LIMIT 1;";
        var keyParameter = valueCommand.CreateParameter();
        keyParameter.ParameterName = "$key";
        keyParameter.Value = key;
        valueCommand.Parameters.Add(keyParameter);
        var result = await valueCommand.ExecuteScalarAsync();
        return result?.ToString();
    }
    catch
    {
        return null;
    }
    finally
    {
        await context.Database.CloseConnectionAsync();
    }
}

static async Task EnsureSeedControlTableAsync(NBDContext context)
{
    await context.Database.ExecuteSqlRawAsync(
        "CREATE TABLE IF NOT EXISTS \"__SeedControl\" (\"Key\" TEXT NOT NULL PRIMARY KEY, \"Value\" TEXT NULL);");
}

static async Task SetSeedControlValueAsync(NBDContext context, string key, string value)
{
    await context.Database.ExecuteSqlInterpolatedAsync(
        $"INSERT INTO \"__SeedControl\" (\"Key\", \"Value\") VALUES ({key}, {value}) ON CONFLICT(\"Key\") DO UPDATE SET \"Value\" = excluded.\"Value\";");
}
