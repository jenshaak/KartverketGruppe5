using KartverketGruppe5.Services;
using KartverketGruppe5;
using KartverketGruppe5.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Bind the API settings from appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Register services and their interfaces
builder.Services.AddHttpClient<IKommuneInfoService, KommuneInfoService>();
builder.Services.AddHttpClient<IStedsnavnService, StedsnavnService>();

// Add this with your other service registrations
builder.Services.AddScoped<BrukerService>();
builder.Services.AddScoped<GeoChangeService>();
builder.Services.AddScoped<LokasjonService>();
builder.Services.AddScoped<InnmeldingService>();
builder.Services.AddScoped<KommunePopulateService>();
builder.Services.AddScoped<KommuneService>();
builder.Services.AddScoped<SaksbehandlerService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework with MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(10, 5, 9))));

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

builder.Services.AddAuthorization();

// Legg til denne konfigurasjonen før andre services
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("KartverketGruppe5");

var app = builder.Build();
app.UseDeveloperExceptionPage();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
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