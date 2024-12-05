using Microsoft.EntityFrameworkCore;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MH_TicketingSystem.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MH_TICKETINGSYSTEM_CONNECTION");
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure the Application Cookie settings
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
//    options.LoginPath = "/Account/Login"; // Set your login path here
//    options.LogoutPath = "/Account/Logout";
//});

builder.Services.AddRazorPages();
builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this
    // for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/", context =>
//{
//    if (!context.User.Identity?.IsAuthenticated ?? true)
//    {
//        context.Response.Redirect("/Identity/Account/Login");
//    }
//    else
//    {
//        context.Response.Redirect("/Home/Index");
//    }
//    return Task.CompletedTask;
//});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<DashboardHub>("/dashboardHub");


app.Run();
