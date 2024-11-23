using KartverketGruppe5.Services;
using KartverketGruppe5.Services.Interfaces;
using KartverketGruppe5.API_Models;
using KartverketGruppe5.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Kobler API-innstillingene fra appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Legger service-filene til containeren.
builder.Services.AddScoped<IBildeService, BildeService>();
builder.Services.AddScoped<IBrukerService, BrukerService>();
builder.Services.AddScoped<IFylkeService, FylkeService>();
builder.Services.AddScoped<ILokasjonService, LokasjonService>();
builder.Services.AddScoped<IInnmeldingService, InnmeldingService>();
builder.Services.AddScoped<IKommunePopulateService, KommunePopulateService>();
builder.Services.AddScoped<IKommuneService, KommuneService>();
builder.Services.AddScoped<ISaksbehandlerService, SaksbehandlerService>();

// Legg til services til containeren.
builder.Services.AddControllersWithViews();

// Konfigurer Entity Framework med MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        }
    ));

// Add these lines after builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

// Legger til autorisering for å sikre at kun autoriserte brukere kan få tilgang til bestemte sider.
builder.Services.AddAuthorization();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("KartverketGruppe5");

// Legg til HttpClient som en service
builder.Services.AddHttpClient();

// Alternativt, hvis du trenger en spesifikk HttpClient for KommunePopulateService
// builder.Services.AddHttpClient<KommunePopulateService>();

builder.Services.AddScoped<IPasswordService, PasswordService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Konfigurer HTTP-forespørsler.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();