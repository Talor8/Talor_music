using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Talor_music.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. הגדרת מסד הנתונים
builder.Services.AddDbContext<Talor_musicContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Talor_musicContext") ?? throw new InvalidOperationException("Connection string 'Talor_musicContext' not found.")));

// 2. הגדרת Identity עם תמיכה בתפקידים (Roles)
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // שיניתי ל-false כדי שתוכלי להתחבר בלי לאשר אימייל בבדיקות
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>() // שורה חשובה מאוד!
.AddEntityFrameworkStores<Talor_musicContext>();

builder.Services.AddControllersWithViews();
builder.Services.ConfigureApplicationCookie(options =>
{
    // זה מבטיח שאם משתמש מנסה להיכנס לאזור סגור, הוא יועבר לדף ההתחברות הנכון
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();


// 3. יצירת תפקיד מנהל ומשתמש מנהל ראשון (רץ כשהאפליקציה עולה)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // יצירת תפקיד Admin אם הוא לא קיים
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // יצירת משתמש מנהל (תשני כאן לאימייל שלך)
    var adminEmail = "admin@music.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "Admin123!"); // זו הסיסמה שלך
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. חשוב: קודם אימות ואז הרשאות
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // חייב להוסיף את זה בשביל דפי ההתחברות

app.Run();
