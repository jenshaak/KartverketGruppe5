using KartverketGruppe5.Services;
using KartverketGruppe5;
using KartverketGruppe5.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bind the API settings from appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Register services and their interfaces
builder.Services.AddHttpClient<IKommuneInfoService, KommuneInfoService>();
builder.Services.AddHttpClient<IStedsnavnService, StedsnavnService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework with MariaDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(10, 5, 9))));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); 
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();