using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Talor_music.Data;
//using Talor_music.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Talor_musicContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Talor_musicContext") ?? throw new InvalidOperationException("Connection string 'Talor_musicContext' not found.")));

//builder.Services.AddDbContext<Talor_musicContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);

builder.Services.AddControllersWithViews();

var app = builder.Build();

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
    pattern: "{controller=Artists}/{action=Index}/{id?}");

app.Run();
